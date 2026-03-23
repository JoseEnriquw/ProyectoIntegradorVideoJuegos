using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Componente central del juego listo para usar. 
    /// Implementa IGameContext. Colócalo en tu objeto '[MANAGERS]'.
    /// Se encarga de pausar el juego, bloquear al jugador, mostrar el cursor y cambiar de escenas.
    /// </summary>
    public class GameManagerCore : MonoBehaviour, IGameContext
    {
        public static GameManagerCore Instance { get; private set; }

        [Header("Configuración de Escenas")]
        public string MainMenuSceneName = "MainMenu";

        // --- Estado Privado ---
        private bool isPaused;
        private bool isInventoryShown;
        public bool PlayerDied { get; private set; }

        // --- Eventos ---
        private Action<bool> onPausedEvent;
        private Action<bool> onInventoryShownEvent;

        // Diccionario opcional para inyectar submódulos
        private readonly Dictionary<Type, ManagerModuleCore> modules = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Opcional: Para que los managers sobrevivan al cargar otra escena
            // DontDestroyOnLoad(gameObject);

            // Iniciar con cursor bloqueado y oculto
            LockCursor(true);
        }

        private void Update()
        {
            // Fallback rápido si quieres un botón fijo de pausa genérico (P)
            if (Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }

        // ==========================================
        // IMPLEMENTACIÓN DE IGameContext
        // ==========================================

        public bool IsPaused => isPaused;
        public bool IsInventoryShown => isInventoryShown;

        public void FreezePlayer(bool freeze, bool showCursor = false)
        {
            // Opcional: Buscar FirstPersonController y desactivarlo
            var player = FindObjectOfType<FirstPerson.FirstPersonController>();
            if (player != null)
            {
                player.Enabled = !freeze;
            }

            var look = FindObjectOfType<FirstPerson.FirstPersonLook>();
            if (look != null)
            {
                look.enabled = !freeze;
            }

            if (showCursor)
                LockCursor(false);
            else if (!freeze)
                LockCursor(true); // Solo volver a bloquear si nos descongelan
        }

        public void LockInput(bool locked)
        {
            // Análogo a FreezePlayer, útil si quieres que mire pero no camine, etc.
            var player = FindObjectOfType<FirstPerson.FirstPersonController>();
            if (player != null) player.Enabled = !locked;
        }

        public void SubscribePauseEvent(Action<bool> onPaused) => onPausedEvent += onPaused;
        public void SubscribeInventoryEvent(Action<bool> onInventoryShown) => onInventoryShownEvent += onInventoryShown;

        public T GetModule<T>() where T : ManagerModuleCore
        {
            Type type = typeof(T);
            if (modules.TryGetValue(type, out var module))
                return (T)module;
            return null;
        }

        public void LoadNextLevel(string sceneName)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(MainMenuSceneName);
        }

        public void ShowHintMessage(string text, float duration)
        {
            // Delegamos la carga visual puramente al UIManager
            if (UI.UIManager.Instance != null)
                UI.UIManager.Instance.ShowHintMessage(text, duration);
            else
                Debug.Log($"[GameManager Hint]: {text}");
        }

        public void ShowItemPickupMessage(string text, float duration)
        {
            // Delegamos al UIManager (puede usar el mismo texto de Hint o puedes añadir uno nuevo)
            if (UI.UIManager.Instance != null)
                UI.UIManager.Instance.ShowHintMessage(text, duration);
            else
                Debug.Log($"[GameManager Item]: {text}");
        }

        // ==========================================
        // UTILIDADES EXTRAS
        // ==========================================

        public void TogglePause()
        {
            if (PlayerDied) return;

            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            FreezePlayer(isPaused, isPaused);
            onPausedEvent?.Invoke(isPaused);
        }

        public void SetPlayerDied()
        {
            PlayerDied = true;
            FreezePlayer(true, true);
        }

        private void LockCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }
    }

}
