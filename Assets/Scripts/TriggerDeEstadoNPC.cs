using System.Collections;
using UnityEngine;
using UHFPS.Runtime; // Las herramientas del paquete

namespace UHFPS.Custom
{
    [RequireComponent(typeof(Collider))]
    public class TriggerDeEstadoNPC : MonoBehaviour, IInteractStart
    {
        public enum TriggerTypeEnum { Trigger, Interact, Event }

        [Header("Configuracion del Trigger")]
        [Tooltip("Asigna aquí el objeto raíz de tu NPC (el que tiene el NPC State Machine)")]
        public NPCStateMachine npcObjetivo;

        [Tooltip("El 'State Key' del estado al que saltará el NPC. Ej: PersecucionAI, PatrullajeAI, etc.")]
        public string stateKeyAForzar = "PersecucionAI";

        [Tooltip("Cómo se disparará este evento (Pisándolo, Interactuando, o Manualmente)")]
        public TriggerTypeEnum tipoDeTrigger = TriggerTypeEnum.Trigger;

        [Tooltip("¿El trigger solo debe funcionar la primera vez que el jugador lo use?")]
        public bool activarUnaSolaVez = true;

        [Tooltip("Audio opcional que sonará al mismo tiempo que el NPC cambia de estado (Ej: Graznido de cuervo)")]
        public AudioSource audioAlActivar;

        [Header("Freeze del Jugador")]
        [Tooltip("¿Debe el trigger congelar al jugador temporalmente?")]
        public bool congelarJugador = false;

        [Tooltip("Tiempo en segundos que el jugador permanecerá congelado")]
        [Min(0f)]
        public float tiempoDeFreeze = 2f;

        [Tooltip("Si está activo, muestra el cursor mientras el jugador está congelado")]
        public bool mostrarCursorDuranteFreeze = false;

        private bool yaActivado = false;
        private Coroutine freezeCoroutine;

        private void OnTriggerEnter(Collider other)
        {
            if (tipoDeTrigger != TriggerTypeEnum.Trigger) return;
            if (activarUnaSolaVez && yaActivado) return;

            // Revisamos nativamente que sea el Jugador quien lo toca (UHFPS usa la tag Player)
            if (other.CompareTag("Player"))
            {
                DispararEvento();
            }
        }

        public void InteractStart()
        {
            if (tipoDeTrigger != TriggerTypeEnum.Interact) return;
            if (activarUnaSolaVez && yaActivado) return;

            DispararEvento();
        }

        // Para ser llamado desde un botón u otro UnityEvent si eliges la opción "Event"
        public void DispararEventoDesdeAfuera()
        {
            if (tipoDeTrigger != TriggerTypeEnum.Event) return;
            if (activarUnaSolaVez && yaActivado) return;

            DispararEvento();
        }

        private void DispararEvento()
        {
            if (npcObjetivo != null)
            {
                npcObjetivo.ChangeState(stateKeyAForzar);
                yaActivado = true;

                // Si hay un audio asignado, lo reproducimos
                if (audioAlActivar != null)
                {
                    audioAlActivar.Play();
                }

                // Congelar al jugador si está configurado
                if (congelarJugador && tiempoDeFreeze > 0f)
                {
                    if (freezeCoroutine != null)
                        StopCoroutine(freezeCoroutine);

                    freezeCoroutine = StartCoroutine(RutinaDeFreeze());
                }

                Debug.Log($"[Trigger] El NPC {npcObjetivo.name} ha sido forzado al estado {stateKeyAForzar}.");
            }
            else
            {
                Debug.LogWarning("Se intentó disparar el estado del NPC pero no hay ninguno asignado.");
            }
        }

        /// <summary>
        /// Congela al jugador durante <see cref="tiempoDeFreeze"/> segundos
        /// usando la API oficial de UHFPS (<see cref="PlayerPresenceManager.FreezePlayer"/>).
        /// </summary>
        private IEnumerator RutinaDeFreeze()
        {
            PlayerPresenceManager.Instance.FreezePlayer(true, mostrarCursorDuranteFreeze);
            Debug.Log($"[Trigger] Jugador congelado por {tiempoDeFreeze} segundos.");

            yield return new WaitForSeconds(tiempoDeFreeze);

            PlayerPresenceManager.Instance.FreezePlayer(false);
            Debug.Log("[Trigger] Jugador liberado.");
            freezeCoroutine = null;
        }

        // Dibuja una cajita verde en la escena para que no lo pierdas de vista al editar
        private void OnDrawGizmos()
        {
            Collider col = GetComponent<Collider>();
            if (col != null && col.isTrigger)
            {
                Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.4f);
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, col.bounds.size / transform.lossyScale.x);
            }
        }
    }
}
