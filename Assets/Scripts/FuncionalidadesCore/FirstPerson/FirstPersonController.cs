using UnityEngine;
using System.Collections.Generic;

namespace FuncionalidadesCore.FirstPerson
{
    /// <summary>
    /// Controlador de Primera Persona construido sobre la StateMachineCore genérica.
    /// Maneja físicas básicas, gravedad y expone propiedades para los estados de movimiento.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : StateMachineCore, IMovementProvider
    {
        [Header("Movement Settings")]
        public float WalkSpeed = 4f;
        public float RunSpeed = 7f;
        public float CrouchSpeed = 2f;
        public float JumpForce = 5f;
        public float Gravity = -15f;
        public float SpeedSmoothing = 10f;

        [Header("References")]
        public Transform CameraRoot;

        // --- Props Locales ---
        public CharacterController Controller { get; private set; }
        private IInputProvider input;
        
        // --- Estado Físico ---
        public Vector2 InputVector { get; private set; }
        public Vector3 Velocity;
        public float CurrentSpeed { get; set; }
        public bool IsRunPressed { get; private set; }
        public bool IsJumpPressed { get; private set; }

        // --- Properties Locales ---
        public Vector3 CurrentVelocity => Velocity;
        public float MovementSpeed => new Vector3(Velocity.x, 0, Velocity.z).magnitude;

        // --- IMovementProvider Properties ---
        public bool IsGrounded => Controller.isGrounded;
        
        public float Height 
        { 
            get => Controller != null ? Controller.height : 2f; 
            set { if (Controller != null) Controller.height = value; } 
        }

        public Vector3 Center 
        { 
            get => Controller != null ? Controller.center : Vector3.up; 
            set { if (Controller != null) Controller.center = value; } 
        }

        public bool Enabled 
        { 
            get => Controller != null ? Controller.enabled : true; 
            set { if (Controller != null) Controller.enabled = value; } 
        }

        private void Awake()
        {
            Controller = GetComponent<CharacterController>();
            
            // Si InputManagerCore existe, lo usamos. Si no, quedará nulo y lo asignaremos luego.
            if (InputManagerCore.HasReference)
                input = InputManagerCore.Instance;

            // Inicializar Estados
            var idle = new FPIdleState(this);
            var walk = new FPWalkState(this);
            var run = new FPRunState(this);
            var jump = new FPJumpState(this);

            // Transiciones
            idle.Transitions = new List<StateTransition>
            {
                new("Walk", () => InputVector.magnitude > 0.1f && !IsRunPressed),
                new("Run", () => InputVector.magnitude > 0.1f && IsRunPressed),
                new("Jump", () => IsJumpPressed && IsGrounded)
            };

            walk.Transitions = new List<StateTransition>
            {
                new("Idle", () => InputVector.magnitude < 0.1f),
                new("Run", () => IsRunPressed),
                new("Jump", () => IsJumpPressed && IsGrounded)
            };

            run.Transitions = new List<StateTransition>
            {
                new("Idle", () => InputVector.magnitude < 0.1f),
                new("Walk", () => !IsRunPressed && InputVector.magnitude > 0.1f),
                new("Jump", () => IsJumpPressed && IsGrounded)
            };

            jump.Transitions = new List<StateTransition>
            {
                // Vuelve a Idle o Walk cuando toca el piso
                new("Idle", () => IsGrounded && Velocity.y <= 0 && InputVector.magnitude < 0.1f),
                new("Walk", () => IsGrounded && Velocity.y <= 0 && InputVector.magnitude > 0.1f)
            };

            RegisterState("Idle", idle);
            RegisterState("Walk", walk);
            RegisterState("Run", run);
            RegisterState("Jump", jump);

            SetInitialState("Idle");
        }

        private void Update()
        {
            if (input == null && InputManagerCore.HasReference)
                input = InputManagerCore.Instance;

            ReadInput();
            ApplyGravity();

            // Ejecuta OnStateUpdate del estado activo
            UpdateStateMachine();

            // Aplicar movimiento final
            Controller.Move(Velocity * Time.deltaTime);
        }

        private void ReadInput()
        {
            if (input != null)
            {
                InputVector = input.ReadInput<Vector2>(Controls.MOVEMENT);
                IsRunPressed = input.ReadButton(Controls.SPRINT);
                IsJumpPressed = input.ReadButton(Controls.JUMP);
            }
            else
            {
                // Fallback a Unity Input nativo si no hay InputSystem
                InputVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
                IsRunPressed = Input.GetKey(KeyCode.LeftShift);
                IsJumpPressed = Input.GetKeyDown(KeyCode.Space);
            }
        }

        private void ApplyGravity()
        {
            if (IsGrounded && Velocity.y < 0)
            {
                Velocity.y = -2f; // Mantener pegado al piso
            }
            Velocity.y += Gravity * Time.deltaTime;
        }

        // --- IMovementProvider Methods ---
        public CollisionFlags Move(Vector3 motion)
        {
            if (Controller != null && Controller.enabled)
                return Controller.Move(motion);
            return CollisionFlags.None;
        }

        public Vector3 GetFeetPosition()
        {
            return transform.position;
        }

        public Vector3 GetCenterPosition()
        {
            return transform.position + Center;
        }        /// <summary>Calcula el vector de dirección en base a la rotación de la cámara</summary>
        public Vector3 GetMovementDirection()
        {
            Vector3 forward = transform.forward * InputVector.y;
            Vector3 right = transform.right * InputVector.x;
            return (forward + right).normalized;
        }
    }
}
