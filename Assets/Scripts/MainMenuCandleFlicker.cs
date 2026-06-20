using UnityEngine;

public class MainMenuCandleFlicker : MonoBehaviour
{
    private Light candleLight;
    private float baseIntensity;
    private Light thunderLight;
    private float noiseTime;

    [Header("Standard Flicker Settings")]
    public float minFlickerSpeed = 4f;
    public float maxFlickerSpeed = 8f;
    public float intensityRange = 0.12f;

    [Header("Storm Flicker Settings")]
    public float stormFlickerSpeed = 22f;
    public float stormIntensityRange = 0.45f;

    void Start()
    {
        candleLight = GetComponent<Light>();
        if (candleLight != null)
        {
            baseIntensity = candleLight.intensity;
        }
        noiseTime = Random.Range(0f, 100f);

        // Find the ThunderManager's light component
        var thunderMgr = Object.FindAnyObjectByType<AdvancedHorrorFPS.ThunderManager>();
        if (thunderMgr != null)
        {
            thunderLight = thunderMgr.GetComponent<Light>();
        }
    }

    void Update()
    {
        if (candleLight == null) return;

        // Check if the storm/lightning is currently flashing
        bool isStorming = (thunderLight != null && thunderLight.enabled && thunderLight.intensity > 0.1f);

        float speed = isStorming ? stormFlickerSpeed : Random.Range(minFlickerSpeed, maxFlickerSpeed);
        float range = isStorming ? stormIntensityRange : intensityRange;

        noiseTime += Time.deltaTime * speed;

        // Generate organic flicker using Perlin noise
        float noise = Mathf.PerlinNoise(noiseTime, 0f);
        
        // Apply intensity flicker relative to base intensity
        candleLight.intensity = baseIntensity + (noise - 0.5f) * 2f * range * baseIntensity;
        
        // If storming, occasionally dim the light significantly to simulate wind gusts blowing the flame
        if (isStorming && Random.value < 0.05f)
        {
            candleLight.intensity *= Random.Range(0.2f, 0.5f);
        }
    }
}
