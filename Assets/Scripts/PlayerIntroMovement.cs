using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UHFPS.Runtime;

public class PlayerIntroMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Duration of the first walk before opening the door.")]
    public float MoveDuration = 2.5f;
    [Tooltip("Speed of the automatic walk.")]
    public float MoveSpeed = 1.5f;
    [Tooltip("How long to wait after fading in before moving.")]
    public float WaitBeforeStart = 0.5f;
    [Tooltip("Speed of the initial fade from black.")]
    public float FadeInSpeed = 2f;

    [Header("Door Interaction")]
    [Tooltip("The door that the player will open and pass through.")]
    public DynamicObject IntroDoor;
    [Tooltip("The dialogue trigger on the door that should play when locked.")]
    public DialogueTrigger IntroDoorDialogue;
    [Tooltip("Wait time after the first walk before opening the door.")]
    public float WaitBeforeOpen = 0.2f;
    [Tooltip("Delay after opening the door before moving through.")]
    public float OpenDelay = 0.8f;
    [Tooltip("Duration of the walk through the door.")]
    public float DoorMoveDuration = 2.0f;
    [Tooltip("Delay after moving through before closing the door.")]
    public float CloseDelay = 0.5f;

    [Header("Triggers")]
    [Tooltip("Should it trigger the sickness announcement after walking?")]
    public bool ShowAnnouncementAtEnd = true;
    [Tooltip("Event triggered just before giving control back to the player.")]
    public UnityEvent OnIntroEnd;

    private void Start()
    {
        if (IntroDoorDialogue != null)
        {
            // We set the dialogue trigger to 'Event' type so it doesn't trigger 
            // on its own via the Interact type, and instead fire it from the locked event.
            // We DON'T disable the component because its Start() needs to run to initialize data.
            IntroDoorDialogue.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;

            if (IntroDoor != null)
            {
                IntroDoor.lockedEvent.AddListener(IntroDoorDialogue.TriggerDialogue);
            }
        }
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        // 1. Initial Setup - Ensure player is frozen
        if (PlayerPresenceManager.HasReference)
        {
            PlayerPresenceManager.Instance.FreezePlayer(true);
        }

        // Wait a frame to ensure all systems are ready
        yield return null;

        // 2. Fade In from Black
        if (GameManager.HasReference)
        {
            // We start the background fade to visible (true)
            yield return GameManager.Instance.StartBackgroundFade(true, fadeSpeed: FadeInSpeed);
        }

        yield return new WaitForSeconds(WaitBeforeStart);

        // 3. Phase 1: Move Player near Door
        if (PlayerPresenceManager.HasReference)
        {
            PlayerStateMachine psm = PlayerPresenceManager.Instance.StateMachine;
            CharacterController controller = psm.Controller;
            
            // Walk forward
            yield return MoveRoutine(controller, psm.transform, MoveDuration);

            // 4. Door Interaction: Open
            if (IntroDoor != null)
            {
                yield return new WaitForSeconds(WaitBeforeOpen);
                IntroDoor.SetOpenState();
                yield return new WaitForSeconds(OpenDelay);

                // 5. Phase 2: Move Player through Door
                yield return MoveRoutine(controller, psm.transform, DoorMoveDuration);

                // 6. Door Interaction: Close
                yield return new WaitForSeconds(CloseDelay);
                IntroDoor.SetCloseState();

                // Wait for the door to physically close before locking it
                // Note: We use a fixed wait because IsOpened returns the target state immediately in UHFPS
                yield return new WaitForSeconds(1.5f);

                // 6.1 Block the door
                IntroDoor.SetLockedStatus(true);
            }
        }

        // 7. Unlock Player control and HUD
        if (OnIntroEnd != null)
        {
            OnIntroEnd.Invoke();
        }

        if (PlayerPresenceManager.HasReference)
        {
            PlayerPresenceManager.Instance.UnlockPlayer();
        }

        // 8. Trigger Announcement if requested
        if (ShowAnnouncementAtEnd && SurvivalTimerAnnouncement.Instance != null)
        {
            SurvivalTimerAnnouncement.Instance.Show();
        }

        // Note: Component is no longer self-destroyed to allow UnlockDoor callback
    }

    public void UnlockDoor()
    {
        if (IntroDoor != null)
        {
            IntroDoor.SetLockedStatus(false);
        }
    }

    private IEnumerator MoveRoutine(CharacterController controller, Transform playerTransform, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            // Calculate move direction based on player current forward
            Vector3 moveDir = playerTransform.forward * MoveSpeed;
            
            // Move the character controller directly
            if (controller != null && controller.enabled)
            {
                controller.Move(moveDir * Time.deltaTime);
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }
}
