using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UHFPS.Runtime;

public class PlayerIntroMovement : MonoBehaviour
{
    [Header("Intro Settings")]
    [Tooltip("How long to wait after fading in before starting the intro events.")]
    public float WaitBeforeStart = 0.5f;
    [Tooltip("Speed of the initial fade from black.")]
    public float FadeInSpeed = 1.5f;
    
    [Header("Door Interaction")]
    [Tooltip("The door that will close in front of the player.")]
    public DynamicObject IntroDoor;
    [Tooltip("The dialogue trigger to play at the start of the intro.")]
    public DialogueTrigger InitialDialogue;
    [Tooltip("The dialogue trigger on the door that should play when locked (optional).")]
    public DialogueTrigger IntroDoorDialogue;
    [Tooltip("Delay before closing the door after the dialogue starts.")]
    public float CloseDelay = 0.5f;

    [Header("Triggers")]
    [Tooltip("Should it trigger the sickness announcement at the end?")]
    public bool ShowAnnouncementAtEnd = true;
    [Tooltip("Event triggered just before giving control back to the player.")]
    public UnityEvent OnIntroEnd;

    private void Start()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (sceneName != "1 IntroHouse" && sceneName != "1 IntroCutScene")
        {
            // Fix para el editor: como el Player es un Prefab y está configurado en "Manually" 
            // (para que la intro funcione en la escena 1), si le damos Play directo a la escena 2,
            // nunca nadie lo desbloquea ni quita la pantalla negra.
            // Con esto forzamos que se destrabe y haga el fade-in si no venimos de una pantalla de carga.
            if (!SaveGameManager.GameActuallyLoad && PlayerPresenceManager.HasReference)
            {
                PlayerPresenceManager.Instance.UnlockPlayer();
            }

            Destroy(this);
            return;
        }

        // Auto-asignar el DialogueTrigger "Intro" de la escena si está vacío en el inspector
        if (InitialDialogue == null)
        {
            foreach (var dt in FindObjectsOfType<DialogueTrigger>())
            {
                if (dt.name == "Intro")
                {
                    InitialDialogue = dt;
                    break;
                }
            }
        }

        if (IntroDoorDialogue != null)
        {
            // We set the dialogue trigger to 'Event' type so it doesn't trigger 
            // on its own via the Interact type, and instead fire it from the locked event.
            IntroDoorDialogue.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;

            if (IntroDoor != null)
            {
                IntroDoor.lockedEvent.AddListener(IntroDoorDialogue.TriggerDialogue);
            }
        }

