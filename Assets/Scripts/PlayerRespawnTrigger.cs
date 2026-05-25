using UnityEngine;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class PlayerRespawnTrigger : MonoBehaviour
    {
        [Header("Respawner Reference")]
        [Tooltip("The specific PlayerRespawner to trigger. If empty, it will automatically use the global PlayerRespawner.Instance.")]
        public PlayerRespawner respawner;

        [Header("Trigger Options")]
        [Tooltip("Should this trigger deactivate itself after one use?")]
        public bool triggerOnce = false;

        private bool hasTriggered = false;

        private void Start()
        {
            // Ensure the collider is set as a trigger
            Collider col = GetComponent<Collider>();
            if (col != null && !col.isTrigger)
            {
                col.isTrigger = true;
                Debug.LogWarning($"[PlayerRespawnTrigger] Collider on '{gameObject.name}' was not set as Trigger. Auto-correcting to true.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (triggerOnce && hasTriggered) return;

            // Check if the collider belongs to the Player
            bool isPlayer = other.CompareTag("Player") || 
                           other.GetComponent<PlayerStateMachine>() != null || 
                           other.GetComponent<CharacterController>() != null;

            if (isPlayer)
            {
                PlayerRespawner activeRespawner = respawner != null ? respawner : PlayerRespawner.Instance;

                if (activeRespawner != null)
                {
                    hasTriggered = true;
                    Debug.Log($"[PlayerRespawnTrigger] Player entered trigger zone on '{gameObject.name}'. Triggering respawn.");
                    activeRespawner.Respawn();
                }
                else
                {
                    Debug.LogError($"[PlayerRespawnTrigger] Could not trigger respawn because no PlayerRespawner was found or assigned!");
                }
            }
        }
    }
}
