using System.Collections;
using UnityEngine;
using UHFPS.Runtime;
using UHFPS.Scriptable;

namespace UHFPS.Custom
{
    /// <summary>
    /// Trigger genérico y reutilizable que activa/desactiva objetos y opcionalmente
    /// muestra un diálogo cuando el jugador entra o interactúa.
    /// El jugador queda congelado durante toda la duración del diálogo.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerActivadorGenerico : MonoBehaviour, IInteractStart
    {
        public enum TriggerModo { Trigger, Interact, Event }
        public enum AccionObjeto { Activar, Desactivar, Alternar }

        [Header("Modo de Activación")]
        [Tooltip("Cómo se disparará este trigger:\n• Trigger: Al pisar el collider.\n• Interact: Al interactuar (click/tecla).\n• Event: Llamando DispararDesdeEvento() desde un UnityEvent.")]
        public TriggerModo modo = TriggerModo.Trigger;

        [Tooltip("¿Solo se puede usar una vez?")]
        public bool unaSolaVez = true;

        [Header("Objetos a Modificar")]
        [Tooltip("Lista de GameObjects que se verán afectados al disparar el trigger.")]
        public GameObject[] objetosAfectados;

        [Tooltip("Qué hacer con los objetos:\n• Activar: SetActive(true)\n• Desactivar: SetActive(false)\n• Alternar: Invierte el estado actual de cada objeto")]
        public AccionObjeto accion = AccionObjeto.Activar;

        [Tooltip("Segundos de espera antes de modificar los objetos (0 = inmediato).")]
        [Min(0f)]
        public float delayActivacion = 0f;

        [Header("Diálogo (Opcional)")]
        [Tooltip("Si asignás un DialogueAsset aquí, se mostrará un diálogo al disparar el trigger.")]
        public DialogueAsset dialogo;

        [Tooltip("AudioSource del jugador para reproducir el audio del diálogo.")]
        public AudioSource audioSourcePJ;

        [Tooltip("¿Debe dispararse el diálogo ANTES de modificar los objetos? Si está desactivado, el diálogo se dispara DESPUÉS.")]
        public bool dialogoAntes = true;

        [Tooltip("Segundos de espera después de que termina el diálogo antes de ejecutar la siguiente acción.")]
        [Min(0f)]
        public float delayPostDialogo = 0f;

        [Tooltip("Si está activo, muestra el cursor mientras el jugador está congelado por el diálogo.")]
        public bool mostrarCursorDuranteDialogo = false;

        [Header("Audio Ambiente (Opcional)")]
        [Tooltip("AudioSource opcional que sonará al disparar el trigger (efecto de sonido, ambiente, etc.).")]
        public AudioSource audioAlDisparar;

        // ─── Estado interno ───
        private bool yaActivado = false;
        private DialogueTrigger triggerDialogo;

        private void Start()
        {
            triggerDialogo = CrearTriggerOculto(dialogo, "Dialogo");
        }

        // ─── Detección ───
        private void OnTriggerEnter(Collider other)
        {
            if (modo != TriggerModo.Trigger) return;
            if (unaSolaVez && yaActivado) return;
            if (!other.CompareTag("Player")) return;

            Disparar();
        }

        public void InteractStart()
        {
            if (modo != TriggerModo.Interact) return;
            if (unaSolaVez && yaActivado) return;

            Disparar();
        }

        /// <summary>
        /// Llamá este método desde un UnityEvent externo (botón, otro script, etc.)
        /// cuando el modo es "Event".
        /// </summary>
        public void DispararDesdeEvento()
        {
            if (modo != TriggerModo.Event) return;
            if (unaSolaVez && yaActivado) return;

            Disparar();
        }

        // ─── Lógica principal ───
        private void Disparar()
        {
            yaActivado = true;

            // Audio ambiente
            if (audioAlDisparar != null)
                audioAlDisparar.Play();

            StartCoroutine(RutinaDeDisparo());

            Debug.Log($"[TriggerActivador] '{gameObject.name}' disparado.");
        }

        private IEnumerator RutinaDeDisparo()
        {
            if (dialogoAntes && triggerDialogo != null)
            {
                // ── Diálogo primero, objetos después ──

                // Congelar al jugador
                PlayerPresenceManager.Instance.FreezePlayer(true, mostrarCursorDuranteDialogo);
                Debug.Log("[TriggerActivador] Jugador congelado para diálogo.");

                // Disparar diálogo
                triggerDialogo.TriggerDialogue();

                // Esperar la duración completa del diálogo
                float duracionDialogo = ObtenerDuracionDialogo(dialogo);
                if (duracionDialogo > 0f)
                    yield return new WaitForSeconds(duracionDialogo);

                // Delay extra post-diálogo
                if (delayPostDialogo > 0f)
                    yield return new WaitForSeconds(delayPostDialogo);

                // Liberar al jugador
                PlayerPresenceManager.Instance.FreezePlayer(false);
                Debug.Log("[TriggerActivador] Jugador liberado tras diálogo.");

                // Delay antes de activar objetos
                if (delayActivacion > 0f)
                    yield return new WaitForSeconds(delayActivacion);

                EjecutarAccionObjetos();
            }
            else
            {
                // ── Objetos primero (o no hay diálogo) ──

                // Delay antes de activar objetos
                if (delayActivacion > 0f)
                    yield return new WaitForSeconds(delayActivacion);

                EjecutarAccionObjetos();

                // Si hay diálogo después
                if (!dialogoAntes && triggerDialogo != null)
                {
                    // Congelar al jugador
                    PlayerPresenceManager.Instance.FreezePlayer(true, mostrarCursorDuranteDialogo);
                    Debug.Log("[TriggerActivador] Jugador congelado para diálogo.");

                    // Delay pre-diálogo
                    if (delayPostDialogo > 0f)
                        yield return new WaitForSeconds(delayPostDialogo);

                    // Disparar diálogo
                    triggerDialogo.TriggerDialogue();

                    // Esperar la duración completa del diálogo
                    float duracionDialogo = ObtenerDuracionDialogo(dialogo);
                    if (duracionDialogo > 0f)
                        yield return new WaitForSeconds(duracionDialogo);

                    // Liberar al jugador
                    PlayerPresenceManager.Instance.FreezePlayer(false);
                    Debug.Log("[TriggerActivador] Jugador liberado tras diálogo.");
                }
            }
        }

        private void EjecutarAccionObjetos()
        {
            if (objetosAfectados == null) return;

            foreach (var obj in objetosAfectados)
            {
                if (obj == null) continue;

                switch (accion)
                {
                    case AccionObjeto.Activar:
                        obj.SetActive(true);
                        break;
                    case AccionObjeto.Desactivar:
                        obj.SetActive(false);
                        break;
                    case AccionObjeto.Alternar:
                        obj.SetActive(!obj.activeSelf);
                        break;
                }
                Debug.Log($"[TriggerActivador] Objeto '{obj.name}' → {(obj.activeSelf ? "ACTIVO" : "INACTIVO")}");
            }
        }

        // ─── Utilidades ───
        private DialogueTrigger CrearTriggerOculto(DialogueAsset asset, string nombre)
        {
            if (asset == null) return null;

            GameObject go = new GameObject($"HiddenDialogue_{nombre}");
            go.transform.SetParent(transform);

            DialogueTrigger dt = go.AddComponent<DialogueTrigger>();
            dt.Dialogue = asset;
            dt.DialogueType = DialogueTrigger.DialogueTypeEnum.Local;
            dt.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;

            if (audioSourcePJ != null)
            {
                dt.DialogueAudio = audioSourcePJ;
            }
            else
            {
                Debug.LogWarning($"[TriggerActivador] '{nombre}': No hay AudioSource del PJ asignado. El diálogo se mostrará sin audio.");
            }

            return dt;
        }

        private float ObtenerDuracionDialogo(DialogueAsset asset)
        {
            if (asset == null) return 0f;
            float duracion = 0f;
            foreach (var d in asset.Dialogues)
            {
                if (d.DialogueAudio != null) duracion += d.DialogueAudio.length;
            }
            return duracion > 0f ? duracion + 0.1f : 0f;
        }

        // ─── Gizmo visual en el editor ───
        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col != null && col.isTrigger)
            {
                Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f); // Azul para diferenciarlo
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, col.bounds.size / transform.lossyScale.x);
            }
        }
    }
}
