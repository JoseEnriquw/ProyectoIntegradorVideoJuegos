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

            public EstadoVigilanciaEstaticaAI_State(NPCStateMachine machine, EstadoVigilanciaEstaticaAI stateAsset, AIStatesGroup group) : base(machine)
            {
                this.asset = stateAsset;
                this.customGroup = group as CustomNPCStateGroup;
            }

            public override Transition[] OnGetTransitions()
            {
                return new Transition[]
                {
                    Transition.To<EstadoDelatarAI>(() =>
                        !IsPlayerDead &&
                        !playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE) &&
                        SeesPlayerOrClose(asset.distanciaDeteccionCercana))
                };
            }

            public override void OnStateEnter()
            {
                // Permitir rotación manual desactivando la de NavMesh
                machine.RotateAgentManually = true;

                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                    agent.ResetPath();
                }

                UpdateAnimator(isIdle: true);
            }

            public override void OnStateExit()
            {
                machine.RotateAgentManually = false;
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }
            }

            public override void OnStateUpdate()
            {
                UpdateAnimator(isIdle: true);

                if (IsPlayerDead) return;

                // Seguir al jugador con la mirada si está en rango de visión y no está escondido
                if (!playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE) && SeesObject(machine.SightsDistance, PlayerHead))
                {
                    Vector3 playerDir = PlayerPosition - machine.transform.position;
                    playerDir.y = 0f;
                    if (playerDir.sqrMagnitude > 0.1f)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(playerDir);
                        machine.transform.rotation = Quaternion.Slerp(machine.transform.rotation, targetRot, Time.deltaTime * 5f);
                    }
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
                    animator.SetFloat("Speed", 0f);
                }
            }
        }
    }
}
