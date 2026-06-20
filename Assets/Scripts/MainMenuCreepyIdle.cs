using UnityEngine;

public class MainMenuCreepyIdle : MonoBehaviour
{
    public Transform lookTarget; // Optional default look target
    public float lookSpeed = 2.0f;
    public float rockSpeed = 2.2f;
    public float rockAmount = 3.5f;
    public float idleDelay = 4.0f; // Retained for compatibility with editor builder

    [Header("Cursor Tracking Settings")]
    public bool trackCursor = true;
    public float maxRotationAngle = 60f; // Limit horizontal rotation so they don't break neck

    private Quaternion initialRotation;
    private Camera mainCam;

    void Start()
    {
        initialRotation = transform.localRotation;
        mainCam = Camera.main;
    }

    void Update()
    {
        // Rocking oscillation (always applied)
        float rockAngle = Mathf.Sin(Time.time * rockSpeed) * rockAmount;

        Vector3 targetWorldLookPoint = Vector3.zero;
        bool hasTarget = false;

        if (trackCursor && mainCam != null)
        {
            // Cast ray from camera to a plane facing the camera at the entity's position
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(-mainCam.transform.forward, transform.position);
            float enter;
            if (plane.Raycast(ray, out enter))
            {
                targetWorldLookPoint = ray.GetPoint(enter);
                hasTarget = true;
            }
        }
        else if (lookTarget != null)
        {
            targetWorldLookPoint = lookTarget.position;
            hasTarget = true;
        }

        if (hasTarget)
        {
            Vector3 toTarget = targetWorldLookPoint - transform.position;
            toTarget.y = 0; // Keep rotation strictly on Y axis

            if (toTarget.sqrMagnitude > 0.01f)
            {
                Quaternion targetLook = Quaternion.LookRotation(toTarget);
                
                // Check angle relative to initial rotation to avoid excessive turning
                float angle = Quaternion.Angle(initialRotation, targetLook);
                if (angle <= maxRotationAngle)
                {
                    Quaternion targetRot = targetLook * Quaternion.Euler(rockAngle, 0, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * lookSpeed);
                    return; // Avoid executing fallback
                }
            }
        }

        // Fallback: Return to default orientation, still applying the rocking motion
        Quaternion defaultRot = initialRotation * Quaternion.Euler(rockAngle, 0, 0);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, defaultRot, Time.deltaTime * lookSpeed);
    }
}
