using UnityEngine;
using UHFPS.Scriptable;
using UnityEngine.AI;

namespace UHFPS.Runtime.States
{
    [CreateAssetMenu(fileName = "EstadoPatrullajeAI", menuName = "UHFPS/AI States/EstadoPatrullajeAI")]
    public class EstadoPatrullajeAI : AIStateAsset
    {
        [Header("Configuracion de Patrullaje")]
        public float velocidadPatrullaje = 1.5f;
        public float distanciaDeParada = 1f;
        public float tiempoEsperaEnPunto = 2f;

        [Header("Configuracion de Persecucion desde Patrulla")]
        [Tooltip("Si el jugador está escondido, no lo detecta. Aquí puedes ajustar un 'oído' para que te sienta si estás pegado a él aunque no te vea de frente.")]
        public float distanciaDeteccionCercana = 1.5f;

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            // Pasamos también el grupo (donde definimos los parámetros del Animador)
            return new EstadoPatrullajeAI_State(machine, this, group);
        }

        public override string StateKey => "PatrullajeAI";
        public override string Name => "Estado de Patrullaje Secuencial";

        public class EstadoPatrullajeAI_State : FSMAIState
        {
            private EstadoPatrullajeAI asset;
            private NavMeshAgent agent;
            private Animator animator;
            
            // Referencia a nuestro grupo global custom (para leer qué Strings usar en el Animator)
            private CustomNPCStateGroup customGroup;
            
            private AIWaypointsGroup currentGroup;
            private int currentWaypointIndex = 0;
            
            private bool isWaiting = false;
            private float waitTimer = 0f;

            public EstadoPatrullajeAI_State(NPCStateMachine machine, EstadoPatrullajeAI stateAsset, AIStatesGroup group) : base(machine) 
            { 
                this.asset = stateAsset;
                this.agent = machine.GetComponent<NavMeshAgent>();
                this.animator = machine.Animator;
                
                this.customGroup = group as CustomNPCStateGroup;
            }

            public override void OnStateEnter()
            {
                // Le delegamos el giro natural (Steering) a la maquina de UHFPS, no a Unity
                machine.RotateAgentManually = true;

                if (agent != null)
                {
                    agent.speed = asset.velocidadPatrullaje;
                    
                    currentGroup = FindClosestWaypointsGroup().Key;
                    
                    if (currentGroup != null)
                    {
                        currentWaypointIndex = 0;
                        MoverAlSiguienteWaypoint();
                    }
                    else
                    {
                        Debug.LogWarning("EstadoPatrullajeAI: No se ha encontrado ningún grupo de waypoints en la escena cercano al NPC.");
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

                // Restablecer animaciones al salir de la patrulla
                UpdateAnimator(false, false);
            }

            public override void OnStateUpdate()
            {
                if (currentGroup == null || agent == null) return;

                if (isWaiting)
                {
                    // Estamos quietos
                    UpdateAnimator(isWalking: false, isIdle: true);

                    waitTimer -= Time.deltaTime;
                    if (waitTimer <= 0f)
                    {
                        isWaiting = false;
                        MoverAlSiguienteWaypoint();
                    }
                }
                else
                {
                    // Estamos caminando hacia el punto
                    UpdateAnimator(isWalking: true, isIdle: false);

                    if (!agent.pathPending && agent.remainingDistance <= asset.distanciaDeParada)
                    {
                        isWaiting = true;
                        waitTimer = asset.tiempoEsperaEnPunto;
                        
                        // Freno absoluto: Borramos el camino que calculó el agente
                        agent.velocity = Vector3.zero;
                        agent.ResetPath(); 
                    }
                }
            }

            private void MoverAlSiguienteWaypoint()
            {
                if (currentGroup == null) return;

                AIWaypoint[] waypoints = currentGroup.GetComponentsInChildren<AIWaypoint>();
                if (waypoints.Length == 0) return;

                if (currentWaypointIndex >= waypoints.Length)
                {
                    currentWaypointIndex = 0;
                }

                AIWaypoint destino = waypoints[currentWaypointIndex];

                if (destino != null)
                {
                    agent.SetDestination(destino.transform.position);
                }

                currentWaypointIndex++;
            }

            // Metodo helper para disparar las animaciones con seguridad usando los strings dinámicos
            private void UpdateAnimator(bool isWalking, bool isIdle)
            {
                if (animator == null || customGroup == null) return;

                // Usamos los strings parametrizados en el Custom Group que armamos
                animator.SetBool(customGroup.WalkParameter, isWalking);
                animator.SetBool(customGroup.IdleParameter, isIdle);
                
                // Actualizamos el Multiplicador de velocidad de tu animación 
                if (agent != null)
                {
                    animator.SetFloat("Speed", agent.velocity.magnitude);
                }
            }

            public override Transition[] OnGetTransitions()
            {
                // Agregamos la lógica para saltar a persecución si nos detecta
                return new Transition[]
                {
                    Transition.To<EstadoPersecucionAI>(() =>
                        !IsPlayerDead && // No te persigue si ya te mató
                        !playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE) && // No te persigue si estás escondido
                        SeesPlayerOrClose(asset.distanciaDeteccionCercana)) // Te persigue si entras a su visión o estás tan cerca que te "oye"
                };
            }
        }
    }
}