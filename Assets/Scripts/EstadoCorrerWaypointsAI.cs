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
        public float distanciaDeParada = 1f;
        public float tiempoEsperaEnPunto = 0.5f;

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
            
            private bool isWaiting = false;
            private float waitTimer = 0f;

            public EstadoCorrerWaypointsAI_State(NPCStateMachine machine, EstadoCorrerWaypointsAI stateAsset, AIStatesGroup group) : base(machine) 
            { 
                this.asset = stateAsset;
                this.agent = machine.GetComponent<NavMeshAgent>();
                this.animator = machine.Animator;
                
                this.customGroup = group as CustomNPCStateGroup;
            }

            public override void OnStateEnter()
            {
                machine.RotateAgentManually = true;

                if (agent != null)
                {
                    agent.speed = asset.velocidadCarrera;
                    
                    currentGroup = FindClosestWaypointsGroup().Key;
                    
                    if (currentGroup != null)
                    {
                        currentWaypointIndex = 0;
                        MoverAlSiguienteWaypoint();
                    }
                    else
                    {
                        Debug.LogWarning("EstadoCorrerWaypointsAI: No se ha encontrado ningún grupo de waypoints en la escena cercano al NPC.");
                    }
                }
            }

            public override void OnStateExit()
            {
                machine.RotateAgentManually = false;

                if (agent != null && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                }
                isWaiting = false;

                UpdateAnimator(false, false);
            }

            public override void OnStateUpdate()
            {
                if (currentGroup == null || agent == null) return;

                if (isWaiting)
                {
                    UpdateAnimator(isRunning: false, isIdle: true);

                    waitTimer -= Time.deltaTime;
                    if (waitTimer <= 0f)
                    {
                        isWaiting = false;
                        MoverAlSiguienteWaypoint();
                    }
                }
                else
                {
                    UpdateAnimator(isRunning: true, isIdle: false);

                    if (!agent.pathPending && agent.remainingDistance <= asset.distanciaDeParada)
                    {
                        AIWaypoint[] waypoints = currentGroup.GetComponentsInChildren<AIWaypoint>();
                        
                        // Si ya llegamos al último waypoint, nos detenemos por completo
                        if (currentWaypointIndex >= waypoints.Length)
                        {
                            agent.velocity = Vector3.zero;
                            agent.ResetPath();
                            UpdateAnimator(isRunning: false, isIdle: true);
                            return; // Terminamos la ruta, nos quedamos en idle
                        }

                        if (asset.tiempoEsperaEnPunto > 0f)
                        {
                            isWaiting = true;
                            waitTimer = asset.tiempoEsperaEnPunto;
                            
                            // Freno absoluto solo si hay tiempo de espera
                            agent.velocity = Vector3.zero;
                            agent.ResetPath(); 
                        }
                        else
                        {
                            // Si el tiempo de espera es 0 o menor, pasamos al siguiente punto inmediatamente sin detener al agente
                            MoverAlSiguienteWaypoint();
                        }
                    }
                }
            }

            private void MoverAlSiguienteWaypoint()
            {
                if (currentGroup == null) return;

                AIWaypoint[] waypoints = currentGroup.GetComponentsInChildren<AIWaypoint>();
                if (waypoints.Length == 0) return;

                // Si ya llegamos al final de la ruta, no volvemos a empezar
                if (currentWaypointIndex >= waypoints.Length)
                {
                    return;
                }

                AIWaypoint destino = waypoints[currentWaypointIndex];

                if (destino != null)
                {
                    agent.SetDestination(destino.transform.position);
                    
                    // Si es el último waypoint de todos, activamos el autoBraking para que frene suave al llegar
                    // Si NO es el último, desactivamos el autoBraking para que pase de largo sin bajar la velocidad
                    if (currentWaypointIndex == waypoints.Length - 1)
                    {
                        agent.autoBraking = true;
                    }
                    else
                    {
                        agent.autoBraking = false;
                    }
                }

                currentWaypointIndex++;
            }

            private void UpdateAnimator(bool isRunning, bool isIdle)
            {
                if (animator == null || customGroup == null) return;

                // Solo intentar setear el parámetro si no está en blanco en el CustomGroup
                if (!string.IsNullOrEmpty(customGroup.RunParameter))
                {
                    // Envolvemos en try-catch genérico por si el parámetro no existe en el Animator
                    try { animator.SetBool(customGroup.RunParameter, isRunning); } catch {}
                }

                if (!string.IsNullOrEmpty(customGroup.IdleParameter))
                {
                    try { animator.SetBool(customGroup.IdleParameter, isIdle); } catch {}
                }
                
                if (agent != null)
                {
                    try { animator.SetFloat("Speed", agent.velocity.magnitude); } catch {}
                }
            }

            public override Transition[] OnGetTransitions()
            {
                // Este estado no tiene transiciones de persecución activas por sí mismo.
                // Puede ser cambiado por un evento o si llega al final de los waypoints (dependiendo de la configuración del State Machine).
                return new Transition[0];
            }
        }
    }
}
