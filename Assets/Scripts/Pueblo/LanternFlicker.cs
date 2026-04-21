using UnityEngine;

public class LanternFlicker : MonoBehaviour
{
    public Light lanternLight;

    [Header("Base Settings")]
    public float baseIntensity = 3f;

    [Header("Flicker Settings")]
    public float flickerAmount = 1f;
    public float speed = 3f;

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * speed, 0.0f);
        float flicker = baseIntensity + (noise * flickerAmount);

        // Pequeña probabilidad de apagón
        if (Random.value < 0.01f)
        {
            flicker *= 0.2f;
        }

        lanternLight.intensity = flicker;
    }
}