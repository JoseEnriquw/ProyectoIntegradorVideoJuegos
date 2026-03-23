using UnityEngine;
using UnityEngine.Events;

namespace FuncionalidadesCore.Interaction
{
    /// <summary>
    /// Componente listo para usar.
    /// Emite un Raycast desde la cámara principal para detectar objetos con las interfaces IInteractStart e IInteractInfo.
    /// </summary>
    public class InteractionDetector : MonoBehaviour
    {
        [Header("Configuración del Raycast")]
        public float ReachDistance = 3f;
        public LayerMask InteractableLayers = ~0; // Todo por defecto
        
        [Header("Cámara (Dejar vacío para usar Camera.main)")]
        public Transform CameraTransform;

        [Header("Eventos de UI (UnityEvents)")]
        [Tooltip("Se dispara cuando miramos un objeto con IInteractInfo. Pasa el título del objeto.")]
        public UnityEvent<string> OnShowPrompt;
        [Tooltip("Se dispara cuando dejamos de mirar un objeto interactable.")]
        public UnityEvent OnHidePrompt;

        private IInteractStart currentInteractable;
        private IInputProvider input;

        private void Start()
        {
            if (CameraTransform == null)
            {
                if (Camera.main != null)
                    CameraTransform = Camera.main.transform;
                else
                    Debug.LogWarning("[InteractionDetector] No hay cámara principal. Asígnala manualmente.");
            }
        }

        private void Update()
        {
            if (CameraTransform == null) return;

            if (input == null && InputManagerCore.HasReference)
                input = InputManagerCore.Instance;

            DetectInteractables();
            CheckInput();
        }

        private void DetectInteractables()
        {
            Ray ray = new Ray(CameraTransform.position, CameraTransform.forward);
            
            if (Physics.Raycast(ray, out RaycastHit hit, ReachDistance, InteractableLayers))
            {
                var interactable = hit.collider.GetComponentInParent<IInteractStart>();

                if (interactable != null && interactable.CanInteract())
                {
                    if (currentInteractable != interactable)
                    {
                        currentInteractable = interactable;
                        
                        // Mostrar Prompt en UI
                        var info = hit.collider.GetComponentInParent<IInteractInfo>();
                        if (info != null)
                            OnShowPrompt?.Invoke(info.InteractTitle);
                        else
                            OnShowPrompt?.Invoke("Interactuar"); // Default fallback
                    }
                    return;
                }
            }

            // Si llegamos aquí, no miramos nada interactable
            if (currentInteractable != null)
            {
                currentInteractable = null;
                OnHidePrompt?.Invoke();
            }
        }

        private void CheckInput()
        {
            if (currentInteractable == null) return;

            bool isInteractPressed = false;
            
            if (input != null)
                isInteractPressed = input.ReadButtonOnce("detector", Controls.INTERACT);
            else
                isInteractPressed = Input.GetKeyDown(KeyCode.E); // Fallback clásico

            if (isInteractPressed)
            {
                currentInteractable.InteractStart();
            }
        }
    }
}
