using UnityEngine;
using UHFPS.Scriptable;
using UnityEngine.AI;

namespace UHFPS.Runtime.States
{
    [CreateAssetMenu(fileName = "EstadoVolarWaypointsAI", menuName = "UHFPS/AI States/EstadoVolarWaypointsAI")]
    public class EstadoVolarWaypointsAI : AIStateAsset
    {
        public float velocidadVuelo = 5.0f;
        public float distanciaCambio = 0.5f;

        [Tooltip("Tiempo en segundos que se quedará en su lugar antes de empezar a volar")]
        public float tiempoDeArranque = 0.5f;
        


        [Tooltip("¿Borrar el NPC del mapa cuando llegue a su destino final?")]
        public bool destruirAlTerminar = true;

        [Tooltip("Ajusta este valor para sincronizar la animación (1 = velocidad real, 0.5 = mitad de velocidad, etc)")]
        public float multiplicadorVelocidadAnim = 1.0f;

        [Tooltip("Nombre del Trigger en el Animator para iniciar la animación de vuelo (ej. Start_fly)")]
        public string triggerDeVuelo = "Start_fly";

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new EstadoVolarWaypointsAI_State(machine, this, group);
        }

        public override string StateKey => "VolarWaypointsAI";
        public override string Name => "Estado de Vuelo Hacia Waypoints";

        public class EstadoVolarWaypointsAI_State : FSMAIState
        {
            private EstadoVolarWaypointsAI asset;
            private NavMeshAgent agent;
            private Animator animator;
            private CustomNPCStateGroup customGroup;

            private AIWaypointsGroup currentGroup;
            private int currentWaypointIndex = 0;
            private AIWaypoint[] waypoints;

            private bool recorridoFinalizado = false;
            private float timerArranque;

            private Collider npcCollider;
            private bool wasTrigger;
            private Rigidbody npcRb;
            private bool wasKinematic;

            public EstadoVolarWaypointsAI_State(NPCStateMachine machine, EstadoVolarWaypointsAI stateAsset, AIStatesGroup group) : base(machine)
            {
                this.asset = stateAsset;
                this.agent = machine.GetComponent<NavMeshAgent>();
                this.animator = machine.Animator;
                this.customGroup = group as CustomNPCStateGroup;
            }

            public override void OnStateEnter()
            {
                machine.RotateAgentManually = true; // El script se encargará de rotarlo manualmente hacia el cielo/waypoint

                timerArranque = asset.tiempoDeArranque;

                if (agent != null)
                {
                    // ¡Apagamos el NavMeshAgent! Para volar no necesitamos el piso verde.
                    agent.enabled = false;
                }

                npcCollider = machine.GetComponent<Collider>();
                if (npcCollider != null)
                {
                    wasTrigger = npcCollider.isTrigger;
                    npcCollider.isTrigger = true; // Fantasma para no chocar con árboles
                }

                npcRb = machine.GetComponent<Rigidbody>();
                if (npcRb != null)
                {
                    wasKinematic = npcRb.isKinematic;
                    npcRb.isKinematic = true; // Sin gravedad ni físicas durante el vuelo
                }

                NPCWaypointAssigner assigner = machine.GetComponent<NPCWaypointAssigner>();
                if (assigner != null && assigner.grupoDeWaypoints != null)
                {
                    currentGroup = assigner.grupoDeWaypoints;
                }
                else
                {
                    var closest = FindClosestWaypointsGroup();
                    if (closest.Key != null)
                        currentGroup = closest.Key;
                }

                if (currentGroup != null)
                {
                    currentWaypointIndex = 0;
                    recorridoFinalizado = false;
                    waypoints = currentGroup.GetComponentsInChildren<AIWaypoint>();
                }

                // Disparamos la animación de vuelo
                if (!string.IsNullOrEmpty(asset.triggerDeVuelo) && animator != null)
                {
                    animator.SetTrigger(asset.triggerDeVuelo);
                }
                UpdateAnimator(true, false);
            }

            public override void OnStateExit()
            {
                machine.RotateAgentManually = false;

                // Devolver el agente y físicas a la normalidad si no se destruyó
                if (agent != null)
                {
                    agent.enabled = true;
                    if (agent.isOnNavMesh) agent.ResetPath();
                }

                if (npcCollider != null) npcCollider.isTrigger = wasTrigger;
                if (npcRb != null) npcRb.isKinematic = wasKinematic;

                UpdateAnimator(false, true);
            }

            public override void OnStateUpdate()
            {
                if (currentGroup == null || waypoints == null || recorridoFinalizado)
                    return;

                if (timerArranque > 0f)
                {
                    timerArranque -= Time.deltaTime;
                    return; 
                }

                UpdateAnimator(true, false);

                if (currentWaypointIndex >= waypoints.Length)
                {
                    recorridoFinalizado = true;
                    UpdateAnimator(false, true);
                    
                    if (asset.destruirAlTerminar)
                    {
                        Destroy(machine.gameObject);
                    }
                    return;
                }

                AIWaypoint destino = waypoints[currentWaypointIndex];
                if (destino == null) return;

                Vector3 targetPosition = destino.transform.position;
                
                // Mover libremente en el aire hacia la posición exacta (X, Y, Z) del waypoint
                machine.transform.position = Vector3.MoveTowards(machine.transform.position, targetPosition, asset.velocidadVuelo * Time.deltaTime);

                // Rotar para mirar hacia el waypoint
                Vector3 direction = (targetPosition - machine.transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    machine.transform.rotation = Quaternion.Slerp(machine.transform.rotation, targetRotation, Time.deltaTime * 5f);
                }

                // Si la distancia real (en 3D) es muy corta, pasamos al siguiente punto
                if (Vector3.Distance(machine.transform.position, targetPosition) <= asset.distanciaCambio)
                {
                    currentWaypointIndex++;
                }
            }

            private void UpdateAnimator(bool isRunning, bool isIdle)
            {
                if (animator == null || customGroup == null) return;

                if (!string.IsNullOrEmpty(customGroup.RunParameter))
                    animator.SetBool(customGroup.RunParameter, isRunning);

                if (!string.IsNullOrEmpty(customGroup.IdleParameter))
                    animator.SetBool(customGroup.IdleParameter, isIdle);

                animator.SetFloat("Speed", asset.velocidadVuelo * asset.multiplicadorVelocidadAnim);
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[0];
            }
        }
    }
}
