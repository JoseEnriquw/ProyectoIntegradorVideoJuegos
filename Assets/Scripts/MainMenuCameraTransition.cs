using UnityEngine;

public class MainMenuCameraTransition : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject loadGamePanel;
    
    [Header("Transition Settings")]
    public float transitionSpeed = 3f;
    
    [Header("Waypoints Offsets")]
    public Vector3 optionsPosOffset = new Vector3(-2f, 0.8f, 1f);
    public Vector3 optionsRotOffset = new Vector3(-12f, 15f, 0f);
    
    public Vector3 loadPosOffset = new Vector3(0.8f, -0.4f, 4f);
    public Vector3 loadRotOffset = new Vector3(4f, -8f, 0f);
    
    private Vector3 defaultPos;
    private Quaternion defaultRot;
    
    private Vector3 targetPos;
    private Quaternion targetRot;
    
    private MainMenuCameraBreathing cameraBreathing;
    
    void Start()
    {
        defaultPos = transform.position;
        defaultRot = transform.rotation;
        
        targetPos = defaultPos;
        targetRot = defaultRot;
        
        cameraBreathing = GetComponent<MainMenuCameraBreathing>();
        
        // If cameraBreathing is active, set its initial anchor
        if (cameraBreathing != null)
        {
            cameraBreathing.startPosition = defaultPos;
            cameraBreathing.startRotation = defaultRot;
        }
    }
    
    void Update()
    {
        // 1. Determine Target position and rotation based on which panel is active
        if (optionsPanel != null && optionsPanel.activeInHierarchy)
        {
            targetPos = defaultPos + optionsPosOffset;
            targetRot = Quaternion.Euler(defaultRot.eulerAngles + optionsRotOffset);
        }
        else if (loadGamePanel != null && loadGamePanel.activeInHierarchy)
        {
            targetPos = defaultPos + loadPosOffset;
            targetRot = Quaternion.Euler(defaultRot.eulerAngles + loadRotOffset);
        }
        else
        {
            targetPos = defaultPos;
            targetRot = defaultRot;
        }
        
        // 2. Smoothly interpolate position and rotation
        if (cameraBreathing != null)
        {
            cameraBreathing.startPosition = Vector3.Lerp(cameraBreathing.startPosition, targetPos, Time.deltaTime * transitionSpeed);
            cameraBreathing.startRotation = Quaternion.Slerp(cameraBreathing.startRotation, targetRot, Time.deltaTime * transitionSpeed);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * transitionSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * transitionSpeed);
        }
    }
}
