using UnityEngine;
using System.Collections;
using UHFPS.Runtime;

public class PlayerIntroMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Duration of the automatic walk.")]
    public float MoveDuration = 2.5f;
    [Tooltip("Speed of the automatic walk.")]
    public float MoveSpeed = 1.5f;
    [Tooltip("How long to wait after fading in before moving.")]
    public float WaitBeforeStart = 0.5f;
    [Tooltip("Speed of the initial fade from black.")]
    public float FadeInSpeed = 2f;

    [Header("Triggers")]
    [Tooltip("Should it trigger the sickness announcement after walking?")]
    public bool ShowAnnouncementAtEnd = true;

    private void Start()
    {
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

        // 3. Move Player Forward
        if (PlayerPresenceManager.HasReference)
        {
            PlayerStateMachine psm = PlayerPresenceManager.Instance.StateMachine;
            CharacterController controller = psm.Controller;
            
            float timer = 0f;
            while (timer < MoveDuration)
            {
                // Calculate move direction based on player current forward
                Vector3 moveDir = psm.transform.forward * MoveSpeed;
                
                // Move the character controller directly
                // Note: psm.Motion is not used here as we want a constant scripted movement
                if (controller != null && controller.enabled)
                {
                    controller.Move(moveDir * Time.deltaTime);
                }

                timer += Time.deltaTime;
                yield return null;
            }
        }

        // 4. Unlock Player control and HUD
        if (PlayerPresenceManager.HasReference)
        {
            PlayerPresenceManager.Instance.UnlockPlayer();
        }

        // 5. Trigger Announcement if requested
        if (ShowAnnouncementAtEnd && SurvivalTimerAnnouncement.Instance != null)
        {
            SurvivalTimerAnnouncement.Instance.Show();
        }

        // Self-destroy this component to avoid duplicate execution/overhead
        Destroy(this);
    }
}
