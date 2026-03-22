using System;
using UnityEngine.InputSystem;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Abstrae el InputManager, proporcionando lectura de input sin depender de implementación concreta.
    /// </summary>
    public interface IInputProvider
    {
        /// <summary>Leer un valor de input tipado.</summary>
        T ReadInput<T>(string actionName) where T : struct;

        /// <summary>Leer un valor de input si el botón está presionado.</summary>
        bool ReadInput<T>(string actionName, out T value) where T : struct;

        /// <summary>Leer un botón como booleano.</summary>
        bool ReadButton(string actionName);

        /// <summary>Leer un botón una sola vez (no repite mientras se mantiene presionado).</summary>
        bool ReadButtonOnce(string key, string actionName);

        /// <summary>Leer un botón como toggle on/off.</summary>
        bool ReadButtonToggle(string key, string actionName);

        /// <summary>Suscribirse al evento performed de una acción.</summary>
        void Performed(string actionName, Action<InputAction.CallbackContext> performed);

        /// <summary>Verificar si alguna tecla está presionada.</summary>
        bool AnyKeyPressed();
    }
}
