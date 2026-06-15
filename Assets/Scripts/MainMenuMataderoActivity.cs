using UnityEngine;

public class MainMenuMataderoActivity : MonoBehaviour
{
    private Light activityLight;
    private Vector3 basePosition;
    private float noiseTimeX;
    private float noiseTimeZ;

    [Header("Movement Settings")]
    public float moveSpeed = 0.5f;
    public Vector3 moveRange = new Vector3(8f, 1f, 3f);

    [Header("Intensity Settings")]
    public float baseIntensity = 3.0f;
    public float flickerSpeed = 4f;

    [Header("Cycle Settings")]
    public float minActiveTime = 10f;
    public float maxActiveTime = 25f;
    public float minInactiveTime = 5f;
    public float maxInactiveTime = 15f;

    private float nextToggleTime;
    private bool isActiveState = true;

    void Start()
    {
        activityLight = GetComponent<Light>();
        basePosition = transform.localPosition;
        noiseTimeX = Random.Range(0f, 100f);
        noiseTimeZ = Random.Range(0f, 100f);
        nextToggleTime = Time.time + Random.Range(minActiveTime, maxActiveTime);
    }

    void Update()
    {
        if (activityLight == null) return;

        // Toggle state periodically
        if (Time.time > nextToggleTime)
        {
            isActiveState = !isActiveState;
            if (isActiveState)
            {
                nextToggleTime = Time.time + Random.Range(minActiveTime, maxActiveTime);
                activityLight.enabled = true;
            }
            else
            {
                nextToggleTime = Time.time + Random.Range(minInactiveTime, maxInactiveTime);
                activityLight.enabled = false;
            }
        }

        if (isActiveState)
        {
            // Move light around inside the building using Perlin noise for smooth organic path
            noiseTimeX += Time.deltaTime * moveSpeed;
            noiseTimeZ += Time.deltaTime * moveSpeed;

            float xOffset = (Mathf.PerlinNoise(noiseTimeX, 0f) - 0.5f) * 2f * moveRange.x;
            float yOffset = (Mathf.PerlinNoise(0f, noiseTimeX) - 0.5f) * 2f * moveRange.y;
            float zOffset = (Mathf.PerlinNoise(noiseTimeZ, noiseTimeZ) - 0.5f) * 2f * moveRange.z;

            transform.localPosition = basePosition + new Vector3(xOffset, yOffset, zOffset);

            // Modulate intensity (slight flicker + panning look)
            float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
            activityLight.intensity = baseIntensity * (0.6f + flicker * 0.4f);
        }
    }
}
