using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Máquina de estados finita genérica.
    /// Gestiona estados con transiciones, lifecycle (Enter/Update/Exit), y búsqueda por key/tipo.
    /// Independiente de CharacterController o NavMeshAgent.
    /// </summary>
    public abstract class StateMachineCore : MonoBehaviour
    {
        /// <summary>Datos de un estado registrado.</summary>
        public class RegisteredState
        {
            public string Key;
            public Type StateType;
            public FSMStateBase State;
            public bool IsEnabled = true;
        }

        private readonly Dictionary<string, RegisteredState> statesByKey = new();
        private readonly Dictionary<Type, RegisteredState> statesByType = new();

        private RegisteredState currentState;
        private RegisteredState previousState;
        private bool stateEntered;

        // --- Propiedades públicas ---
        public RegisteredState Current => currentState;
        public RegisteredState Previous => previousState;
        public string CurrentStateKey => currentState?.Key ?? "None";
        public bool IsInState(string key) => currentState?.Key == key;

        // --- Eventos ---
        public event Action<string> OnStateChanged;

        /// <summary>
        /// Registra un estado en la máquina. Llamar desde Awake.
        /// </summary>
        protected void RegisterState(string key, FSMStateBase state)
        {
            var registered = new RegisteredState
            {
                Key = key,
                StateType = state.GetType(),
                State = state
            };

            statesByKey[key] = registered;
            statesByType[state.GetType()] = registered;
        }

        /// <summary>
        /// Establece el estado inicial de la máquina.
        /// </summary>
        protected void SetInitialState(string key)
        {
            if (statesByKey.TryGetValue(key, out var state))
            {
                currentState = state;
                stateEntered = false;
            }
        }

        /// <summary>
        /// Tick de la máquina (llamar desde Update).
        /// </summary>
        protected void UpdateStateMachine()
        {
            if (!stateEntered)
            {
                currentState?.State.OnStateEnter();
                OnStateChanged?.Invoke(CurrentStateKey);
                stateEntered = true;
            }
            else if (currentState != null)
            {
                currentState.State.OnStateUpdate();

                // Verificar transiciones
                if (currentState.State.Transitions != null)
                {
                    foreach (var transition in currentState.State.Transitions)
                    {
                        if (transition.Evaluate() && currentState.Key != transition.NextStateKey)
                        {
                            if (statesByKey.TryGetValue(transition.NextStateKey, out var nextState))
                            {
                                if (nextState.IsEnabled)
                                {
                                    ChangeState(nextState);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Tick de FixedUpdate para la máquina.</summary>
        protected void FixedUpdateStateMachine()
        {
            currentState?.State.OnStateFixedUpdate();
        }

        // --- Cambio de estado ---

        /// <summary>Cambiar estado por key.</summary>
        public void ChangeState(string key)
        {
            if (statesByKey.TryGetValue(key, out var state))
            {
                if (state.IsEnabled && (currentState == null || currentState.Key != key))
                    ChangeState(state);
            }
            else
            {
                Debug.LogError($"[StateMachine] State '{key}' not found.");
            }
        }

        /// <summary>Cambiar estado por tipo.</summary>
        public void ChangeState<T>() where T : FSMStateBase
        {
            if (statesByType.TryGetValue(typeof(T), out var state))
            {
                if (state.IsEnabled && (currentState == null || currentState.StateType != typeof(T)))
                    ChangeState(state);
            }
            else
            {
                Debug.LogError($"[StateMachine] State of type '{typeof(T).Name}' not found.");
            }
        }

        /// <summary>Cambiar al estado anterior.</summary>
        public void ChangeToPreviousState()
        {
            if (previousState != null && previousState != currentState && previousState.IsEnabled)
            {
                var temp = currentState;
                currentState?.State.OnStateExit();
                currentState = previousState;
                previousState = temp;
                stateEntered = false;
            }
        }

        private void ChangeState(RegisteredState nextState)
        {
            currentState?.State.OnStateExit();
            previousState = currentState;
            currentState = nextState;
            stateEntered = false;
        }

        /// <summary>Habilitar/deshabilitar un estado.</summary>
        public void SetStateEnabled(string key, bool enabled)
        {
            if (statesByKey.TryGetValue(key, out var state))
                state.IsEnabled = enabled;
        }

        /// <summary>Obtener un estado por tipo.</summary>
        public T GetState<T>() where T : FSMStateBase
        {
            return statesByType.TryGetValue(typeof(T), out var state) ? (T)state.State : null;
        }
    }

    /// <summary>
    /// Base para un estado de la máquina de estados.
    /// Override los métodos virtuales para definir el comportamiento.
    /// </summary>
    public abstract class FSMStateBase
    {
        /// <summary>Datos opcionales pasados al estado al entrar.</summary>
        public StorableCollection StateData { get; set; }

        /// <summary>Transiciones desde este estado.</summary>
        public List<StateTransition> Transitions { get; set; }

        /// <summary>Si puede transicionar cuando el componente está deshabilitado.</summary>
        public virtual bool CanTransitionWhenDisabled => false;

        public virtual void OnStateEnter() { }
        public virtual void OnStateUpdate() { }
        public virtual void OnStateFixedUpdate() { }
        public virtual void OnStateExit() { }
        public virtual void OnPlayerDeath() { }
    }

    /// <summary>
    /// Define una transición condicional entre estados.
    /// </summary>
    public class StateTransition
    {
        public string NextStateKey;
        private Func<bool> condition;

        public StateTransition(string nextState, Func<bool> condition)
        {
            NextStateKey = nextState;
            this.condition = condition;
        }

        /// <summary>Evalúa si la transición debe ocurrir.</summary>
        public bool Evaluate() => condition?.Invoke() ?? false;
    }
}
