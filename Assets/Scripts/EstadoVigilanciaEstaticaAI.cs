using UnityEngine;
using UHFPS.Scriptable;
using UnityEngine.AI;

namespace UHFPS.Runtime.States
{
    [CreateAssetMenu(fileName = "EstadoVigilanciaEstaticaAI", menuName = "UHFPS/AI States/EstadoVigilanciaEstaticaAI")]
    public class EstadoVigilanciaEstaticaAI : AIStateAsset
    {
        [Header("Configuracion de Vigilancia Estatica")]
        [Tooltip("Si el jugador está escondido, no lo detecta. Aquí puedes ajustar un 'oído' para que te sienta si estás pegado a él aunque no te vea de frente.")]
        public float distanciaDeteccionCercana = 1.5f;

        [Tooltip("Velocidad a la que el NPC regresa a su punto de origen después de perder de vista al jugador.")]
        public float velocidadRetorno = 1.5f;

        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new EstadoVigilanciaEstaticaAI_State(machine, this, group);
        }

        public override string StateKey => "VigilanciaEstaticaAI";
        public override string Name => "Estado de Vigilancia Estatica (Sentry)";

        public class EstadoVigilanciaEstaticaAI_State : FSMAIState
        {
            private EstadoVigilanciaEstaticaAI asset;
            private CustomNPCStateGroup customGroup;

            private Vector3 originPosition;
            private Quaternion originRotation;
            private bool isReturning;

            public EstadoVigilanciaEstaticaAI_State(NPCStateMachine machine, EstadoVigilanciaEstaticaAI stateAsset, AIStatesGroup group) : base(machine)
            {
                this.asset = stateAsset;
                this.customGroup = group as CustomNPCStateGroup;

                // Salvar posición y rotación iniciales
                this.originPosition = machine.transform.position;
                this.originRotation = machine.transform.rotation;

                // Si tiene waypoints asignados, usar el primero como su punto de origen
                var assigner = machine.GetComponent<NPCWaypointAssigner>();
                if (assigner != null && assigner.grupoDeWaypoints != null)
                {
                    AIWaypoint[] waypoints = assigner.grupoDeWaypoints.GetComponentsInChildren<AIWaypoint>();
                    if (waypoints.Length > 0 && waypoints[0] != null)
                    {
                        this.originPosition = waypoints[0].transform.position;
                        this.originRotation = waypoints[0].transform.rotation;
                    }
                }
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<EstadoPersecucionAI>(() =>
                        !IsPlayerDead &&
                        !playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE) &&
                        SeesPlayerOrClose(asset.distanciaDeteccionCercana) &&
                        TieneEstado<EstadoPersecucionAI>()),

                    Transition.To<EstadoDelatarAI>(() =>
                        !IsPlayerDead &&
                        !playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE) &&
                        SeesPlayerOrClose(asset.distanciaDeteccionCercana) &&
                        !TieneEstado<EstadoPersecucionAI>() && TieneEstado<EstadoDelatarAI>())
                };
            }

            private bool TieneEstado<T>() where T : AIStateAsset
            {
                if (machine == null || machine.StatesAssetRuntime == null) return false;
                foreach (var stateData in machine.StatesAssetRuntime.AIStates)
                {
                    if (stateData.StateAsset != null && stateData.StateAsset is T && stateData.IsEnabled)
                    {
                        return true;
                    }
                }
                return false;
            }

            public override void OnStateEnter()
            {
                // Permitir rotación manual desactivando la de NavMesh
                machine.RotateAgentManually = true;

                if (agent != null && agent.isOnNavMesh)
                {
                    float dist = Vector3.Distance(machine.transform.position, originPosition);
                    if (dist > 0.5f)
                    {
                        isReturning = true;
                        agent.isStopped = false;
                        agent.speed = asset.velocidadRetorno;
                        agent.stoppingDistance = 0.1f;
                        agent.SetDestination(originPosition);
                        UpdateAnimator(isIdle: false);
                    }
                    else
                    {
                        isReturning = false;
                        agent.isStopped = true;
                        agent.velocity = Vector3.zero;
                        agent.ResetPath();
                        UpdateAnimator(isIdle: true);
                    }
                }
                else
                {
                    isReturning = false;
                    UpdateAnimator(isIdle: true);
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
                isReturning = false;
            }

            public override void OnStateUpdate()
            {
                if (IsPlayerDead) return;

                if (agent != null && agent.isOnNavMesh)
                {
                    if (isReturning)
                    {
                        UpdateAnimator(isIdle: false);

                        if (!agent.pathPending && agent.remainingDistance <= 0.2f)
                        {
                            isReturning = false;
                            agent.isStopped = true;
                            agent.velocity = Vector3.zero;
                            agent.ResetPath();
                        }
                    }
                    else
                    {
                        UpdateAnimator(isIdle: true);

                        // Seguir al jugador con la mirada si está en rango de visión y no está escondido
                        if (!playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE) && SeesPlayer())
                        {
                            Vector3 playerDir = PlayerPosition - machine.transform.position;
                            playerDir.y = 0f;
                            if (playerDir.sqrMagnitude > 0.1f)
                            {
                                Quaternion targetRot = Quaternion.LookRotation(playerDir);
                                machine.transform.rotation = Quaternion.Slerp(machine.transform.rotation, targetRot, Time.deltaTime * 5f);
                            }
                        }
                        else
                        {
                            // Rotar de vuelta a la orientación original
                            machine.transform.rotation = Quaternion.Slerp(machine.transform.rotation, originRotation, Time.deltaTime * 5f);
                        }
                    }
                }
                else
                {
                    UpdateAnimator(isIdle: true);
                }
            }

            private void UpdateAnimator(bool isIdle)
            {
                if (animator == null || customGroup == null) return;

                animator.SetBool(customGroup.IdleParameter, isIdle);
                animator.SetBool(customGroup.WalkParameter, !isIdle);
                animator.SetBool(customGroup.RunParameter, false);

                if (agent != null)
                {
                    animator.SetFloat("Speed", isIdle ? 0f : agent.velocity.magnitude);
                }
            }
        }
    }
}
