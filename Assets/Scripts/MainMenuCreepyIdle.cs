using UnityEngine;

public class MainMenuCreepyIdle : MonoBehaviour
{
    public Transform lookTarget;
    public float idleDelay = 4.0f; // Seconds before looking at the camera
    public float lookSpeed = 1.8f;
    public float rockSpeed = 2.2f;
    public float rockAmount = 3.5f;

    private Quaternion initialRotation;
    private float lastMouseMoveTime;
    private Vector3 lastMousePosition;
    private bool isLooking = false;

    void Start()
    {
        initialRotation = transform.localRotation;
        lastMouseMoveTime = Time.time;
        lastMousePosition = Input.mousePosition;

        if (lookTarget == null && Camera.main != null)
        {
            lookTarget = Camera.main.transform;
        }
    }

    void Update()
    {
        Vector3 currentMousePos = Input.mousePosition;
        if (Vector3.Distance(currentMousePos, lastMousePosition) > 1.0f)
        {
            lastMouseMoveTime = Time.time;
            lastMousePosition = currentMousePos;
            isLooking = false;
        }

        // Check if idle
        if (Time.time - lastMouseMoveTime > idleDelay)
        {
            isLooking = true;
        }

        // Base rocking effect (simulates a self-rocking horse)
        float rockAngle = Mathf.Sin(Time.time * rockSpeed) * rockAmount;

        if (isLooking && lookTarget != null)
        {
            // Direct vector pointing to camera
            Vector3 toCamera = lookTarget.position - transform.position;
            toCamera.y = 0; // Rotate only on Y axis
            
            if (toCamera.sqrMagnitude > 0.01f)
            {
                Quaternion targetLook = Quaternion.LookRotation(toCamera);
                // Combine look rotation with rocking oscillation
                Quaternion targetRot = targetLook * Quaternion.Euler(rockAngle, 0, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lookSpeed);
            }
        }
        else
        {
            // Return to default orientation, still applying the rocking motion
            Quaternion targetRot = initialRotation * Quaternion.Euler(rockAngle, 0, 0);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRot, Time.deltaTime * lookSpeed * 2.0f);
        }
    }
}
