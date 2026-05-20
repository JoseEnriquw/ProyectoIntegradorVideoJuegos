using UnityEngine;
using UHFPS.Scriptable;
using UnityEngine.AI;
using UHFPS.Runtime;

namespace UHFPS.Runtime.States
{
    [CreateAssetMenu(fileName = "EstadoIrYAnimarAI", menuName = "UHFPS/AI States/EstadoIrYAnimarAI")]
    public class EstadoIrYAnimarAI : AIStateAsset
    {
        [Header("Configuracion del Destino")]
        [Tooltip("El nombre exacto del GameObject en la escena al que debe ir el NPC. Si se deja vacio, hara la animacion en su lugar actual.")]
        public string nombreDelPuntoDestino = "PuntoPickup";

        [Header("Configuracion de Movimiento")]
        public float velocidadMovimiento = 1.5f;
        public float distanciaDeParada = 0.5f;

        [Header("Configuracion de Animacion")]
        [Tooltip("El nombre del parametro bool en el Animator que activa la animacion (Ej: pickup)")]
        public string parametroAnimacion = "pickup";
        [Tooltip("Tiempo en segundos que debe esperar mientras hace la animacion.")]
        public float tiempoDeAnimacion = 3f;

        [Header("Al finalizar")]
        [Tooltip("Key del estado al que pasará cuando termine la animacion (ej: PatrullajeAI)")]
        public string estadoPostAnimacion = "PatrullajeAI";

        [Header("Interacción con Entorno (Opcional)")]
        [Tooltip("Nombres exactos de los objetos que quieres que desaparezcan cuando termine la animación (ej: los melones).")]
        public string[] nombresObjetosADesaparecer;

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new EstadoIrYAnimarAI_State(machine, this, group);
        }

        public override string StateKey => "IrYAnimarAI";
        public override string Name => "Estado: Ir a Punto y Animar";

        public class EstadoIrYAnimarAI_State : FSMAIState
        {
            private EstadoIrYAnimarAI asset;
            private NavMeshAgent agent;
            private Animator animator;
            private CustomNPCStateGroup customGroup;
            
            private Transform puntoDestino;
            private bool llegoAlDestino = false;
            private bool animacionTerminada = false;
            private float timerAnimacion = 0f;

            public EstadoIrYAnimarAI_State(NPCStateMachine machine, EstadoIrYAnimarAI stateAsset, AIStatesGroup group) : base(machine)
            {
                this.asset = stateAsset;
                this.agent = machine.GetComponent<NavMeshAgent>();
                this.animator = machine.Animator;
                this.customGroup = group as CustomNPCStateGroup;
            }

            public override void OnStateEnter()
            {
                llegoAlDestino = false;
                animacionTerminada = false;
                timerAnimacion = 0f;

                machine.RotateAgentManually = true;

                if (agent != null)
                {
                    // Liberamos el agente si estaba detenido
                    agent.isStopped = false;
                    agent.speed = asset.velocidadMovimiento;
                }

                // Buscar el punto de destino
                if (!string.IsNullOrEmpty(asset.nombreDelPuntoDestino))
                {
                    GameObject destinoObj = GameObject.Find(asset.nombreDelPuntoDestino);
                    if (destinoObj != null)
                    {
                        puntoDestino = destinoObj.transform;
                        if (agent != null && agent.isOnNavMesh)
                        {
                            agent.SetDestination(puntoDestino.position);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[EstadoIrYAnimarAI] No se encontro un objeto con el nombre '{asset.nombreDelPuntoDestino}'. Se animara en el lugar.");
                        llegoAlDestino = true; // Forzamos animacion si no hay punto
                    }
                }
                else
                {
                    llegoAlDestino = true;
                }
            }

            public override void OnStateExit()
            {
                machine.RotateAgentManually = false;
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.isStopped = false;
                }

                // Asegurar que el bool de la animacion se apague al salir
                if (animator != null && !string.IsNullOrEmpty(asset.parametroAnimacion))
                {
                    animator.SetBool(asset.parametroAnimacion, false);
                    UpdateMovementAnim(false, false);
                }
            }

            public override void OnStateUpdate()
            {
                if (agent == null) return;

                if (!llegoAlDestino)
                {
                    // Todavia en camino
                    UpdateMovementAnim(true, false);

                    if (!agent.pathPending && agent.remainingDistance <= asset.distanciaDeParada)
                    {
                        // Llego al punto
                        llegoAlDestino = true;
                        
                        // Freno total del agente para que no siga caminando y no se hunda
                        agent.velocity = Vector3.zero;
                        agent.isStopped = true;
                        agent.ResetPath();

                        // Desactivamos rotacion manual de UHFPS para que no rote solo
                        machine.RotateAgentManually = false;

                        // Rotar al NPC para que mire igual que el punto de destino
                        if (puntoDestino != null)
                        {
                            machine.transform.rotation = puntoDestino.rotation;
                        }



                        // Activar animacion
                        if (animator != null && !string.IsNullOrEmpty(asset.parametroAnimacion))
                        {
                            UpdateMovementAnim(false, true); // Apagar caminar
                            animator.SetBool(asset.parametroAnimacion, true);
                        }
                    }
                }
                else if (!animacionTerminada)
                {
                    // Forzar que el NavMeshAgent no se mueva durante la animacion
                    if (agent.isOnNavMesh) {
                        agent.velocity = Vector3.zero;
                        agent.isStopped = true;
                    }

                    // Forzar rotacion al punto destino mientras hace la animacion
                    if (puntoDestino != null)
                    {
                        machine.transform.rotation = puntoDestino.rotation;
                    }

                    // Esperando que termine el tiempo de animacion
                    timerAnimacion += Time.deltaTime;
                    if (timerAnimacion >= asset.tiempoDeAnimacion)
                    {
                        animacionTerminada = true;
                        
                        // Apagar animacion
                        if (animator != null && !string.IsNullOrEmpty(asset.parametroAnimacion))
                        {
                            animator.SetBool(asset.parametroAnimacion, false);
                        }

                        // Hacer desaparecer los objetos configurados
                        if (asset.nombresObjetosADesaparecer != null && asset.nombresObjetosADesaparecer.Length > 0)
                        {
                            foreach (string nombreObj in asset.nombresObjetosADesaparecer)
                            {
                                if (!string.IsNullOrEmpty(nombreObj))
                                {
                                    GameObject obj = GameObject.Find(nombreObj);
                                    if (obj != null) obj.SetActive(false);
                                }
                            }
                        }

                        // Cambiar de estado si se configuro
                        if (!string.IsNullOrEmpty(asset.estadoPostAnimacion))
                        {
                            machine.ChangeState(asset.estadoPostAnimacion);
                        }
                    }
                }
            }

            private void UpdateMovementAnim(bool isWalking, bool isIdle)
            {
                if (animator == null || customGroup == null) return;

                if (!string.IsNullOrEmpty(customGroup.WalkParameter)) animator.SetBool(customGroup.WalkParameter, isWalking);
                if (!string.IsNullOrEmpty(customGroup.IdleParameter)) animator.SetBool(customGroup.IdleParameter, isIdle);
                
                if (agent != null)
                {
                    animator.SetFloat("Speed", isWalking ? agent.velocity.magnitude : 0f);
                }
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[0];
            }
        }
    }
}
