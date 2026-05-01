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

        [Tooltip("Tiempo en segundos que se quedará en su lugar antes de empezar a moverse (Para dar tiempo a que tu animación inicial termine)")]
        public float tiempoDeArranque = 1.5f;
        
        [Tooltip("¿Borrar el NPC del mapa cuando llegue a su destino final?")]
        public bool destruirAlTerminar = true;

        [Tooltip("Ajusta este valor para sincronizar la animación (1 = velocidad real, 0.5 = mitad de velocidad, etc)")]
        public float multiplicadorVelocidadAnim = 1.0f;

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
            private float timerArranque;

            public EstadoCorrerWaypointsAI_State(NPCStateMachine machine, EstadoCorrerWaypointsAI stateAsset, AIStatesGroup group) : base(machine)
            {
                this.asset = stateAsset;
                this.agent = machine.GetComponent<NavMeshAgent>();
                this.animator = machine.Animator;
                this.customGroup = group as CustomNPCStateGroup;

                var assigner = machine.GetComponent<NPCWaypointAssigner>();
                if (assigner != null && assigner.grupoDeWaypoints != null)
                {
                    currentGroup = assigner.grupoDeWaypoints;
                }
            }

            public override void OnStateEnter()
            {
                machine.RotateAgentManually = false;

                timerArranque = asset.tiempoDeArranque;

                if (agent != null)
                {
                    agent.speed = asset.velocidadCarrera;
                    
                    // Si el timer es mayor a 0, congelamos mientras reproduce el grito/susto. Si es 0, corre.
                    agent.isStopped = (timerArranque > 0f); 

                    agent.autoBraking = false;
                    agent.stoppingDistance = 0f;
                    agent.acceleration = 120f;
                    agent.angularSpeed = 720f;

                    if (currentGroup == null)
                    {
                        var closest = FindClosestWaypointsGroup();
                        currentGroup = closest.Key;
                    }

                    if (currentGroup != null)
                    {
                        currentWaypointIndex = 0;
                        recorridoFinalizado = false;
                        MoverAlSiguienteWaypoint();
                    }
                }

                // Encendemos su Bool "Run" desde el Frame 1, que debería activar tu animación de Screamer -> Run
                UpdateAnimator(true, false);
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

                // Lógica del "Susto Inicial" antes de empezar a pisar
                if (timerArranque > 0f)
                {
                    timerArranque -= Time.deltaTime;
                    if (timerArranque <= 0f)
                    {
                        // Se terminó la animación previa, soltamos las riendas y permitimos caminar
                        agent.isStopped = false;
                    }
                    return; // Retornamos para que no procese el avance mientras grita/posa
                }

                // Asegurar que el bool de correr siga vivo
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

                if (currentWaypointIndex >= waypoints.Length)
                {
                    recorridoFinalizado = true;
                    agent.velocity = Vector3.zero;
                    agent.ResetPath();

                    UpdateAnimator(false, true);
                    
                    // ACCIÓN FINAL: Desaparecer
                    if (asset.destruirAlTerminar)
                    {
                        Destroy(machine.gameObject);
                    }
                    
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
                    animator.SetBool(customGroup.RunParameter, isRunning);

                if (!string.IsNullOrEmpty(customGroup.IdleParameter))
                    animator.SetBool(customGroup.IdleParameter, isIdle);

                if (agent != null)
                    animator.SetFloat("Speed", agent.velocity.magnitude * asset.multiplicadorVelocidadAnim);
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[0];
            }
        }
    }
}