using System.Collections;
using UnityEngine;
using UHFPS.Runtime;

public class TurnPlayerAroundOnDialogue : MonoBehaviour
{
    private DialogueTrigger dialogueTrigger;
    private bool isPlayerInTrigger = false;
    private bool shouldRotate = false;

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

                // Check if the player is looking towards the car or away from it
                if (PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
                {
                    var lookController = PlayerPresenceManager.Instance.Player.GetComponentInChildren<LookController>();
                    if (lookController != null)
                    {
                        Vector3 playerPos = PlayerPresenceManager.Instance.Player.transform.position;
                        Vector3 carPos = FindCarPosition();

                        Vector3 playerLookDir = lookController.LookForward2D;
                        playerLookDir.y = 0;
                        playerLookDir.Normalize();

                        Vector3 dirToCar = carPos - playerPos;
                        dirToCar.y = 0;
                        dirToCar.Normalize();

                        float dot = Vector3.Dot(playerLookDir, dirToCar);
                        
                        // If dot < 0.1, the player is looking away from the car (front towards the boundary).
                        // If dot >= 0.1, the player is looking towards the car (going back first/de espaldas).
                        if (dot < 0.1f)
                        {
                            shouldRotate = true;
                            Debug.Log($"[TurnPlayerAroundOnDialogue] Player entered looking away from the car (dot: {dot}). Will rotate on dialogue end.");
                        }
                        else
                        {
                            shouldRotate = false;
                            Debug.Log($"[TurnPlayerAroundOnDialogue] Player entered looking towards the car (dot: {dot}). Will NOT rotate.");
                        }
                    }
                }

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
        // 1. Wait a frame to let the dialogue trigger start
        yield return null;

        // 2. Wait until the dialogue system has finished playing the dialogue
        if (DialogueSystem.HasReference)
        {
            yield return new WaitUntil(() => !DialogueSystem.Instance.IsPlaying);
        }

        // 3. Rotate the player to face the car if they entered facing away from it
        if (shouldRotate && PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
        {
            var lookController = PlayerPresenceManager.Instance.Player.GetComponentInChildren<LookController>();
            if (lookController != null)
            {
                Vector3 playerPos = PlayerPresenceManager.Instance.Player.transform.position;
                Vector3 carPos = FindCarPosition();

                Vector3 directionToCar = carPos - playerPos;
                directionToCar.y = 0; // Keep rotation on the horizontal plane
                
                if (directionToCar.sqrMagnitude > 0.01f)
                {
                    Quaternion rotationToCar = Quaternion.LookRotation(directionToCar);
                    float targetYaw = rotationToCar.eulerAngles.y;

                    Debug.Log($"[TurnPlayerAroundOnDialogue] Dialogue finished. Rotating player to face the car (Target Yaw: {targetYaw}).");
                    lookController.LerpRotation(new Vector2(targetYaw, lookController.LookRotation.y), 1.0f);
                }
            }
        }

        shouldRotate = false;
    }

    private Vector3 FindCarPosition()
    {
        GameObject car = GameObject.Find("peugeot 504");
        if (car != null)
        {
            return car.transform.position;
        }

        GameObject engine = GameObject.Find("3D_CarEngine");
        if (engine != null)
        {
            return engine.transform.position;
        }

        // Fallback position in scene 2 Bosque
        return new Vector3(456.55f, 27.30f, 146.94f);
    }
}