        if (InitialDialogue != null)
        {
            InitialDialogue.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;
        }

        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        IntroDebugger.Log($"[PlayerIntroMovement] IntroRoutine started in scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        // Deshabilitar la tecla Escape (menú de pausa) e Inventario durante toda la cinemática
        if (GameManager.HasReference)
        {
            GameManager.Instance.LockInput(true);
        }

        // 1. Initial Setup - Ensure player is frozen and cursor is hidden/locked
        if (PlayerPresenceManager.HasReference)
        {
            PlayerPresenceManager.Instance.FreezePlayer(true);
        }

        // Wait a frame to ensure all systems are ready
        yield return null;
        IntroDebugger.Log("[PlayerIntroMovement] Frame 1 wait completed");

        // Wait until the IntroDisclaimer has finished showing and destroyed itself
        IntroDisclaimer disclaimer = FindObjectOfType<IntroDisclaimer>();
        if (disclaimer != null)
        {
            IntroDebugger.Log("[PlayerIntroMovement] Disclaimer found, waiting for it to destroy");
            yield return new WaitUntil(() => disclaimer == null);
            IntroDebugger.Log("[PlayerIntroMovement] Disclaimer destroyed");
        }

        // 2. Fade In from Black
        if (GameManager.HasReference)
        {
            IntroDebugger.Log("[PlayerIntroMovement] Starting fade in");
            yield return GameManager.Instance.StartBackgroundFade(true, fadeSpeed: FadeInSpeed);
            if (PlayerPresenceManager.HasReference)
            {
                PlayerPresenceManager.Instance.IsBackgroundFadedOut = true;
            }
            IntroDebugger.Log("[PlayerIntroMovement] Fade in completed");
        }

        yield return new WaitForSeconds(WaitBeforeStart);
        IntroDebugger.Log("[PlayerIntroMovement] WaitBeforeStart completed");

        // 3. Play initial dialogue
        if (InitialDialogue != null)
        {
            IntroDebugger.Log($"[PlayerIntroMovement] Triggering dialogue: {InitialDialogue.name}");
            InitialDialogue.TriggerDialogue();
        }

        // 4. Door Interaction: Close
        if (IntroDoor != null)
        {
            IntroDebugger.Log("[PlayerIntroMovement] IntroDoor is closing");
            yield return new WaitForSeconds(CloseDelay);
            IntroDoor.SetCloseState();

            // Wait for the door to physically close before locking it
            yield return new WaitForSeconds(1.5f);

            // 4.1 Block the door
            IntroDoor.SetLockedStatus(true);
            IntroDebugger.Log("[PlayerIntroMovement] IntroDoor is locked");
        }

        // ESPERAR a que termine el diálogo inicial antes de continuar
        if (DialogueSystem.HasReference)
        {
            IntroDebugger.Log("[PlayerIntroMovement] Waiting for dialogue to finish");
            yield return new WaitUntil(() => !DialogueSystem.Instance.IsPlaying);
            IntroDebugger.Log("[PlayerIntroMovement] Dialogue finished");
        }

        // Esperar 1 segundo de pausa después de finalizar el audio/subtítulo
        yield return new WaitForSeconds(1.0f);
        IntroDebugger.Log("[PlayerIntroMovement] 1.0 second wait completed");

        // 5. Activar el Trigger de Cambio de Escena (CinematicSceneLoader) de forma automática
        var sceneLoader = FindObjectOfType<CinematicSceneLoader>();
        IntroDebugger.Log($"[PlayerIntroMovement] CinematicSceneLoader check: {(sceneLoader != null ? "Found" : "NULL")}");
        if (sceneLoader != null)
        {
            Debug.Log("[PlayerIntroMovement] Diálogo terminado. Deteniendo todos los sonidos e iniciando fundido a negro...");
            
            // Stop all active AudioSources in the scene immediately
            AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
            foreach (AudioSource source in audioSources)
            {
                if (source != null && source.isPlaying)
                {
                    source.Stop();
                }
            }

            // Disable input locking to ensure everything cleans up properly (identical to skip button)
            if (GameManager.HasReference)
            {
                GameManager.Instance.LockInput(false);
            }

            // Fundido suave a negro (fadeOut = false) con velocidad 1.5
            if (GameManager.HasReference)
            {
                yield return GameManager.Instance.StartBackgroundFade(false, fadeSpeed: 1.5f);
            }

            Debug.Log("[PlayerIntroMovement] Fundido completado. Cargando siguiente escena de forma directa...");
            sceneLoader.LoadNextSceneDirect();
            yield break; // Finaliza la corrutina aquí ya que cambiamos de escena
        }

        // 6. Si no hay scene loader, liberar al jugador y desbloquear controles (comportamiento por defecto)
        if (OnIntroEnd != null)
        {
            OnIntroEnd.Invoke();
        }

        if (PlayerPresenceManager.HasReference)
        {
            IntroDebugger.Log("[PlayerIntroMovement] Unlocking player...");
            PlayerPresenceManager.Instance.UnlockPlayer();
            IntroDebugger.Log("[PlayerIntroMovement] UnlockPlayer called");
        }

        // Habilitar de nuevo la tecla Escape (menú de pausa) al finalizar la cinemática
        if (GameManager.HasReference)
        {
            GameManager.Instance.LockInput(false);
            IntroDebugger.Log("[PlayerIntroMovement] LockInput(false) called");
        }

        // 7. Trigger Announcement if requested
        if (ShowAnnouncementAtEnd && SurvivalTimerAnnouncement.Instance != null)
        {
            SurvivalTimerAnnouncement.Instance.Show();
        }
        IntroDebugger.Log("[PlayerIntroMovement] IntroRoutine fully finished");
    }

    public void UnlockDoor()
    {
        if (IntroDoor != null)
        {
            IntroDoor.SetLockedStatus(false);
        }
    }
}
