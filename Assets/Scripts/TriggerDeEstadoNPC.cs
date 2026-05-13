using System.Collections;
using UnityEngine;
using UHFPS.Runtime; // Las herramientas del paquete
using UHFPS.Scriptable; // Para DialogueAsset

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

        [Tooltip("Tiempo extra a esperar después del 1er diálogo (ej. para que el nene se dé vuelta y arranque a correr)")]
        [Min(0f)]
        public float tiempoDeFreeze = 2f;

        [Tooltip("Si está activo, muestra el cursor mientras el jugador está congelado")]
        public bool mostrarCursorDuranteFreeze = false;

        [Header("Audios del Jugador (PJ)")]
        [Tooltip("AudioSource que pertenece al jugador (PJ) para emitir sus diálogos.")]
        public AudioSource audioSourcePJ;

        [Tooltip("Primer diálogo: se reproduce apenas se congela al jugador.")]
        public DialogueAsset primerDialogo;

        [Tooltip("Segundo diálogo: se reproduce justo antes de liberar al jugador.")]
        public DialogueAsset segundoDialogo;

        private bool yaActivado = false;
        private Coroutine freezeCoroutine;

        private DialogueTrigger triggerPrimerDialogo;
        private DialogueTrigger triggerSegundoDialogo;

        private void Start()
        {
            triggerPrimerDialogo = CrearTriggerOculto(primerDialogo, "PrimerDialogo");
            triggerSegundoDialogo = CrearTriggerOculto(segundoDialogo, "SegundoDialogo");
        }

        private DialogueTrigger CrearTriggerOculto(DialogueAsset asset, string nombre)
        {
            if (asset == null) return null;

            GameObject go = new GameObject($"HiddenDialogue_{nombre}");
            go.transform.SetParent(transform);
            
            DialogueTrigger dt = go.AddComponent<DialogueTrigger>();
            dt.Dialogue = asset;
            dt.DialogueAudio = audioSourcePJ;
            dt.DialogueType = DialogueTrigger.DialogueTypeEnum.Local;
            dt.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;
            
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
            // Agregamos un pequeño margen para asegurar que termine bien
            return duracion > 0f ? duracion + 0.1f : 0f;
        }

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
                yaActivado = true;

                if (congelarJugador)
                {
                    if (freezeCoroutine != null)
                        StopCoroutine(freezeCoroutine);

                    freezeCoroutine = StartCoroutine(RutinaDeFreeze());
                }
                else
                {
                    EjecutarCambioDeEstado();
                }
            }
            else
            {
                Debug.LogWarning("Se intentó disparar el estado del NPC pero no hay ninguno asignado.");
            }
        }

        private void EjecutarCambioDeEstado()
        {
            npcObjetivo.ChangeState(stateKeyAForzar);
            
            // Si hay un audio asignado (del entorno o del NPC), lo reproducimos
            if (audioAlActivar != null)
            {
                audioAlActivar.Play();
            }

            Debug.Log($"[Trigger] El NPC {npcObjetivo.name} ha sido forzado al estado {stateKeyAForzar}.");
        }

        /// <summary>
        /// Congela al jugador, maneja los diálogos del PJ y retrasa el cambio de estado del NPC.
        /// </summary>
        private IEnumerator RutinaDeFreeze()
        {
            PlayerPresenceManager.Instance.FreezePlayer(true, mostrarCursorDuranteFreeze);
            Debug.Log($"[Trigger] Jugador congelado. Iniciando secuencia de audios y animación.");

            // Acomodamos la cámara del jugador para que mire al NPC suavemente en medio segundo
            if (npcObjetivo != null)
            {
                PlayerPresenceManager.Instance.LookController.LerpRotation(npcObjetivo.transform, 0.5f, false);
            }

            // 1. Apenas se freezea, tiramos el primer diálogo
            if (triggerPrimerDialogo != null)
            {
                triggerPrimerDialogo.TriggerDialogue();
                // Esperamos a que termine el primer diálogo
                yield return new WaitForSeconds(ObtenerDuracionDialogo(primerDialogo));
            }

            // 2. Terminado el diálogo, se coordina la animación/estado del nene (se da vuelta)
            EjecutarCambioDeEstado();

            // 3. Esperamos el tiempo necesario (hasta que arranque a correr)
            if (tiempoDeFreeze > 0f)
            {
                yield return new WaitForSeconds(tiempoDeFreeze);
            }

            // 4. Justo antes de liberar el control, disparamos el segundo audio
            if (triggerSegundoDialogo != null)
            {
                // Si el sistema sigue reproduciendo algo, lo frenamos para que entre el segundo audio
                if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsPlaying)
                {
                    DialogueSystem.Instance.StopDialogue();
                }
                triggerSegundoDialogo.TriggerDialogue();
            }

            // 5. Liberamos el control
            PlayerPresenceManager.Instance.LookController.LookLocked = false; // Por seguridad
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
