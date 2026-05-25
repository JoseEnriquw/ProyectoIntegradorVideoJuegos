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

        [Header("Camera Look At Settings")]
        [Tooltip("If enabled, the NPC will rotate relative to the player's camera position.")]
        public bool LookAtPlayerCamera = true;

        [Tooltip("If true, only horizontal rotation (Y axis) is applied, avoiding tilting up/down.")]
        public bool IgnoreYAxis = true;

        [Tooltip("Speed of the rotation. Set to 0 for instant rotation.")]
        public float RotationSpeed = 5f;

        [Tooltip("Rotation offset in degrees. Default is 180 to keep the NPC facing away (back to the camera).")]
        public float RotationOffset = 180f;

        public class EstadoEsperarTriggerAI_State : FSMAIState
        {
            private CustomNPCStateGroup customGroup;
            private EstadoEsperarTriggerAI customAsset;

            public EstadoEsperarTriggerAI_State(NPCStateMachine machine, EstadoEsperarTriggerAI asset, AIStatesGroup group) : base(machine)
            {
                this.customAsset = asset;
                this.customGroup = group as CustomNPCStateGroup;
            }

            public override void OnStateEnter()
            {
                // Congelamos las físicas del NPC por completo para que no camine solo
                NavMeshAgent agent = machine.GetComponent<NavMeshAgent>();
                if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
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
                // Este estado originalmente no hacía nada. Ahora puede rotar hacia la cámara del jugador (o darle la espalda).
                if (customAsset.LookAtPlayerCamera)
                {
                    Camera playerCam = playerManager != null && playerManager.MainCamera != null ? playerManager.MainCamera : Camera.main;
                    if (playerCam != null)
                    {
                        Transform npcTransform = machine.transform;
                        Vector3 direction = playerCam.transform.position - npcTransform.position;
                        
                        if (customAsset.IgnoreYAxis)
                        {
                            direction.y = 0f;
                        }
                        
                        if (direction.sqrMagnitude > 0.001f)
                        {
                            Quaternion targetRotation = Quaternion.LookRotation(direction);
                            
                            if (Mathf.Abs(customAsset.RotationOffset) > 0.001f)
                            {
                                targetRotation *= Quaternion.Euler(0, customAsset.RotationOffset, 0);
                            }
                            
                            if (customAsset.RotationSpeed > 0f)
                            {
                                npcTransform.rotation = Quaternion.Slerp(npcTransform.rotation, targetRotation, Time.deltaTime * customAsset.RotationSpeed);
                            }
                            else
                            {
                                npcTransform.rotation = targetRotation;
                            }
                        }
                    }
                }
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
