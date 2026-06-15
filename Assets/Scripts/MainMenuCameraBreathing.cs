using UnityEngine;

public class MainMenuCameraBreathing : MonoBehaviour
{
    [Header("Translation Sway")]
    public float translationSpeed = 0.5f;
    public float translationAmount = 0.04f;

    [Header("Rotation Sway")]
    public float rotationSpeed = 0.35f;
    public float rotationAmount = 0.2f;

    [HideInInspector] public Vector3 startPosition;
    [HideInInspector] public Quaternion startRotation;

    void Start()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        float time = Time.time;
        
        // Horizontal and vertical Lissajous translation breathing
        float xOffset = Mathf.Sin(time * translationSpeed) * translationAmount;
        float yOffset = Mathf.Cos(time * translationSpeed * 0.7f) * translationAmount * 0.5f;
        transform.position = startPosition + new Vector3(xOffset, yOffset, 0f);

        // Slow pitch and yaw rotation sway
        float pitchOffset = Mathf.Sin(time * rotationSpeed) * rotationAmount;
        float yawOffset = Mathf.Cos(time * rotationSpeed * 0.8f) * rotationAmount;
        transform.rotation = startRotation * Quaternion.Euler(pitchOffset, yawOffset, 0f);
    }
}
