using UnityEngine;
using UHFPS.Scriptable;
using UnityEngine.AI;

namespace UHFPS.Runtime.States
{
    [CreateAssetMenu(fileName = "EstadoCorrerWaypointsAI", menuName = "UHFPS/AI States/EstadoCorrerWaypointsAI")]
    public class EstadoCorrerWaypointsAI : AIStateAsset
    {
        [Header("Configuracion de Carrera hacia Waypoints")]
        public float velocidadCarrera = 3.5f;

        [Tooltip("Distancia para cambiar al siguiente waypoint antes de frenar")]
        public float distanciaCambio = 0.5f;

        [Header("Delay Inicial")]
        [Tooltip("Tiempo antes de empezar a moverse (para animacion de giro)")]
        public float delayInicial = 1.5f;

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new EstadoCorrerWaypointsAI_State(machine, this, group);
        }

        public override string StateKey => "CorrerWaypointsAI";
        public override string Name => "Estado de Carrera Hacia Waypoints";

        public class EstadoCorrerWaypointsAI_State : FSMAIState
        {
            private EstadoCorrerWaypointsAI asset;
            private NavMeshAgent agent;
            private Animator animator;
            private CustomNPCStateGroup customGroup;

            private AIWaypointsGroup currentGroup;
            private int currentWaypointIndex = 0;

            private bool recorridoFinalizado = false;

            // Delay inicial
            private bool esperandoInicio = true;
            private float timerInicio = 0f;

            public EstadoCorrerWaypointsAI_State(NPCStateMachine machine, EstadoCorrerWaypointsAI stateAsset, AIStatesGroup group) : base(machine)
            {
                asset = stateAsset;
                agent = machine.GetComponent<NavMeshAgent>();
                animator = machine.Animator;
                customGroup = group as CustomNPCStateGroup;
            }

            public override void OnStateEnter()
            {
                esperandoInicio = true;

                // escuchar evento de animación
                machine.CatchMessage("StartRun", () =>
                {
                    esperandoInicio = false;

                    UpdateAnimator(true, false); // activar run
                    MoverAlSiguienteWaypoint();  // empezar movimiento
                });
            }

            public override void OnStateExit()
            {
                machine.RotateAgentManually = false;

                if (agent != null && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                }

                UpdateAnimator(false, true);
            }

            public override void OnStateUpdate()
            {
                if (currentGroup == null || agent == null || recorridoFinalizado)
                    return;


                // Movimiento normal
                UpdateAnimator(true, false);

                if (!agent.pathPending && agent.remainingDistance <= asset.distanciaCambio)
                {
                    MoverAlSiguienteWaypoint();
                }
            }

            private void MoverAlSiguienteWaypoint()
            {
                if (currentGroup == null) return;

                AIWaypoint[] waypoints = currentGroup.GetComponentsInChildren<AIWaypoint>();
                if (waypoints.Length == 0) return;

                // 🛑 FIN DEL RECORRIDO (sin loop)
                if (currentWaypointIndex >= waypoints.Length)
                {
                    recorridoFinalizado = true;

                    agent.velocity = Vector3.zero;
                    agent.ResetPath();

                    UpdateAnimator(false, true); // idle
                    return;
                }

                AIWaypoint destino = waypoints[currentWaypointIndex];

                if (destino != null)
                {
                    agent.SetDestination(destino.transform.position);
                }

                currentWaypointIndex++;
            }

            private void UpdateAnimator(bool isRunning, bool isIdle)
            {
                if (animator == null || customGroup == null) return;

                if (!string.IsNullOrEmpty(customGroup.RunParameter))
                {
                    try { animator.SetBool(customGroup.RunParameter, isRunning); } catch { }
                }

                if (!string.IsNullOrEmpty(customGroup.IdleParameter))
                {
                    try { animator.SetBool(customGroup.IdleParameter, isIdle); } catch { }
                }

                if (agent != null)
                {
                    try { animator.SetFloat("Speed", agent.velocity.magnitude); } catch { }
                }
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[0];
            }
        }
    }
}