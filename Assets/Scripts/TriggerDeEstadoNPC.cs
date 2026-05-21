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

        [Tooltip("(Opcional) Si el NPC está desactivado en la escena, asignalo aquí para activarlo automáticamente cuando se dispare el trigger.")]
        public GameObject npcGameObject;

        [Tooltip("El 'State Key' del estado al que saltará el NPC. Ej: PersecucionAI, PatrullajeAI, etc.")]
        public string stateKeyAForzar = "PersecucionAI";

        [Tooltip("Cómo se disparará este evento (Pisándolo, Interactuando, o Manualmente)")]
        public TriggerTypeEnum tipoDeTrigger = TriggerTypeEnum.Trigger;

        [Tooltip("¿El trigger solo debe funcionar la primera vez que el jugador lo use?")]
        public bool activarUnaSolaVez = true;

        [Tooltip("Audio opcional que sonará al mismo tiempo que el NPC cambia de estado (Ej: Graznido de cuervo)")]
        public AudioSource audioAlActivar;

        [Tooltip("Tiempo en segundos que espera ANTES de hacer que el NPC cambie de estado (útil si el objeto cae y quieres esperar un rato).")]
        public float retrasoAntesDeActivar = 0f;

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

        [Header("Diálogo al Aparecer el NPC")]
        [Tooltip("Diálogo del PJ que se dispara justo después de que el NPC aparece en escena.")]
        public DialogueAsset dialogoAlAparecer;

        [Tooltip("Segundos de espera entre que el NPC aparece y que arranca el diálogo (ej. 0.5 para dar un respiro).")]
        [Min(0f)]
        public float delayDialogoAparecer = 0.5f;

        [Tooltip("Si está activo, congela al jugador mientras dura este diálogo (solo aplica cuando 'Congelar Jugador' está desactivado).")]
        public bool congelarDuranteDialogoAparecer = false;

        [Header("Giro de Escape (después del freeze)")]
        [Tooltip("Si está activo, después de mirar al NPC y terminar los diálogos, el jugador rota hacia 'objetivoDeEscape' para quedar de frente a la ruta de escape.")]
        public bool girarHaciaEscape = false;

        [Tooltip("Transform vacío (Empty GameObject) colocado en la dirección a la que querés que quede mirando el jugador (Ej: la puerta del baño). Arrastralo desde la escena.")]
        public Transform objetivoDeEscape;

        [Tooltip("Duración en segundos de la rotación hacia la dirección de escape (0.3 – 1.0 recomendado).")]
        [Range(0.1f, 3f)]
        public float duracionGiroEscape = 0.5f;

        [Tooltip("Segundos de espera antes de iniciar el giro de escape (para que se vea natural después del susto).")]
        [Min(0f)]
        public float delayGiroEscape = 0.2f;

        [Tooltip("(Opcional) GameObjects que se activan justo después de completar el giro de escape. Ideal para triggers con diálogos, luces, efectos, etc.")]
        public GameObject[] objetosActivarAlGirar;

        private bool yaActivado = false;
        private Coroutine freezeCoroutine;

        private DialogueTrigger triggerPrimerDialogo;
        private DialogueTrigger triggerSegundoDialogo;
        private DialogueTrigger triggerDialogoAparecer;

        private void Start()
        {
            triggerPrimerDialogo   = CrearTriggerOculto(primerDialogo,    "PrimerDialogo");
            triggerSegundoDialogo  = CrearTriggerOculto(segundoDialogo,   "SegundoDialogo");
            triggerDialogoAparecer = CrearTriggerOculto(dialogoAlAparecer, "DialogoAparecer");
        }

        private DialogueTrigger CrearTriggerOculto(DialogueAsset asset, string nombre)
        {
            if (asset == null) return null;

            GameObject go = new GameObject($"HiddenDialogue_{nombre}");
            go.transform.SetParent(transform);
            
            DialogueTrigger dt = go.AddComponent<DialogueTrigger>();
            dt.Dialogue = asset;
            dt.DialogueType = DialogueTrigger.DialogueTypeEnum.Local;
            dt.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;

            // Solo asignamos el AudioSource si está configurado en el Inspector
            if (audioSourcePJ != null)
            {
                dt.DialogueAudio = audioSourcePJ;
            }
            else
            {
                Debug.LogWarning($"[Trigger] '{nombre}': No hay AudioSource del PJ asignado. El diálogo se mostrará sin audio.");
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

                if (retrasoAntesDeActivar > 0f && !congelarJugador)
                {
                    StartCoroutine(RutinaDeRetraso());
                }
                else if (congelarJugador)
                {
                    if (freezeCoroutine != null)
                        StopCoroutine(freezeCoroutine);

                    freezeCoroutine = StartCoroutine(RutinaDeFreeze());
                }
                else
                {
                    EjecutarCambioDeEstado();

                    // Usamos una coroutine para manejar todo lo asíncrono (diálogo + giro de escape)
                    StartCoroutine(RutinaSinFreeze());
                }
            }
            else
            {
                Debug.LogWarning("Se intentó disparar el estado del NPC pero no hay ninguno asignado.");
            }
        }

        private IEnumerator RutinaDeRetraso()
        {
            yield return new WaitForSeconds(retrasoAntesDeActivar);
            EjecutarCambioDeEstado();
        }

        private void EjecutarCambioDeEstado()
        {
            // Si hay un GameObject de NPC desactivado, lo activamos primero
            if (npcGameObject != null && !npcGameObject.activeSelf)
            {
                npcGameObject.SetActive(true);
                Debug.Log($"[Trigger] NPC '{npcGameObject.name}' activado.");
            }

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

            // 2b. Diálogo post-aparición del NPC (dentro del freeze del jugador)
            if (triggerDialogoAparecer != null)
            {
                if (delayDialogoAparecer > 0f)
                    yield return new WaitForSeconds(delayDialogoAparecer);

                if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsPlaying)
                    DialogueSystem.Instance.StopDialogue();

                triggerDialogoAparecer.TriggerDialogue();
                yield return new WaitForSeconds(ObtenerDuracionDialogo(dialogoAlAparecer));
            }

            // 3. Esperamos el tiempo necesario (hasta que arranque a correr)
            if (tiempoDeFreeze > 0f)
            {
                yield return new WaitForSeconds(tiempoDeFreeze);
            }

            // 4. Giro de escape: giramos al jugador hacia la ruta de huida antes de liberarlo
            if (girarHaciaEscape && objetivoDeEscape != null)
            {
                Debug.Log($"[Trigger] === GIRO DE ESCAPE INICIANDO ===");

                if (delayGiroEscape > 0f)
                    yield return new WaitForSeconds(delayGiroEscape);

                yield return StartCoroutine(GirarCamaraHacia(objetivoDeEscape, duracionGiroEscape));
                Debug.Log($"[Trigger] === GIRO DE ESCAPE COMPLETADO ===");
                ActivarObjetoPostGiro();
            }
            else if (girarHaciaEscape && objetivoDeEscape == null)
            {
                Debug.LogWarning("[Trigger] 'Girar Hacia Escape' está activado pero no hay un 'Objetivo De Escape' asignado en el Inspector.");
            }
            else
            {
                Debug.Log($"[Trigger] Giro de escape desactivado (girarHaciaEscape={girarHaciaEscape}, objetivo={(objetivoDeEscape != null ? objetivoDeEscape.name : "NULL")})");
            }

            // 5. Justo antes de liberar el control, disparamos el segundo audio
            if (triggerSegundoDialogo != null)
            {
                // Si el sistema sigue reproduciendo algo, lo frenamos para que entre el segundo audio
                if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsPlaying)
                {
                    DialogueSystem.Instance.StopDialogue();
                }
                triggerSegundoDialogo.TriggerDialogue();
            }

            // 6. Liberamos el control
            PlayerPresenceManager.Instance.LookController.LookLocked = false; // Por seguridad
            PlayerPresenceManager.Instance.FreezePlayer(false);
            Debug.Log("[Trigger] Jugador liberado.");
            freezeCoroutine = null;
        }

        /// <summary>
        /// Maneja la secuencia SIN freeze: diálogo opcional + giro de escape.
        /// </summary>
        private IEnumerator RutinaSinFreeze()
        {
            Debug.Log("[Trigger] RutinaSinFreeze: INICIO de la rutina.");

            // 1. Diálogo post-aparición (si hay)
            if (triggerDialogoAparecer != null)
            {
                Debug.Log("[Trigger] RutinaSinFreeze: Paso 1 — Preparando diálogo post-aparición.");

                if (delayDialogoAparecer > 0f)
                    yield return new WaitForSeconds(delayDialogoAparecer);

                if (congelarDuranteDialogoAparecer)
                    PlayerPresenceManager.Instance.FreezePlayer(true, false);

                try
                {
                    triggerDialogoAparecer.TriggerDialogue();
                    Debug.Log("[Trigger] RutinaSinFreeze: Diálogo disparado correctamente.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Trigger] Error al disparar diálogo: {e.Message}");
                }

                if (congelarDuranteDialogoAparecer)
                {
                    yield return new WaitForSeconds(ObtenerDuracionDialogo(dialogoAlAparecer));
                    PlayerPresenceManager.Instance.FreezePlayer(false);
                }
            }
            else
            {
                Debug.Log("[Trigger] RutinaSinFreeze: Paso 1 — Sin diálogo post-aparición configurado (dialogoAlAparecer está vacío).");
            }

            // 2. Giro de escape (si está activado)
            Debug.Log($"[Trigger] RutinaSinFreeze: Paso 2 — girarHaciaEscape={girarHaciaEscape}, objetivoDeEscape={(objetivoDeEscape != null ? objetivoDeEscape.name : "NULL")}, objetosActivarAlGirar={objetosActivarAlGirar?.Length ?? 0}");

            if (girarHaciaEscape && objetivoDeEscape != null)
            {
                Debug.Log("[Trigger] === GIRO DE ESCAPE (sin freeze) INICIANDO ===");

                // Congelamos brevemente al jugador para que el giro se vea limpio
                PlayerPresenceManager.Instance.FreezePlayer(true, false);

                if (delayGiroEscape > 0f)
                    yield return new WaitForSeconds(delayGiroEscape);

                yield return StartCoroutine(GirarCamaraHacia(objetivoDeEscape, duracionGiroEscape));

                // Liberamos al jugador
                PlayerPresenceManager.Instance.LookController.LookLocked = false;
                PlayerPresenceManager.Instance.FreezePlayer(false);
                Debug.Log("[Trigger] === GIRO DE ESCAPE (sin freeze) COMPLETADO ===");

                ActivarObjetoPostGiro();
            }
            else if (girarHaciaEscape)
            {
                Debug.LogWarning("[Trigger] 'Girar Hacia Escape' activado pero falta asignar 'Objetivo De Escape' en el Inspector.");
            }

            Debug.Log("[Trigger] RutinaSinFreeze: FIN de la rutina.");
        }

        private void ActivarObjetoPostGiro()
        {
            if (objetosActivarAlGirar == null || objetosActivarAlGirar.Length == 0) return;

            foreach (var obj in objetosActivarAlGirar)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"[Trigger] Objeto '{obj.name}' activado después del giro de escape.");
                }
            }
        }

        /// <summary>
        /// Gira la cámara del jugador para que mire en la dirección del forward del Transform objetivo.
        /// Detiene cualquier coroutine previa del LookController para evitar conflictos.
        /// </summary>
        private IEnumerator GirarCamaraHacia(Transform objetivo, float duracion)
        {
            var lookCtrl = PlayerPresenceManager.Instance.LookController;

            // 1. Matamos cualquier coroutine previa del LookController (la del primer giro hacia el NPC)
            lookCtrl.ResetCustomLerp();
            yield return null; // Un frame para que se limpie

            // 2. Calculamos la rotación destino usando el FORWARD del Empty
            Vector3 dir = objetivo.forward;
            float targetYaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float targetPitch = -Mathf.Asin(Mathf.Clamp(dir.y, -1f, 1f)) * Mathf.Rad2Deg;

            // 3. Leemos la rotación actual
            Vector2 actual = lookCtrl.LookRotation;

            Debug.Log($"[Trigger] GirarCamaraHacia: actual=({actual.x:F1}, {actual.y:F1}), destino=({targetYaw:F1}, {targetPitch:F1}), duracion={duracion}");

            // 4. Calculamos el total de rotación a aplicar (respetando camino más corto)
            float deltaYaw = Mathf.DeltaAngle(actual.x, targetYaw);
            float deltaPitch = Mathf.DeltaAngle(actual.y, targetPitch);

            Debug.Log($"[Trigger] GirarCamaraHacia: deltaYaw={deltaYaw:F1}, deltaPitch={deltaPitch:F1}");

            // 5. Aplicamos la rotación gradualmente usando LookOffset
            //    LookOffset es sumado a LookRotation por el Update() nativo del LookController 
            //    y luego reseteado a cero automáticamente, así no hay conflicto.
            float acumuladoYaw = 0f;
            float acumuladoPitch = 0f;
            float elapsed = 0f;

            while (elapsed < duracion)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duracion);
                // SmoothStep para una curva suave
                t = t * t * (3f - 2f * t);

                // Cuánto deberíamos haber rotado en total hasta este momento
                float yawDeseado = deltaYaw * t;
                float pitchDeseado = deltaPitch * t;

                // Cuánto necesitamos rotar ESTE frame (la diferencia con lo ya acumulado)
                float offsetYaw = yawDeseado - acumuladoYaw;
                float offsetPitch = pitchDeseado - acumuladoPitch;

                // Aplicamos vía LookOffset (el Update del LookController lo suma a LookRotation)
                lookCtrl.LookOffset = new Vector2(offsetYaw, offsetPitch);

                acumuladoYaw = yawDeseado;
                acumuladoPitch = pitchDeseado;

                yield return null;
            }

            // 6. Aseguramos que llegamos al 100%
            float remanYaw = deltaYaw - acumuladoYaw;
            float remanPitch = deltaPitch - acumuladoPitch;
            if (Mathf.Abs(remanYaw) > 0.01f || Mathf.Abs(remanPitch) > 0.01f)
            {
                lookCtrl.LookOffset = new Vector2(remanYaw, remanPitch);
                yield return null;
            }

            Debug.Log($"[Trigger] GirarCamaraHacia: FINAL LookRotation=({lookCtrl.LookRotation.x:F1}, {lookCtrl.LookRotation.y:F1})");
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
