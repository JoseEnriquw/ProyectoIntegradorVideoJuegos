using System.Collections;
using UnityEngine;
using UHFPS.Runtime;

public class TurnPlayerAroundOnDialogue : MonoBehaviour
{
    private DialogueTrigger dialogueTrigger;
    private bool isPlayerInTrigger = false;

    private void Awake()
    {
        dialogueTrigger = GetComponent<DialogueTrigger>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && dialogueTrigger != null)
        {
            if (!isPlayerInTrigger && !dialogueTrigger.IsCompleted)
            {
                isPlayerInTrigger = true;
                StartCoroutine(RotatePlayerRoutine());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }

    private IEnumerator RotatePlayerRoutine()
    {
        // 1. Wait a frame to let the dialogue start (or wait until dialogue starts playing)
        yield return null;

        // 2. Wait until the dialogue system has finished playing the dialogue
        if (DialogueSystem.HasReference)
        {
            yield return new WaitUntil(() => !DialogueSystem.Instance.IsPlaying);
        }

        // 3. Rotate the player 180 degrees (turn around)
        if (PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
        {
            var lookController = PlayerPresenceManager.Instance.Player.GetComponentInChildren<LookController>();
            if (lookController != null)
            {
                Debug.Log($"[TurnPlayerAroundOnDialogue] Dialogue finished on {gameObject.name}. Turning player around 180 degrees.");
                float targetYaw = lookController.LookRotation.x + 180f;
                lookController.LerpRotation(new Vector2(targetYaw, lookController.LookRotation.y), 1.0f);
            }
        }
    }
}
