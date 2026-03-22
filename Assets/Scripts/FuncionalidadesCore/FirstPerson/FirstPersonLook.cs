using UnityEngine;

namespace FuncionalidadesCore.FirstPerson
{
    /// <summary>
    /// Controlador de cámara (Mouse Look), desacoplado del movimiento.
    /// Soporta HeadBobbing básico inspirado en el paquete FPS.
    /// </summary>
    public class FirstPersonLook : MonoBehaviour
    {
        [Header("References")]
        public Transform PlayerBody;
        public Transform CameraRoot;      // Objeto vacío hijo del jugador
        public Transform CameraTransform; // La cámara real (puede estar separada del jugador)
        public FirstPersonController Controller;

        [Header("Look Settings")]
        public float MouseSensitivity = 2f;
        public float TopClamp = -90.0f;
        public float BottomClamp = 90.0f;

        [Header("Headbob Settings")]
        public bool EnableHeadbob = true;
        public float BobFrequency = 1.5f;
        public float BobAmount = 0.05f;

        private IInputProvider input;
        private float pitch;
        private float defaultPosY = 0;
        private float timer = 0;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            if (CameraRoot != null)
                defaultPosY = CameraRoot.localPosition.y;
        }

        private void Update()
        {
            if (input == null && InputManagerCore.HasReference)
                input = InputManagerCore.Instance;

            HandleLook();
            if (EnableHeadbob) HandleHeadbob();
        }

        private void LateUpdate()
        {
            // La cámara real sigue al CameraRoot suavemente para evitar stuttering de físicas
            if (CameraTransform != null && CameraRoot != null)
            {
                CameraTransform.position = CameraRoot.position;
                CameraTransform.rotation = CameraRoot.rotation;
            }
        }

        private void HandleLook()
        {
            Vector2 lookInput = Vector2.zero;

            if (input != null)
                lookInput = input.ReadInput<Vector2>(Controls.LOOK);
            else
                lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            // Rotar el cuerpo (Yaw)
            if (PlayerBody != null && lookInput.x != 0)
            {
                PlayerBody.Rotate(Vector3.up * (lookInput.x * MouseSensitivity));
            }

            // Rotar la cabeza (Pitch) (Se aplica al root)
            if (CameraRoot != null && lookInput.y != 0)
            {
                pitch -= lookInput.y * MouseSensitivity;
                pitch = Mathf.Clamp(pitch, TopClamp, BottomClamp);
                CameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void HandleHeadbob()
        {
            if (Controller == null || CameraRoot == null) return;
            if (!Controller.IsGrounded) return;

            float speed = Controller.MovementSpeed;
            if (speed > 0.1f)
            {
                // Multiplicador de frecuencia (corre = más rápido bobbing)
                float freqMultiplier = Controller.IsRunPressed ? 1.5f : 1f;
                
                timer += Time.deltaTime * BobFrequency * freqMultiplier;
                float offsetY = Mathf.Sin(timer) * BobAmount;
                
                CameraRoot.localPosition = new Vector3(
                    CameraRoot.localPosition.x,
                    Mathf.Lerp(CameraRoot.localPosition.y, defaultPosY + offsetY, Time.deltaTime * 10f),
                    CameraRoot.localPosition.z
                );
            }
            else
            {
                timer = 0;
                CameraRoot.localPosition = new Vector3(
                    CameraRoot.localPosition.x,
                    Mathf.Lerp(CameraRoot.localPosition.y, defaultPosY, Time.deltaTime * 5f),
                    CameraRoot.localPosition.z
                );
            }
        }
    }
}
