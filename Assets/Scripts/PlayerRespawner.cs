using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace UHFPS.Runtime
{
    public class PlayerRespawner : MonoBehaviour
    {
        public static PlayerRespawner Instance { get; private set; }

        [Header("Player Settings")]
        [Tooltip("The player GameObject. If empty, the script will automatically find the player via UHFPS PlayerPresenceManager or the 'Player' tag.")]
        public GameObject playerObject;

        [Header("Spawn Settings")]
        [Tooltip("Transform for the spawn location. If empty, this script's GameObject position and rotation will be used.")]
        public Transform spawnLocation;

        [Tooltip("Make this script the default global respawn point (accessible via PlayerRespawner.Instance).")]
        public bool isDefaultSpawnPoint = true;

        [Header("Respawn Condition Options")]
        [Tooltip("If true, the player will respawn automatically if they fall below a certain height (Y coordinate).")]
        public bool useFallLimit = true;
        [Tooltip("The minimum Y coordinate (height) before triggering a respawn.")]
        public float fallLimitY = -50f;

        [Tooltip("If true, the player will respawn when their health drops to 0 (if UHFPS PlayerHealth is present).")]
        public bool respawnOnDeath = true;

        [Tooltip("Should we restore player health to maximum on respawn?")]
        public bool restoreHealth = true;

        [Header("Visual Transition")]
        [Tooltip("Should we use the screen fade effect during teleportation?")]
        public bool useScreenFade = true;
        [Tooltip("Speed of the fade out/in effect.")]
        public float fadeSpeed = 3f;
        [Tooltip("Delay in seconds when the screen is fully black before starting to fade back in.")]
        public float blackScreenDelay = 0.3f;

        [Header("Events")]
        public UnityEvent OnBeforeRespawn;
        public UnityEvent OnAfterRespawn;

        private bool isRespawning = false;
        private PlayerHealth cachedPlayerHealth;

        private void Awake()
        {
            if (isDefaultSpawnPoint)
            {
                if (Instance != null && Instance != this)
                {
                    Debug.LogWarning($"[PlayerRespawner] Multiple instances of PlayerRespawner set as default. Replacing previous reference on '{Instance.gameObject.name}' with '{gameObject.name}'.");
                }
                Instance = this;
            }
        }

        private void Start()
        {
            InitializePlayerReference();
        }

        private void Update()
        {
            if (isRespawning) return;

            // 1. Check if the player falls below the height limit
            if (useFallLimit && playerObject != null)
            {
                if (playerObject.transform.position.y < fallLimitY)
                {
                    Debug.Log($"[PlayerRespawner] Player fell below height limit ({playerObject.transform.position.y} < {fallLimitY}). Triggering respawn.");
                    Respawn();
                }
            }

            // 2. Backup check for death condition (if events or state machine didn't handle it directly)
            if (respawnOnDeath && cachedPlayerHealth != null && cachedPlayerHealth.IsDead)
            {
                Debug.Log("[PlayerRespawner] Player is dead. Triggering respawn.");
                Respawn();
            }
        }

        private void InitializePlayerReference()
        {
            if (playerObject == null)
            {
                if (PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
                {
                    playerObject = PlayerPresenceManager.Instance.Player;
                }
                else
                {
                    playerObject = GameObject.FindWithTag("Player");
                }
            }

            if (playerObject != null)
            {
                cachedPlayerHealth = playerObject.GetComponent<PlayerHealth>();
            }
        }

        /// <summary>
        /// Dynamically updates the spawn transform location.
        /// </summary>
        public void SetSpawnLocation(Transform newSpawnLocation)
        {
            spawnLocation = newSpawnLocation;
        }

        /// <summary>
        /// Triggers the player respawn process.
        /// </summary>
        public void Respawn()
        {
            if (isRespawning) return;
            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            isRespawning = true;

            // Ensure player reference is initialized
            if (playerObject == null)
            {
                InitializePlayerReference();
            }

            if (playerObject == null)
            {
                Debug.LogError("[PlayerRespawner] Player object could not be found! Cannot respawn.");
                isRespawning = false;
                yield break;
            }

            // Trigger events before teleporting (e.g. deactivate hazards, stop timers, etc.)
            OnBeforeRespawn?.Invoke();

            // Check if player is the UHFPS player instance
            bool hasUHFPSPresence = PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player == playerObject;
            
            // 1. Freeze player input and motion
            if (hasUHFPSPresence)
            {
                PlayerPresenceManager.Instance.FreezePlayer(true);
            }

            // 2. Screen Fade Out (fade to black)
            if (useScreenFade && GameManager.HasReference)
            {
                // In UHFPS: false fades the screen to black
                yield return GameManager.Instance.StartBackgroundFade(false, fadeSpeed: fadeSpeed);
            }
            else if (useScreenFade)
            {
                // Fallback wait if GameManager is not active
                yield return new WaitForSeconds(0.5f);
            }

            // Determine target position and rotation
            Vector3 targetPosition = spawnLocation != null ? spawnLocation.position : transform.position;
            Vector3 targetEuler = spawnLocation != null ? spawnLocation.eulerAngles : transform.eulerAngles;

            // 3. Teleport Player
            if (hasUHFPSPresence)
            {
                // UHFPS look rotation uses euler angles (yaw: horizontal, pitch: vertical)
                Vector2 eulerLook = new Vector2(targetEuler.y, 0f); // Face target direction, reset vertical look angle to look forward
                PlayerPresenceManager.Instance.SetPlayerPositionAndLook(targetPosition, eulerLook);
            }
            else
            {
                // Fallback for general CharacterController / Rigidbody player setup
                CharacterController cc = playerObject.GetComponent<CharacterController>();
                Rigidbody rb = playerObject.GetComponent<Rigidbody>();

                if (cc != null) cc.enabled = false;
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                playerObject.transform.position = targetPosition;
                playerObject.transform.rotation = Quaternion.Euler(0f, targetEuler.y, 0f);

                if (cc != null) cc.enabled = true;
                if (rb != null) rb.isKinematic = false;
            }

            // 4. Restore Health & Revive
            if (restoreHealth && cachedPlayerHealth != null)
            {
                cachedPlayerHealth.InitializeHealth((int)cachedPlayerHealth.MaxHealth, (int)cachedPlayerHealth.MaxHealth);
                cachedPlayerHealth.IsDead = false;
            }

            // 5. Hide Dead Panel if player was dead
            if (GameManager.HasReference)
            {
                GameManager.Instance.ShowPanel(GameManager.PanelType.MainPanel);
            }

            // Optional delay during full black screen
            if (blackScreenDelay > 0f)
            {
                yield return new WaitForSeconds(blackScreenDelay);
            }

            // 6. Screen Fade In (reveal the game)
            if (useScreenFade && GameManager.HasReference)
            {
                // In UHFPS: true fades the black background back to transparent
                yield return GameManager.Instance.StartBackgroundFade(true, fadeSpeed: fadeSpeed);
            }

            // 7. Unfreeze player and give controls back
            if (hasUHFPSPresence)
            {
                PlayerPresenceManager.Instance.UnlockPlayer();
            }

            // Trigger events after teleporting (e.g. play sounds, save game, restart timer, etc.)
            OnAfterRespawn?.Invoke();

            isRespawning = false;
        }
    }
}
