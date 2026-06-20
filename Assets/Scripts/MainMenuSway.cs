using UnityEngine;

public class MainMenuSway : MonoBehaviour
{
    private Quaternion originalRotation;
    private float noiseTimeX;
    private float noiseTimeZ;

    [Header("Sway Settings")]
    public float speedX = 1.0f;
    public float speedZ = 0.8f;
    public float maxAngleX = 3.0f;
    public float maxAngleZ = 4.0f;

    [Header("Random Offset")]
    public bool randomStartupOffset = true;

    void Start()
    {
        originalRotation = transform.localRotation;
        
        if (randomStartupOffset)
        {
            noiseTimeX = Random.Range(0f, 100f);
            noiseTimeZ = Random.Range(0f, 100f);
        }
    }

    void Update()
    {
        noiseTimeX += Time.deltaTime * speedX;
        noiseTimeZ += Time.deltaTime * speedZ;

        // Smooth swing movement on X and Z axes using Sin
        float angleX = Mathf.Sin(noiseTimeX) * maxAngleX;
        float angleZ = Mathf.Cos(noiseTimeZ) * maxAngleZ;

        // Apply rotation relative to original local rotation
        transform.localRotation = originalRotation * Quaternion.Euler(angleX, 0f, angleZ);
    }
}
