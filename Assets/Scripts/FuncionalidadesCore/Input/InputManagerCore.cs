using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Constantes de nombres de acciones de input.
    /// Centraliza todos los nombres para evitar magic strings.
    /// </summary>
    public static class Controls
    {
        public const string MOVEMENT = "Movement";
        public const string LOOK = "Look";
        public const string JUMP = "Jump";
        public const string SPRINT = "Sprint";
        public const string CROUCH = "Crouch";
        public const string INTERACT = "Interact";
        public const string EXAMINE = "Examine";
        public const string FIRE = "Fire";
        public const string AIM = "Aim";
        public const string RELOAD = "Reload";
        public const string INVENTORY = "Inventory";
        public const string FLASHLIGHT = "Flashlight";
        public const string PAUSE = "Pause";
        public const string USE = "Use";
    }

    /// <summary>
    /// InputManager desacoplado - wrapper de Unity Input System.
    /// Implementa IInputProvider para que otros scripts no dependan de esta clase directamente.
    /// </summary>
    public class InputManagerCore : Singleton<InputManagerCore>, IInputProvider
    {
        [Header("Input Configuration")]
        public InputActionAsset inputActions;
        public bool debugMode;

        private readonly Dictionary<string, InputAction> cachedActions = new();
        private readonly List<string> pressedActions = new();
        private readonly Dictionary<string, bool> toggledActions = new();

        private void Awake()
        {
            if (!inputActions)
            {
                Debug.LogError("[InputManager] InputActionAsset is not assigned!");
                return;
            }

            // Cache all actions
            foreach (var map in inputActions.actionMaps)
            {
                foreach (var action in map.actions)
                {
                    cachedActions[action.name] = action;
                }
            }

            inputActions.Enable();
            if (debugMode) Debug.Log("[InputManager] Initialized and enabled.");
        }

        private InputAction GetAction(string name)
        {
            if (cachedActions.TryGetValue(name, out var action))
                return action;

            Debug.LogError($"[InputManager] Action '{name}' not found!");
            return null;
        }

        // --- IInputProvider ---

        public T ReadInput<T>(string actionName) where T : struct
        {
            var action = GetAction(actionName);
            return action != null ? action.ReadValue<T>() : default;
        }

        public bool ReadInput<T>(string actionName, out T value) where T : struct
        {
            var action = GetAction(actionName);
            if (action != null && action.IsPressed())
            {
                value = action.ReadValue<T>();
                return true;
            }
            value = default;
            return false;
        }

        public bool ReadButton(string actionName)
        {
            var action = GetAction(actionName);
            if (action == null) return false;

            if (action.type == InputActionType.Button)
                return Convert.ToBoolean(action.ReadValueAsObject());

            return false;
        }

        public bool ReadButtonOnce(string key, string actionName)
        {
            string inputKey = actionName + "." + key;

            if (ReadButton(actionName))
            {
                if (!pressedActions.Contains(inputKey))
                {
                    pressedActions.Add(inputKey);
                    return true;
                }
            }
            else
            {
                pressedActions.Remove(inputKey);
            }

            return false;
        }

        public bool ReadButtonToggle(string key, string actionName)
        {
            string inputKey = actionName + "." + key;

            if (ReadButtonOnce(key, actionName))
            {
                if (!toggledActions.ContainsKey(inputKey))
                    toggledActions.Add(inputKey, true);
                else if (!toggledActions[inputKey])
                    toggledActions.Remove(inputKey);
            }
            else if (toggledActions.ContainsKey(inputKey))
            {
                toggledActions[inputKey] = false;
            }

            return toggledActions.ContainsKey(inputKey);
        }

        public void Performed(string actionName, Action<InputAction.CallbackContext> performed)
        {
            var action = GetAction(actionName);
            if (action != null)
                action.performed += performed;
        }

        public bool AnyKeyPressed()
        {
            Mouse mouse = Mouse.current;
            return Keyboard.current.anyKey.isPressed
                || mouse.leftButton.isPressed
                || mouse.rightButton.isPressed;
        }

        // --- Utilidades adicionales ---

        /// <summary>Resetear un botón toggle.</summary>
        public void ResetToggledButton(string key, string actionName)
        {
            string inputKey = actionName + "." + key;
            toggledActions.Remove(inputKey);
        }

        /// <summary>Resetear todos los toggles.</summary>
        public void ResetToggledButtons() => toggledActions.Clear();

        /// <summary>Encontrar una acción por nombre.</summary>
        public InputAction FindAction(string name)
        {
            return inputActions.FindAction(name);
        }
    }
}
