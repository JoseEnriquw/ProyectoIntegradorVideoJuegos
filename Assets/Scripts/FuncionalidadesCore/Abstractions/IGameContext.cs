using System;
using System.Collections;
using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Abstrae el GameManager central del juego.
    /// Expone funcionalidad de estado del juego sin depender de UI concreta.
    /// </summary>
    public interface IGameContext
    {
        // --- Estado del juego ---
        bool IsPaused { get; }
        bool IsInventoryShown { get; }
        bool PlayerDied { get; }

        // --- Control del jugador ---
        void FreezePlayer(bool freeze, bool showCursor = false);
        void LockInput(bool locked);

        // --- Suscripción a eventos (sin UI) ---
        void SubscribePauseEvent(Action<bool> onPaused);
        void SubscribeInventoryEvent(Action<bool> onInventoryShown);

        // --- Módulos ---
        T GetModule<T>() where T : ManagerModuleCore;

        // --- Gestión de escenas ---
        void LoadNextLevel(string sceneName);
        void RestartGame();
        void GoToMainMenu();

        // --- Mensajes genéricos (la implementación decide cómo mostrar) ---
        void ShowHintMessage(string text, float duration);
        void ShowItemPickupMessage(string text, float duration);

        // --- Coroutine runner ---
        Coroutine StartCoroutine(IEnumerator coroutine);
    }
}
