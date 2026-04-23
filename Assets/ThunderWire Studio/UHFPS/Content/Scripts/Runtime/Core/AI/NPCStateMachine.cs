using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Disposables;
using UnityEngine;
using UHFPS.Tools;
using UHFPS.Scriptable;
using UnityEngine.AI;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(NavMeshAgent))]
    [Docs("https://docs.twgamesdev.com/uhfps/guides/state-machines/adding-ai-states")]
    public class NPCStateMachine : MonoBehaviour
    {
        #region Getters / Setters
        private NavMeshAgent m_NavMeshAgent;
        public NavMeshAgent Agent
        {
            get
            {
                if (m_NavMeshAgent == null)
                    m_NavMeshAgent = GetComponent<NavMeshAgent>();

                return m_NavMeshAgent;
            }
        }

        private PlayerStateMachine m_Player;
        public PlayerStateMachine Player
        {
            get
            {
                if (m_Player == null)
                    m_Player = PlayerPresenceManager.Instance.Player.GetComponent<PlayerStateMachine>();

                return m_Player;
            }
        }

        private PlayerHealth m_PlayerHealth;
        public PlayerHealth PlayerHealth
        {
            get
            {
                if (m_PlayerHealth == null)
                    m_PlayerHealth = Player.GetComponent<PlayerHealth>();

                return m_PlayerHealth;
            }
        }

        private PlayerManager m_PlayerManager;
        public PlayerManager PlayerManager
        {
            get
            {
                if (m_PlayerManager == null)
                    m_PlayerManager = Player.GetComponent<PlayerManager>();

                return m_PlayerManager;
            }
        }

        public State? CurrentState => currentState;

        public State? PreviousState => previousState;

        public string CurrentStateKey => CurrentState?.StateData.StateAsset.StateKey;

        public bool RotateAgentManually { get; set; }
        #endregion

        public enum NPCTypeEnum { Enemy, Ally }

        public struct State
        {
            public AIStateData StateData;
            public FSMAIState FSMState;
        }

        public AIStatesGroup StatesAsset;
        public AIStatesGroup StatesAssetRuntime;

        public Animator Animator;
        public Transform HeadBone;
        public LayerMask SightsMask;
        public NPCTypeEnum NPCType;

        [Range(0, 179)] public float SightsFOV = 110;
        public float SightsDistance = 15;
        public float SteeringSpeed = 6f;

        public bool ShowDestination;
        public bool ShowSights;

        private MultiKeyDictionary<string, Type, State> aiStates;
        private readonly Subject<string> messages = new();
        private readonly CompositeDisposable disposables = new();

        private State? currentState;
        private State? previousState;
        private bool stateEntered;

        public bool IsPlayerDead { get; private set; }

        private void OnDestroy()
        {
            disposables.Dispose();
        }

        private void Awake()
        {
            aiStates = new MultiKeyDictionary<string, Type, State>();
            StatesAssetRuntime = Instantiate(StatesAsset);

            if (StatesAsset != null)
            {
                // initialize all states
                foreach (var state in StatesAssetRuntime.GetStates(this))
                {
                    Type stateType = state.StateData.StateAsset.GetType();
                    string stateKey = state.StateData.StateAsset.StateKey;
                    aiStates.Add(stateKey, stateType, state);
                }

                // select initial ai state
                if (aiStates.Count > 0)
                {
                    stateEntered = false;
                    ChangeState(aiStates.subDictionary.Keys.First());
                }
            }
        }

        private void Update()
        {
            if (!stateEntered)
            {
                // enter state
                currentState?.FSMState.OnStateEnter();
                stateEntered = true;
            }
            else if (currentState != null)
            {
                // update state
                currentState?.FSMState.OnStateUpdate();

                // check state transitions
                if (currentState.Value.FSMState.Transitions != null)
                {
                    foreach (var transition in currentState.Value.FSMState.Transitions)
                    {
                        if (transition.Value && currentState.GetType() != transition.NextStateType)
                        {
                            ChangeState(transition.NextStateType);
                            break;
                        }
                    }
                }
            }

            // player death event
            if(currentState != null && !IsPlayerDead && PlayerHealth.IsDead)
            {
                currentState.Value.FSMState.OnPlayerDeath();
                IsPlayerDead = true;
            }

            // agent rotation
            if (RotateAgentManually)
            {
                Agent.updateRotation = false;
                RotateManually();
            }
            else
            {
                Agent.updateRotation = true;
            }
        }

        /// <summary>
        /// Rotate agent manually.
        /// </summary>
        private void RotateManually()
        {
            Vector3 target = Agent.steeringTarget;
            Vector3 direction = (target - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, lookRotation, Time.deltaTime * SteeringSpeed);
        }

        /// <summary>
        /// Send a message to the state machine so that you can catch it in the state.
        /// </summary>
        public void SendAnimationMessage(string message)
        {
            messages.OnNext(message);
        }

        /// <summary>
        /// Catch animation messages to perform actions.
        /// </summary>
        public void CatchMessage(string message, Action action)
        {
            disposables.Add(messages.Where(msg => msg == message).Subscribe(_ => action?.Invoke()));
        }

        /// <summary>
        /// Change AI FSM state.
        /// </summary>
        public void ChangeState<TState>() where TState : AIStateAsset
        {
            if (aiStates.TryGetValue(typeof(TState), out State state))
            {
                if ((currentState == null || !currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    currentState?.FSMState.OnStateExit();
                    if (currentState.HasValue) previousState = currentState;
                    currentState = state;
                    stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with type '{typeof(TState).Name}'");
        }

        /// <summary>
        /// Change AI FSM state.
        /// </summary>
        public void ChangeState(Type nextState)
        {
            if (aiStates.TryGetValue(nextState, out State state))
            {
                if ((currentState == null || !currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    currentState?.FSMState.OnStateExit();
                    if (currentState.HasValue) previousState = currentState;
                    currentState = state;
                    stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with type '{nextState.Name}'");
        }

        /// <summary>
        /// Change AI FSM state.
        /// </summary>
        public void ChangeState(string nextState)
        {
            if (aiStates.TryGetValue(nextState, out State state))
            {
                if ((currentState == null || !currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    currentState?.FSMState.OnStateExit();
                    if (currentState.HasValue) previousState = currentState;
                    currentState = state;
                    stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with key '{nextState}'");
        }

        /// <summary>
        /// Change AI FSM state and set the state data.
        /// </summary>
        public void ChangeState(string nextState, StorableCollection stateData)
        {
            if (aiStates.TryGetValue(nextState, out State state))
            {
                if ((currentState == null || !currentState.Value.Equals(state)) && state.StateData.IsEnabled)
                {
                    currentState?.FSMState.OnStateExit();
                    if (currentState.HasValue) previousState = currentState;
                    state.FSMState.StateData = stateData;
                    currentState = state;
                    stateEntered = false;
                }
                return;
            }

            throw new MissingReferenceException($"Could not find a state with key '{nextState}'");
        }

        /// <summary>
        /// Check if current state is of the specified type.
        /// </summary>
        public bool IsCurrent(Type stateType)
        {
            return currentState.Value.StateData.StateAsset.GetType() == stateType;
        }

        /// <summary>
        /// Check if current state matches the specified state key.
        /// </summary>
        public bool IsCurrent(string stateKey)
        {
            return currentState.Value.StateData.StateAsset.StateKey == stateKey;
        }

        private void OnDrawGizmosSelected()
        {
            if (ShowSights)
            {
                // Dibujar el cono de visión real — igual a lo que usa SeesPlayer() en runtime
                int segments = 30;
                float halfFOV = SightsFOV * 0.5f;
                Vector3 origin = transform.position;

                // Rayos laterales del cono
                Vector3 fovLeftDir  = Quaternion.AngleAxis(-halfFOV, Vector3.up) * transform.forward;
                Vector3 fovRightDir = Quaternion.AngleAxis( halfFOV, Vector3.up) * transform.forward;

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(origin, fovLeftDir  * SightsDistance);
                Gizmos.DrawRay(origin, fovRightDir * SightsDistance);

                // Arco del extremo del cono (para ver claramente hasta dónde llega la detección)
                Vector3 prevPoint = origin + fovLeftDir * SightsDistance;
                for (int i = 1; i <= segments; i++)
                {
                    float t = (float)i / segments;
                    float angle = Mathf.Lerp(-halfFOV, halfFOV, t);
                    Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * transform.forward;
                    Vector3 nextPoint = origin + dir * SightsDistance;
                    Gizmos.DrawLine(prevPoint, nextPoint);
                    prevPoint = nextPoint;
                }

                // Relleno semitransparente del cono
                Gizmos.color = new Color(1f, 1f, 0f, 0.05f);
                for (int i = 0; i < segments; i++)
                {
                    float t0 = (float)i / segments;
                    float t1 = (float)(i + 1) / segments;
                    float angle0 = Mathf.Lerp(-halfFOV, halfFOV, t0);
                    float angle1 = Mathf.Lerp(-halfFOV, halfFOV, t1);
                    Vector3 dir0 = Quaternion.AngleAxis(angle0, Vector3.up) * transform.forward;
                    Vector3 dir1 = Quaternion.AngleAxis(angle1, Vector3.up) * transform.forward;
                    // Triángulo: origin → punto0 → punto1
                    Gizmos.DrawLine(origin, origin + dir0 * SightsDistance);
                    Gizmos.DrawLine(origin + dir0 * SightsDistance, origin + dir1 * SightsDistance);
                }
            }

            // Dibuja todos los radios de detección leyendo directamente los assets del NPC
            if (StatesAsset != null)
            {
                foreach (var stateData in StatesAsset.AIStates)
                {
                    // ─── Radio que INICIA la persecución (desde estado de patrulla) ───
                    if (stateData.StateAsset is UHFPS.Runtime.States.EstadoPatrullajeAI patrullaje)
                    {
                        // Relleno naranja semitransparente
                        Gizmos.color = new Color(1f, 0.5f, 0f, 0.15f);
                        Gizmos.DrawSphere(transform.position, patrullaje.distanciaDeteccionCercana);
                        // Borde naranja sólido
                        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
                        Gizmos.DrawWireSphere(transform.position, patrullaje.distanciaDeteccionCercana);
#if UNITY_EDITOR
                        UnityEditor.Handles.color = new Color(1f, 0.5f, 0f, 1f);
                        UnityEditor.Handles.Label(
                            transform.position + transform.right * patrullaje.distanciaDeteccionCercana,
                            "Detección\ncercana (inicia)");
#endif
                    }

                    // ─── Radio de seguimiento durante persecución ───
                    if (stateData.StateAsset is UHFPS.Runtime.States.EstadoPersecucionAI persecucion)
                    {
                        // Relleno rojo semitransparente
                        Gizmos.color = new Color(1f, 0f, 0f, 0.1f);
                        Gizmos.DrawSphere(transform.position, persecucion.radioDeteccionCercana);
                        // Borde rojo sólido
                        Gizmos.color = new Color(1f, 0f, 0f, 1f);
                        Gizmos.DrawWireSphere(transform.position, persecucion.radioDeteccionCercana);
#if UNITY_EDITOR
                        UnityEditor.Handles.color = new Color(1f, 0f, 0f, 1f);
                        UnityEditor.Handles.Label(
                            transform.position - transform.right * persecucion.radioDeteccionCercana,
                            "Radio seguimiento\n(durante persec.)");
#endif

                        // ─── Radio de ataque ───
                        Gizmos.color = new Color(1f, 0f, 1f, 1f);
                        Gizmos.DrawWireSphere(transform.position, persecucion.distanciaDeAtaque);
#if UNITY_EDITOR
                        UnityEditor.Handles.color = new Color(1f, 0f, 1f, 1f);
                        UnityEditor.Handles.Label(
                            transform.position + Vector3.forward * persecucion.distanciaDeAtaque,
                            "Ataque");
#endif
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!ShowDestination || !Application.isPlaying) 
                return;

            Vector3 targetPosition = Agent.destination;
            Color outerColor = Color.green;
            Color innerColor = Color.green.Alpha(0.01f);
            GizmosE.DrawDisc(targetPosition, 0.5f, outerColor, innerColor);
        }
    }
}