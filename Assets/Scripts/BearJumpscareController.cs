using System.Collections;
using UnityEngine;
using UHFPS.Runtime;
using UHFPS.Tools;

public class BearJumpscareController : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Maximum distance from the bear to check for look-at triggers.")]
    public float maxTriggerDistance = 100f;

    private CapsuleCollider originalCol;
    private JumpscareTrigger jumpscareTrigger;
    private bool hasTriggered = false;
    private float lastLogTime = 0f;

    private void Start()
    {
        // 1. Get or add JumpscareTrigger component
        jumpscareTrigger = GetComponent<JumpscareTrigger>();
        if (jumpscareTrigger == null)
        {
            jumpscareTrigger = gameObject.AddComponent<JumpscareTrigger>();
        }

        // 2. Fetch the bear sound clip from the Bear's AudioSource
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            jumpscareTrigger.JumpscareSound = new SoundClip(audioSource.clip, 1.0f);
            Debug.Log($"[BearJumpscareController] JumpscareSound initialized with: {audioSource.clip.name}");
        }
        else
        {
            Debug.LogWarning("[BearJumpscareController] Bear AudioSource or clip not found.");
        }

        // Find the original non-trigger collider first so we can use it for calculations
        originalCol = null;
        CapsuleCollider[] allColliders = GetComponents<CapsuleCollider>();
        foreach (var col in allColliders)
        {
            if (!col.isTrigger)
            {
                originalCol = col;
                break;
            }
        }

        // 3. Configure JumpscareTrigger properties programmatically
        jumpscareTrigger.JumpscareType = JumpscareTrigger.JumpscareTypeEnum.Audio;
        jumpscareTrigger.TriggerType = JumpscareTrigger.TriggerTypeEnum.Event;
        jumpscareTrigger.LookAtJumpscare = true;

        // Create a child look-at target at the bear's head (top portion of capsule collider)
        GameObject lookAtTargetGo = new GameObject("Jumpscare_HeadLookAt");
        lookAtTargetGo.transform.SetParent(transform, false);
        if (originalCol != null)
        {
            lookAtTargetGo.transform.localPosition = originalCol.center + new Vector3(0f, originalCol.height * 0.45f, 0f);
        }
        else
        {
            lookAtTargetGo.transform.localPosition = new Vector3(-0.01f, 0.6f, -0.55f);
        }
        jumpscareTrigger.LookAtTarget = lookAtTargetGo.transform;

        jumpscareTrigger.LookAtDuration = 0.5f;
        jumpscareTrigger.InfluenceWobble = true;
        jumpscareTrigger.WobbleAmplitudeGain = 0.8f;
        jumpscareTrigger.WobbleFrequencyGain = 8f;
        jumpscareTrigger.WobbleDuration = 0.8f;
        jumpscareTrigger.InfluenceFear = true;
        jumpscareTrigger.FearDuration = 2.0f;

        // 4. Add a trigger CapsuleCollider for proximity/touch detection
        CapsuleCollider triggerCol = gameObject.AddComponent<CapsuleCollider>();
        triggerCol.isTrigger = true;

        if (originalCol != null)
        {
            triggerCol.center = originalCol.center;
            triggerCol.radius = originalCol.radius * 1.3f; // Slightly wider for easy proximity/touch detection
            triggerCol.height = originalCol.height;
            triggerCol.direction = originalCol.direction;
            Debug.Log($"[BearJumpscareController] Copying collider values from original: center={triggerCol.center}, radius={triggerCol.radius}, height={triggerCol.height}");
        }
        else
        {
            // Fallback to coordinates from Inspector
            triggerCol.center = new Vector3(-0.01f, -1.48f, -0.55f);
            triggerCol.radius = 1.2f;
            triggerCol.height = 4.17f;
            Debug.LogWarning("[BearJumpscareController] Original CapsuleCollider not found. Using fallback coordinates.");
        }

        // 5. Connect to the door jumpscare trigger for mutual exclusion
        JumpscareTrigger doorTrigger = FindDoorTrigger();
        if (doorTrigger != null)
        {
            doorTrigger.OnJumpscareStarted.AddListener(OnDoorJumpscareTriggered);
            Debug.Log("[BearJumpscareController] Successfully connected to door trigger for mutual exclusion.");
        }
        else
        {
            Debug.LogWarning("[BearJumpscareController] Door trigger 'Trigger_AudioEvent' under 'Casa_Desvio' not found!");
        }
    }

    private JumpscareTrigger FindDoorTrigger()
    {
        JumpscareTrigger[] triggers = FindObjectsByType<JumpscareTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in triggers)
        {
            // The cabin door trigger is named "Trigger_AudioEvent" and its parent is "Casa_Desvio"
            if (t.gameObject.name == "Trigger_AudioEvent" && t.transform.parent != null && t.transform.parent.name == "Casa_Desvio")
            {
                return t;
            }
        }
        return null;
    }

    private void OnDoorJumpscareTriggered()
    {
        Debug.Log("[BearJumpscareController] Door jumpscare was triggered. Disabling bear jumpscare.");
        hasTriggered = true;
        this.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (other.CompareTag("Player"))
        {
            Debug.Log("[BearJumpscareController] Player touched the bear's trigger collider! Activating jumpscare.");
            TriggerJumpscare();
        }
    }

    private void Update()
    {
        if (hasTriggered || jumpscareTrigger == null)
            return;

        if (PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
        {
            Camera mainCam = PlayerPresenceManager.Instance.PlayerCamera;
            if (mainCam != null)
            {
                if (IsPlayerLookingAtBear(mainCam))
                {
                    Debug.Log("[BearJumpscareController] Player is looking at the bear with clear line of sight! Activating jumpscare.");
                    TriggerJumpscare();
                }
            }
        }
    }

    private bool IsPlayerLookingAtBear(Camera mainCam)
    {
        // Vector pointing to the bear's physical center (dynamically calculated from capsule collider)
        Vector3 bearCenter = originalCol != null ? transform.TransformPoint(originalCol.center) : transform.position;
        Vector3 dirToBear = bearCenter - mainCam.transform.position;
        float distance = dirToBear.magnitude;

        if (distance > maxTriggerDistance)
            return false;

        Vector3 dirToBearNormalized = dirToBear.normalized;

        // 1. Cone of vision check: angle between player's look direction and bear direction
        float angle = Vector3.Angle(mainCam.transform.forward, dirToBearNormalized);
        if (angle > 22f) // Conic angle of ~44 degrees total field of view for trigger
            return false;

        // 2. Line of sight check: raycast from camera to bear center
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Ignore Raycast");

        // Raycast from camera to bear center.
        // We set max distance to 'distance + 1.0f' to allow it to reach the bear
        if (Physics.Raycast(mainCam.transform.position, dirToBearNormalized, out hit, distance + 1.0f, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return true;
            }
            else
            {
                // Print block reason occasionally for debugging
                if (Time.time - lastLogTime > 1.0f)
                {
                    Debug.Log($"[BearJumpscareController] Look-at blocked by: '{hit.transform.name}' (Layer: {LayerMask.LayerToName(hit.transform.gameObject.layer)}, Tag: '{hit.transform.tag}') at distance {hit.distance}m (Bear is at {distance}m)");
                    lastLogTime = Time.time;
                }
            }
        }
        else
        {
            // If the raycast didn't hit anything, it means there are no obstacles in the way!
            // (e.g. if the bear's collider didn't register but there is no wall either)
            return true;
        }

        // 3. Fallback: Raycast directly in camera's forward direction to see if it hits the bear's collider
        Ray forwardRay = new Ray(mainCam.transform.position, mainCam.transform.forward);
        if (Physics.Raycast(forwardRay, out hit, maxTriggerDistance, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
            {
                return true;
            }
        }

        return false;
    }

    private void TriggerJumpscare()
    {
        hasTriggered = true;
        jumpscareTrigger.TriggerJumpscare();

        // Copy the door jumpscare configuration and close the door if the bear is triggered first!
        JumpscareTrigger doorTrigger = FindDoorTrigger();
        if (doorTrigger != null)
        {
            Debug.Log("[BearJumpscareController] Bear jumpscare was triggered. Executing door close and sound events from Trigger_AudioEvent.");
            
            // 1. Invoke the door trigger's events (closes the door and triggers sounds)
            doorTrigger.OnJumpscareStarted?.Invoke();
            
            // 2. Play the door slam audio clip at the door's position
            if (doorTrigger.JumpscareSound != null)
            {
                GameTools.PlayOneShot2D(doorTrigger.transform.position, doorTrigger.JumpscareSound, "Jumpscare Sound");
            }

            // 3. Disable the door trigger collider and script so it cannot trigger again
            var col = doorTrigger.GetComponent<Collider>();
            if (col != null) col.enabled = false;
            doorTrigger.enabled = false;
            doorTrigger.gameObject.SetActive(false);
        }
    }
}
