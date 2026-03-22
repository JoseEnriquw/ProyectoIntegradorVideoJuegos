using UnityEngine;

namespace FuncionalidadesCore.FirstPerson
{
    public class FPIdleState : FSMStateBase
    {
        private FirstPersonController player;

        public FPIdleState(FirstPersonController player) 
        { 
            this.player = player; 
        }

        public override void OnStateEnter()
        {
            player.CurrentSpeed = 0f;
        }

        public override void OnStateUpdate()
        {
            // Frenado suave
            player.Velocity.x = Mathf.Lerp(player.Velocity.x, 0, Time.deltaTime * player.SpeedSmoothing);
            player.Velocity.z = Mathf.Lerp(player.Velocity.z, 0, Time.deltaTime * player.SpeedSmoothing);
        }
    }

    public class FPWalkState : FSMStateBase
    {
        private FirstPersonController player;

        public FPWalkState(FirstPersonController player) 
        { 
            this.player = player; 
        }

        public override void OnStateUpdate()
        {
            player.CurrentSpeed = Mathf.Lerp(player.CurrentSpeed, player.WalkSpeed, Time.deltaTime * player.SpeedSmoothing);
            
            Vector3 direction = player.GetMovementDirection();
            player.Velocity.x = direction.x * player.CurrentSpeed;
            player.Velocity.z = direction.z * player.CurrentSpeed;
        }
    }

    public class FPRunState : FSMStateBase
    {
        private FirstPersonController player;

        public FPRunState(FirstPersonController player) 
        { 
            this.player = player; 
        }

        public override void OnStateUpdate()
        {
            player.CurrentSpeed = Mathf.Lerp(player.CurrentSpeed, player.RunSpeed, Time.deltaTime * player.SpeedSmoothing);
            
            Vector3 direction = player.GetMovementDirection();
            player.Velocity.x = direction.x * player.CurrentSpeed;
            player.Velocity.z = direction.z * player.CurrentSpeed;
        }
    }

    public class FPJumpState : FSMStateBase
    {
        private FirstPersonController player;

        public FPJumpState(FirstPersonController player) 
        { 
            this.player = player; 
        }

        public override void OnStateEnter()
        {
            // Aplica fuerza instantánea al eje Y
            player.Velocity.y = player.JumpForce;
        }

        public override void OnStateUpdate()
        {
            // Movimiento en el aire conservando la inercia actual
            Vector3 direction = player.GetMovementDirection();
            player.Velocity.x = direction.x * player.CurrentSpeed;
            player.Velocity.z = direction.z * player.CurrentSpeed;
        }
    }
}
