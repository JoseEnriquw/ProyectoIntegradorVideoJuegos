using UnityEngine;
using UHFPS.Scriptable;
using UnityEngine.AI;

namespace UHFPS.Runtime.States
{
    [CreateAssetMenu(fileName = "EstadoEsperarTriggerAI", menuName = "UHFPS/AI States/EstadoEsperarTriggerAI")]
    public class EstadoEsperarTriggerAI : AIStateAsset
    {
        public override FSMAIState InitState(NPCStateMachine machine, AIStatesGroup group)
        {
            return new EstadoEsperarTriggerAI_State(machine, this, group);
        }

        public override string StateKey => "EsperarTriggerAI";
        public override string Name => "Estado: Esperar Inmóvil";

        public class EstadoEsperarTriggerAI_State : FSMAIState
        {
            private CustomNPCStateGroup customGroup;

            public EstadoEsperarTriggerAI_State(NPCStateMachine machine, AIStateAsset asset, AIStatesGroup group) : base(machine)
            {
                this.customGroup = group as CustomNPCStateGroup;
            }

            public override void OnStateEnter()
            {
                // Congelamos las físicas del NPC por completo para que no camine solo
                NavMeshAgent agent = machine.GetComponent<NavMeshAgent>();
                if (agent != null)
                {
                    agent.isStopped = true;
                    agent.velocity = Vector3.zero;
                }

                // Ponemos su animador en postura Idle
                if (machine.Animator != null && customGroup != null)
                {
                    machine.Animator.SetBool(customGroup.RunParameter, false);
                    machine.Animator.SetBool(customGroup.WalkParameter, false);
                    machine.Animator.SetBool(customGroup.IdleParameter, true);
                    machine.Animator.speed = 1f;
                }
            }

            public override void OnStateExit()
            {
                // Cuando el trigger nos ordene salir de aquí, aflojamos al personaje
                NavMeshAgent agent = machine.GetComponent<NavMeshAgent>();
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = false;
                }
            }

            public override void OnStateUpdate()
            {
                // Este estado literalmente no piensa ni hace nada. Se queda mirando la pared eternamente.
            }

            public override Transition[] OnGetTransitions()
            {
                // No transiciona por voluntad propia a NINGÚN lado. 
                // Solo saldrá de aquí cuando "TriggerDeEstadoNPC" lo jale a la fuerza.
                return new Transition[0];
            }
        }
    }
}
