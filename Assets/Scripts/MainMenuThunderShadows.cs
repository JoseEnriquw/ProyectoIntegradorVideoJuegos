using UnityEngine;

public class MainMenuThunderShadows : MonoBehaviour
{
    private Light thunderLight;
    private Quaternion originalRotation;
    private float noiseTimeX;
    private float noiseTimeY;

    [Header("Shadow Drift Settings")]
    public float maxAngleDriftY = 10f;   // How far shadows sweep left/right
    public float maxAngleDriftX = 5f;    // How far shadows sweep forward/backward
    public float speedX = 28f;
    public float speedY = 38f;

    void Start()
    {
        thunderLight = GetComponent<Light>();
        originalRotation = transform.rotation;
        noiseTimeX = Random.Range(0f, 100f);
        noiseTimeY = Random.Range(0f, 100f);
    }

    void OnEnable()
    {
        if (thunderLight == null)
        {
            thunderLight = GetComponent<Light>();
        }
        originalRotation = transform.rotation;
    }

    void Update()
    {
        if (thunderLight != null && thunderLight.enabled && thunderLight.intensity > 0.1f)
        {
            noiseTimeX += Time.deltaTime * speedX;
            noiseTimeY += Time.deltaTime * speedY;

            // Generate high-speed erratic offsets using Perlin noise
            float offsetAngleY = (Mathf.PerlinNoise(noiseTimeY, 0f) - 0.5f) * 2f * maxAngleDriftY;
            float offsetAngleX = (Mathf.PerlinNoise(noiseTimeX, 0f) - 0.5f) * 2f * maxAngleDriftX;

            // Apply the offset rotation dynamically to sweep shadows tétrico-style
            transform.rotation = originalRotation * Quaternion.Euler(offsetAngleX, offsetAngleY, 0f);
        }
        else
        {
            // Reset to original rotation when the flash is over
            if (transform.rotation != originalRotation)
            {
                transform.rotation = originalRotation;
            }
        }
    }
}
