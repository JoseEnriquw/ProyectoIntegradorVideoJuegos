using UnityEngine;
using UHFPS.Scriptable;
using UnityEngine.AI;

namespace UHFPS.Runtime.States
{
    [CreateAssetMenu(fileName = "EstadoCorrerWaypointsAI", menuName = "UHFPS/AI States/EstadoCorrerWaypointsAI")]
    public class EstadoCorrerWaypointsAI : AIStateAsset
    {
        public float velocidadCarrera = 3.5f;
        public float distanciaCambio = 0.5f;

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

            // 🔥 NUEVO
            private bool esperandoStartRun = true;

            public EstadoCorrerWaypointsAI_State(NPCStateMachine machine, EstadoCorrerWaypointsAI stateAsset, AIStatesGroup group) : base(machine)
            {
                this.asset = stateAsset;
                this.agent = machine.GetComponent<NavMeshAgent>();
                this.animator = machine.Animator;
                this.customGroup = group as CustomNPCStateGroup;
            }

            public override void OnStateEnter()
            {
                // Dejamos que el NavMeshAgent controle la rotación de forma nativa
                machine.RotateAgentManually = false;

                esperandoStartRun = true;

                if (agent != null)
                {
                    agent.speed = asset.velocidadCarrera;
                    agent.isStopped = true; // ⛔ NO SE MUEVE TODAVÍA

                    agent.autoBraking = false;
                    agent.stoppingDistance = 0f;
                    agent.acceleration = 100f;
                    agent.angularSpeed = 720f;

                    var closest = FindClosestWaypointsGroup();
                    currentGroup = closest.Key;

                    if (currentGroup != null)
                    {
                        Debug.Log("Grupo de waypoints encontrado: " + currentGroup.gameObject.name);
                        currentWaypointIndex = 0;
                        recorridoFinalizado = false;
                        
                        // ✅ Le asignamos el destino DESDE EL INICIO para que sepa hacia dónde tiene que orientarse
                        MoverAlSiguienteWaypoint();
                    }
                    else
                    {
                        Debug.LogWarning("No se encontró AI Waypoints Group cercano.");
                    }
                }

                // ✅ Le decimos al Animator que empiece el flujo (Idle -> Turn -> Run)
                UpdateAnimator(true, false);

                // ✅ ESCUCHAR EVENTO DE ANIMACIÓN (Opcional)
                machine.CatchMessage("StartRun", () =>
                {
                    ActivarMovimiento();
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

                // ⛔ NO HACER NADA FÍSICAMENTE HASTA QUE TERMINE DE GIRAR
                if (esperandoStartRun)
                {
                    // ALTERNATIVA SEGURA: Si el evento falla, revisamos directamente si el Animator ya llegó al estado "run"
                    // (En tu captura el estado se llama "run" en minúsculas).
                    if (IsAnimation(0, "run") || IsAnimation(0, "Run"))
                    {
                        ActivarMovimiento();
                    }
                    return;
                }

                UpdateAnimator(true, false);

                if (!agent.pathPending && agent.remainingDistance <= asset.distanciaCambio)
                {
                    MoverAlSiguienteWaypoint();
                }
            }

            private void ActivarMovimiento()
            {
                if (!esperandoStartRun) return; // Si ya se activó, no hacer nada
                
                esperandoStartRun = false;

                if (agent != null)
                    agent.isStopped = false;

                // NOTA: Ya no llamamos a MoverAlSiguienteWaypoint() aquí porque lo llamamos en OnStateEnter 
                // para que el NavMeshAgent calcule el steeringTarget desde el principio y se oriente bien.
            }

            private void MoverAlSiguienteWaypoint()
            {
                if (currentGroup == null) return;

                AIWaypoint[] waypoints = currentGroup.GetComponentsInChildren<AIWaypoint>();
                if (waypoints.Length == 0) return;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    Debug.Log("Recorrido finalizado.");
                    recorridoFinalizado = true;

                    agent.velocity = Vector3.zero;
                    agent.ResetPath();

                    UpdateAnimator(false, true);
                    return;
                }

                AIWaypoint destino = waypoints[currentWaypointIndex];

                if (destino != null)
                {
                    Debug.Log("Moviendo al waypoint índice: " + currentWaypointIndex);
                    agent.SetDestination(destino.transform.position);
                }

                currentWaypointIndex++;
            }

            private void UpdateAnimator(bool isRunning, bool isIdle)
            {
                if (animator == null || customGroup == null) return;

                if (!string.IsNullOrEmpty(customGroup.RunParameter))
                    animator.SetBool(customGroup.RunParameter, isRunning);

                if (!string.IsNullOrEmpty(customGroup.IdleParameter))
                    animator.SetBool(customGroup.IdleParameter, isIdle);

                if (agent != null)
                    animator.SetFloat("Speed", agent.velocity.magnitude);
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[0];
            }
        }
    }
}