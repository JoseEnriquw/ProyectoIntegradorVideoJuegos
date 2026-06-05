using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UHFPS.Runtime; // Requerido para integrarse con UHFPS (Ultimate Horror FPS Template)

/// <summary>
/// Trigger premium y altamente personalizable que ejecuta eventos de Unity (UnityEvents)
/// cuando un personaje enemigo (NPC) entra o sale de su área de colisión.
/// Permite filtrar por Tag, Componente y Capa física (Layer).
/// </summary>
[RequireComponent(typeof(Collider))]
    public class TriggerPorEnemigo : MonoBehaviour
    {
        [Header("Filtros de Detección")]
        [Tooltip("Si está activo, verifica que el objeto tenga un Tag específico.")]
        public bool filtrarPorTag = true;
        [Tooltip("El Tag que debe tener el enemigo (ej: 'NPC', 'Enemy').")]
        public string tagEnemigo = "Enemy";

        [Tooltip("Si está activo, verifica que el objeto tenga un componente específico (ej: NPCStateMachine).")]
        public bool filtrarPorComponente = true;
        [Tooltip("Nombre de la clase/componente que define al enemigo (dejar vacío para buscar 'NPCStateMachine' por defecto).")]
        public string nombreComponente = "NPCStateMachine";

        [Tooltip("Si está activo, verifica que el objeto pertenezca a ciertas capas físicas.")]
        public bool filtrarPorCapa = false;
        [Tooltip("Máscara de capas físicas válidas para la detección.")]
        public LayerMask capasEnemigo;

        [Header("Configuración de Disparo")]
        [Tooltip("¿El trigger solo debe dispararse una única vez en toda la partida?")]
        public bool unaSolaVez = true;

        [Header("Eventos al Entrar")]
        [Tooltip("Eventos que se ejecutan cuando el enemigo entra al trigger.")]
        public UnityEvent AlEntrarEnemigo;
        [Tooltip("Eventos que se ejecutan al entrar, pasando la referencia del GameObject del enemigo.")]
        public UnityEvent<GameObject> AlEntrarEnemigoConObjeto;

        [Header("Eventos al Salir")]
        [Tooltip("Eventos que se ejecutan cuando el enemigo sale del trigger.")]
        public UnityEvent AlSalirEnemigo;
        [Tooltip("Eventos que se ejecutan al salir, pasando la referencia del GameObject del enemigo.")]
        public UnityEvent<GameObject> AlSalirEnemigoConObjeto;

        // Estado interno
        private bool yaSeDisparoEnter = false;
        private bool yaSeDisparoExit = false;

        private void Reset()
        {
            // Se ejecuta al añadir el componente en el Editor de Unity
            ConfigurarFisicas();
        }

        private void Awake()
        {
            ConfigurarFisicas();
        }

        private void ConfigurarFisicas()
        {
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.Log($"[TriggerPorEnemigo] El collider en '{gameObject.name}' fue configurado como Trigger.");
            }

            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;
                Debug.Log($"[TriggerPorEnemigo] Se añadió automáticamente un Rigidbody Kinematic a '{gameObject.name}' para asegurar la detección de colisiones.");
            }
            else
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!enabled) return;
            if (unaSolaVez && yaSeDisparoEnter) return;

            if (EsEnemigoValido(other))
            {
                yaSeDisparoEnter = true;
                
                AlEntrarEnemigo?.Invoke();
                AlEntrarEnemigoConObjeto?.Invoke(other.gameObject);

                Debug.Log($"[TriggerPorEnemigo] Enemigo '{other.name}' ENTRÓ a '{gameObject.name}' y disparó los eventos.");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!enabled) return;
            if (unaSolaVez && yaSeDisparoExit) return;

            if (EsEnemigoValido(other))
            {
                yaSeDisparoExit = true;

                AlSalirEnemigo?.Invoke();
                AlSalirEnemigoConObjeto?.Invoke(other.gameObject);

                Debug.Log($"[TriggerPorEnemigo] Enemigo '{other.name}' SALIÓ de '{gameObject.name}' y disparó los eventos.");
            }
        }

        /// <summary>
        /// Evalúa si el objeto colisionado cumple con todos los filtros de enemigo configurados.
        /// </summary>
        private bool EsEnemigoValido(Collider other)
        {
            if (other == null) return false;

            // 1. Filtrar por Tag (busca en el propio collider, en sus padres y en el root)
            if (filtrarPorTag)
            {
                bool tagCoincide = other.CompareTag(tagEnemigo) || 
                                   (other.transform.parent != null && other.transform.parent.CompareTag(tagEnemigo)) ||
                                   other.transform.root.CompareTag(tagEnemigo);

                if (!tagCoincide)
                    return false;
            }

            // 2. Filtrar por Capa Física
            if (filtrarPorCapa)
            {
                // Comparamos el bitwise layer mask
                if (((1 << other.gameObject.layer) & capasEnemigo) == 0)
                    return false;
            }

            // 3. Filtrar por Componente
            if (filtrarPorComponente)
            {
                string compName = string.IsNullOrEmpty(nombreComponente) ? "NPCStateMachine" : nombreComponente;
                bool encontrado = false;

                // Buscar en el propio objeto
                foreach (Component comp in other.GetComponents<Component>())
                {
                    if (comp != null && comp.GetType().Name == compName)
                    {
                        encontrado = true;
                        break;
                    }
                }

                // Buscar en los padres (por si el collider está en las extremidades del personaje)
                if (!encontrado)
                {
                    foreach (Component comp in other.GetComponentsInParent<Component>())
                    {
                        if (comp != null && comp.GetType().Name == compName)
                        {
                            encontrado = true;
                            break;
                        }
                    }
                }

                // Buscar en los hijos (por si el script está en un objeto secundario)
                if (!encontrado)
                {
                    foreach (Component comp in other.GetComponentsInChildren<Component>())
                    {
                        if (comp != null && comp.GetType().Name == compName)
                        {
                            encontrado = true;
                            break;
                        }
                    }
                }

                if (!encontrado)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Método público para reiniciar el estado del trigger si se requiere volver a usar
        /// cuando 'unaSolaVez' está activo.
        /// </summary>
        public void ReiniciarTrigger()
        {
            yaSeDisparoEnter = false;
            yaSeDisparoExit = false;
            Debug.Log($"[TriggerPorEnemigo] Trigger '{gameObject.name}' reiniciado.");
        }

        // ─── Dibujo de Gizmos en el Editor ───
        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col == null) return;

            // Naranja rojizo translúcido para representar peligro/enemigos
            Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.7f);
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(sphere.center, sphere.radius);
                Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.7f);
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
            else
            {
                // Caso genérico para CapsuleCollider u otros usando los límites locales
                Vector3 center = transform.InverseTransformPoint(col.bounds.center);
                Vector3 size = transform.InverseTransformVector(col.bounds.size);
                
                Gizmos.DrawCube(center, size);
                Gizmos.color = new Color(1f, 0.35f, 0.1f, 0.7f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }

