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

        [Tooltip("Si el jugador está escondido, no lo detecta. Aquí puedes ajustar un 'oído' para que te sienta si estás pegado a él aunque no te vea de frente.")]
        public float distanciaDeteccionCercana = 1.5f;

        [Tooltip("Ajusta este valor para sincronizar la animación (1 = velocidad real, 0.5 = mitad de velocidad, etc)")]
        public float multiplicadorVelocidadAnim = 1.0f;

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
            // Rotación congelada: la guardamos cuando el NPC llega al waypoint
            // y la aplicamos cada frame para que no se gire mientras espera.
            private Quaternion rotacionCongelada;

            public EstadoPatrullajeAI_State(NPCStateMachine machine, EstadoPatrullajeAI stateAsset, AIStatesGroup group) : base(machine) 
            { 
                this.asset = stateAsset;
                this.agent = machine.GetComponent<NavMeshAgent>();
                this.animator = machine.Animator;
                this.customGroup = group as CustomNPCStateGroup;
                // El grupo de waypoints se resuelve en OnStateEnter para garantizar
                // que todas las referencias de Unity estén inicializadas.
            }

            public override void OnStateEnter()
            {
                // Le delegamos el giro natural (Steering) a la maquina de UHFPS, no a Unity
                machine.RotateAgentManually = true;

                if (agent != null)
                {
                    agent.speed = asset.velocidadPatrullaje;

                    // Siempre re-leemos el assigner en OnStateEnter para evitar problemas
                    // de orden de inicialización de Unity (el constructor corre antes de Awake/Start).
                    var assigner = machine.GetComponent<NPCWaypointAssigner>();
                    if (assigner != null && assigner.grupoDeWaypoints != null)
                    {
                        currentGroup = assigner.grupoDeWaypoints;
                    }

                    if (currentGroup == null)
                    {
                        currentGroup = FindClosestWaypointsGroup().Key;
                        if (currentGroup == null)
                        {
                            Debug.LogWarning($"[{machine.name}] EstadoPatrullajeAI: No se encontró un grupo de waypoints. " +
                                "Asegurate de agregar el componente NPCWaypointAssigner al NPC y arrastrar su ruta.");
                        }
                    }
                    
                    if (currentGroup != null)
                    {
                        currentWaypointIndex = 0;
                        MoverAlSiguienteWaypoint();
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
                    // Estamos quietos — bloqueamos la rotación al valor guardado
                    machine.transform.rotation = rotacionCongelada;
                    UpdateAnimator(isWalking: false, isIdle: true);

                    waitTimer -= Time.deltaTime;
                    if (waitTimer <= 0f)
                    {
                        isWaiting = false;
                        // Devolvemos el control de rotación a UHFPS al reanudar la marcha
                        machine.RotateAgentManually = true;
                        MoverAlSiguienteWaypoint();
                    }
                }
                else
                {
                    // Estamos caminando hacia el punto
                    UpdateAnimator(isWalking: true, isIdle: false);

                    if (!agent.pathPending && agent.remainingDistance <= asset.distanciaDeParada)
                    {
                        // Capturamos la rotación del NPC exactamente al llegar
                        rotacionCongelada = machine.transform.rotation;

                        isWaiting = true;
                        waitTimer = asset.tiempoEsperaEnPunto;
                        
                        // Freno absoluto: borramos el camino y la velocidad
                        agent.velocity = Vector3.zero;
                        agent.ResetPath();

                        // Desactivamos el steering de UHFPS para que no nos gire mientras esperamos
                        machine.RotateAgentManually = false;
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
                    animator.SetFloat("Speed", agent.velocity.magnitude * asset.multiplicadorVelocidadAnim);
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