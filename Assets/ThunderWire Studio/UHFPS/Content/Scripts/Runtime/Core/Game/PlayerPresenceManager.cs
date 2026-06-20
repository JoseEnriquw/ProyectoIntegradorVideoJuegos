using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UHFPS.Tools;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [Docs("https://docs.twgamesdev.com/uhfps/getting-started")]
    public class PlayerPresenceManager : Singleton<PlayerPresenceManager>
    {
        public enum UnlockType
        {
            /// <summary>
            /// Player will be unlocked at the start or after the game state is loaded.
            /// </summary>
            Automatically,

            /// <summary>
            /// Player will be unlocked after calling the <b>UnlockPlayer()</b> function.
            /// </summary>
            Manually
        }

        public UnlockType PlayerUnlockType = UnlockType.Automatically;
        public GameObject Player;

        public float WaitFadeOutTime = 0.5f;
        public float FadeOutSpeed = 3f;

        private PlayerComponent[] playerComponents;
        private GameManager gameManager;
        private GameObject activeCamera;

        public bool PlayerIsUnlocked;
        public bool GameStateIsLoaded;
        public bool IsCameraSwitched;

        private bool isBackgroundFadedOut;
        public bool IsBackgroundFadedOut
        {
            get => isBackgroundFadedOut;
            set => isBackgroundFadedOut = value;
        }

        private PlayerManager playerManager;

        public PlayerManager PlayerManager
        {
            get
            {
                if (playerManager == null)
                    playerManager = Player.GetComponent<PlayerManager>();

                return playerManager;
            }
        }

        public PlayerStateMachine StateMachine => PlayerManager.PlayerStateMachine;
        public LookController LookController => PlayerManager.LookController;

        /// <summary>
        /// Check if player is unlocked and the active camera is player camera.
        /// </summary>
        public bool IsUnlockedAndCamera => PlayerIsUnlocked && !IsCameraSwitched;

        public Camera PlayerCamera => PlayerManager.MainCamera;

        public CinemachineCamera PlayerVirtualCamera => PlayerManager.MainVirtualCamera;

        private void OnEnable()
        {
            SaveGameManager.Instance.OnGameLoaded += () =>
            {
                // Executed only when loading game state
                UnlockPlayer();
                GameStateIsLoaded = true;
            };
        }

        private void Awake()
        {
            gameManager = GetComponent<GameManager>();
            playerComponents = Player.GetComponentsInChildren<PlayerComponent>(true);
            IntroDebugger.Log($"[PlayerPresenceManager] Awake: found {playerComponents.Length} PlayerComponents.");

            // keep player frozen at start
            FreezePlayer(true);
        }

        private void Start()
        {
            if (!SaveGameManager.GameActuallyLoad)
            {
                // If game state is not loaded, unlock player based on the selected unlock type
                if (PlayerUnlockType == UnlockType.Automatically)
                    UnlockPlayer();
            }
        }

        public T Component<T>()
        {
            return Player.GetComponentInChildren<T>(true);
        }

        public T[] Components<T>()
        {
            return Player.GetComponentsInChildren<T>(true);
        }

        public void FreezeMovement(bool freeze)
        {
            StateMachine.SetEnabled(!freeze);
        }

        public void FreezeLook(bool freeze, bool showCursor = false)
        {
            GameTools.ShowCursor(!showCursor, showCursor);
            LookController.SetEnabled(!freeze);
        }

        public void FreezePlayer(bool freeze, bool showCursor = false)
        {
            IntroDebugger.Log($"[PlayerPresenceManager] FreezePlayer: freeze={freeze}, showCursor={showCursor}");
            GameTools.ShowCursor(!showCursor, showCursor);

            foreach (var component in playerComponents)
            {
                component.SetEnabled(!freeze);
            }
        }

        public void FadeBackground(bool fadeOut, Action onBackgroundFade)
        {
            StartCoroutine(StartFadeBackground(fadeOut, onBackgroundFade));
        }

        IEnumerator StartFadeBackground(bool fadeOut, Action onBackgroundFade)
        {
            yield return gameManager.StartBackgroundFade(fadeOut, WaitFadeOutTime, FadeOutSpeed);
            isBackgroundFadedOut = fadeOut;
            onBackgroundFade?.Invoke();
        }

        public void UnlockPlayer()
        {
            IntroDebugger.Log("[PlayerPresenceManager] UnlockPlayer called.");
            StartCoroutine(DoUnlockPlayer());
        }

        private IEnumerator DoUnlockPlayer()
        {
            IntroDebugger.Log($"[PlayerPresenceManager] DoUnlockPlayer started. isBackgroundFadedOut={isBackgroundFadedOut}");
            if (!isBackgroundFadedOut)
            {
                IntroDebugger.Log("[PlayerPresenceManager] DoUnlockPlayer: Background is not faded out, waiting for fade...");
                yield return gameManager.StartBackgroundFade(true, WaitFadeOutTime, FadeOutSpeed);
                isBackgroundFadedOut = true;
            }

            PlayerIsUnlocked = true;

            // Check if there is any active popup or if the game is paused.
            // If so, we do NOT unfreeze the player now, because the popup/pause menu expects the player to remain frozen.
            // The popup/pause menu will call FreezePlayer(false) when it is dismissed.
            bool anyPopupShown = (ControlsPopupPanel.HasReference && ControlsPopupPanel.Instance.IsShown) 
                || (PopupInfoPanel.HasReference && PopupInfoPanel.Instance.IsShown) 
                || (gameManager != null && gameManager.IsPaused);

            IntroDebugger.Log($"[PlayerPresenceManager] DoUnlockPlayer: anyPopupShown={anyPopupShown} (ControlsPopup={ControlsPopupPanel.HasReference && ControlsPopupPanel.Instance.IsShown}, PopupInfo={PopupInfoPanel.HasReference && PopupInfoPanel.Instance.IsShown}, Paused={gameManager != null && gameManager.IsPaused})");

            if (!anyPopupShown)
            {
                FreezePlayer(false);
            }
            else
            {
                IntroDebugger.Log("[PlayerPresenceManager] DoUnlockPlayer: Player remains frozen because a popup is open or game is paused.");
            }
        }

        public (Vector3 position, Vector2 rotation) GetPlayerTransform()
        {
            return (Player.transform.position, LookController.LookRotation);
        }

        public void SetPlayerTransform(Vector3 position, Vector2 rotation)
        {
            Player.transform.SetPositionAndRotation(position, Quaternion.identity);
            LookController.LookRotation = rotation;
            Physics.SyncTransforms(); // sync position to character controller
        }

        public void SetPlayerPositionAndLook(Vector3 position, Vector2 eulerLook)
        {
            Player.transform.SetPositionAndRotation(position, Quaternion.identity);
            LookController.ApplyEulerLook(eulerLook);
            Physics.SyncTransforms(); // sync position to character controller
        }

        public void Teleport(Vector3 position)
        {
            Player.transform.SetPositionAndRotation(position, Quaternion.identity);
            Physics.SyncTransforms();
        }

        public void Teleport(Vector3 position, bool colliderState = true)
        {
            Player.transform.position = position;
            StateMachine.PlayerCollider.enabled = colliderState;

            // sync position to character controller
            if (colliderState) Physics.SyncTransforms();
        }

        public void Teleport(Vector3 position, Vector2 eulerLook, bool colliderState = true)
        {
            Player.transform.SetPositionAndRotation(position, Quaternion.identity);
            StateMachine.PlayerCollider.enabled = colliderState;
            LookController.ApplyEulerLook(eulerLook);

            // sync position to character controller
            if (colliderState) Physics.SyncTransforms();
        }

        public void SwitchActiveCamera(GameObject virtualCameraObj, float fadeSpeed, Action onBackgroundFade)
        {
            StartCoroutine(SwitchCamera(virtualCameraObj, fadeSpeed, onBackgroundFade, null));
            IsCameraSwitched = true;
        }

        public void SwitchActiveCamera(GameObject virtualCameraObj, float fadeSpeed, Action onBackgroundFade,
            Action onFadeComplete)
        {
            StartCoroutine(SwitchCamera(virtualCameraObj, fadeSpeed, onBackgroundFade, onFadeComplete));
            IsCameraSwitched = true;
        }

        public void SwitchToPlayerCamera(float fadeSpeed, Action onBackgroundFade)
        {
            StartCoroutine(SwitchCamera(null, fadeSpeed, onBackgroundFade, null));
        }

        public void SwitchToPlayerCamera(float fadeSpeed, Action onBackgroundFade, Action onFadeComplete)
        {
            StartCoroutine(SwitchCamera(null, fadeSpeed, onBackgroundFade, onFadeComplete));
        }

        public IEnumerator SwitchCamera(GameObject cameraObj, float fadeSpeed)
        {
            yield return gameManager.StartBackgroundFade(false, fadeSpeed: fadeSpeed);
            playerManager.MainVirtualCamera.gameObject.SetActive(cameraObj == null);

            if (cameraObj != null) playerManager.PlayerItems.DeactivateCurrentItem();
            else playerManager.PlayerItems.ActivatePreviouslyDeactivatedItem();

            if (activeCamera != null) activeCamera.SetActive(false);
            if (cameraObj != null) cameraObj.SetActive(cameraObj != null);
            activeCamera = cameraObj;

            yield return new WaitForEndOfFrame();
            yield return gameManager.StartBackgroundFade(true, fadeSpeed: fadeSpeed);

            IsCameraSwitched = cameraObj != null; // check if camera switched to player camera
        }

        private IEnumerator SwitchCamera(GameObject cameraObj, float fadeSpeed, Action onBackgroundFade,
            Action onFadeComplete)
        {
            yield return gameManager.StartBackgroundFade(false, fadeSpeed: fadeSpeed);
            playerManager.MainVirtualCamera.gameObject.SetActive(cameraObj == null);

            if (cameraObj != null) playerManager.PlayerItems.DeactivateCurrentItem();
            else playerManager.PlayerItems.ActivatePreviouslyDeactivatedItem();

            if (activeCamera != null) activeCamera.SetActive(false);
            if (cameraObj != null) cameraObj.SetActive(cameraObj != null);
            activeCamera = cameraObj;

            onBackgroundFade?.Invoke();

            yield return new WaitForEndOfFrame();
            yield return gameManager.StartBackgroundFade(true, fadeSpeed: fadeSpeed);

            IsCameraSwitched = cameraObj != null; // check if camera switched to player camera

            yield return new WaitForSeconds(0.1f);
            onFadeComplete?.Invoke();
        }
    }
}