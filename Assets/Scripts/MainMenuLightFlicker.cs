using UnityEngine;

public class MainMenuLightFlicker : MonoBehaviour
{
    private Light lightComponent;
    private float baseIntensity;

    [Header("Flicker Settings")]
    public float minIntensityMultiplier = 0.70f;
    public float maxIntensityMultiplier = 1.30f;
    public float flickerSpeed = 0.12f;

    void Start()
    {
        lightComponent = GetComponent<Light>();
        if (lightComponent != null)
        {
            baseIntensity = lightComponent.intensity;
        }
    }

    void Update()
    {
        if (lightComponent == null) return;
        
        // Simulates realistic fire lantern flicker using Perlin noise
        float noise = Mathf.PerlinNoise(Time.time * (1f / flickerSpeed), 0f);
        float multiplier = Mathf.Lerp(minIntensityMultiplier, maxIntensityMultiplier, noise);
        lightComponent.intensity = baseIntensity * multiplier;
    }
}
