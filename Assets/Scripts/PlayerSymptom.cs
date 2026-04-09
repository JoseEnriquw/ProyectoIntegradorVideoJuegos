using UnityEngine;
using UnityEngine.Rendering.Universal;
using UHFPS.Runtime;

public class PlayerSymptom : MonoBehaviour
{
    [Header("General Settings")]
    public bool EnableSymptoms = true;

    [Header("Progression Settings")]
    [Tooltip("If true, the symptoms will start with 0 intensity and grow over time.")]
    public bool EnableProgression = true;
    [Tooltip("How many minutes it takes to reach the maximum intensity.")]
    public float MinutesToMaxIntensity = 5f;

    [Header("Blurred Vision Symptom")]
    public float TimeBetweenSymptoms = 10f;
    public float SymptomDuration = 3f;
    
    [Range(1f, 15f)]
    public float MaxBlurIntensity = 6f;
    [Range(0f, 1f)]
    public float MaxTunnelIntensity = 0.6f;
    
    public float BlurTransitionSpeed = 1f;

    private float timer;
    private bool isBlurActive;
    private float timeAlive = 0f;

    // Evaluated Intensities
    private float currentBlurTarget;
    private float currentTunnelTarget;

    // Vignette / Tunnel variables
    private Vignette vignette;
    private float originalVignetteIntensity;
    private float targetVignetteIntensity;
    private float currentVignetteIntensity;

    void Start()
    {
        timer = TimeBetweenSymptoms;

        // Intentar obtener el componente Vignette del Post Processing global
        if (GameManager.Instance != null && GameManager.Instance.GlobalPPVolume != null)
        {
            if (GameManager.Instance.GlobalPPVolume.profile.TryGet(out vignette))
            {
                originalVignetteIntensity = vignette.intensity.value;
                currentVignetteIntensity = originalVignetteIntensity;
                targetVignetteIntensity = originalVignetteIntensity;
            }
        }
    }

    void Update()
    {
        timeAlive += Time.deltaTime;

        // Calcular la intensidad actual si hay progresion activa
        if (EnableProgression)
        {
            float limitSeconds = MinutesToMaxIntensity * 60f;
            float progress = Mathf.Clamp01(timeAlive / limitSeconds);
            
            // Starts at roughly 0 and scales to Max
            currentBlurTarget = Mathf.Lerp(0f, MaxBlurIntensity, progress);
            currentTunnelTarget = Mathf.Lerp(originalVignetteIntensity, MaxTunnelIntensity, progress);
        }
        else
        {
            currentBlurTarget = MaxBlurIntensity;
            currentTunnelTarget = MaxTunnelIntensity;
        }

        // Smooth transition para el efecto tunel
        if (vignette != null)
        {
            float speed = Mathf.Max(0.5f, Mathf.Abs(targetVignetteIntensity - originalVignetteIntensity)) / BlurTransitionSpeed;
            if (speed > 0f)
            {
                currentVignetteIntensity = Mathf.MoveTowards(currentVignetteIntensity, targetVignetteIntensity, Time.deltaTime * speed);
                vignette.intensity.value = currentVignetteIntensity;
            }
        }

        if (!EnableSymptoms)
        {
            if (isBlurActive)
            {
                EndBlur();
            }
            return;
        }

        timer -= Time.deltaTime;

        if (!isBlurActive && timer <= 0f)
        {
            StartBlur();
        }
        else if (isBlurActive && timer <= -SymptomDuration)
        {
            EndBlur();
        }
    }

    private void StartBlur()
    {
        isBlurActive = true;
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.EnableBlur && currentBlurTarget > 0.1f)
            {
                GameManager.Instance.InterpolateBlur(currentBlurTarget, BlurTransitionSpeed);
            }
            
            if (vignette != null)
            {
                targetVignetteIntensity = currentTunnelTarget;
            }
        }
    }

    private void EndBlur()
    {
        isBlurActive = false;
        timer = TimeBetweenSymptoms;

        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.EnableBlur)
            {
                GameManager.Instance.InterpolateBlur(0f, BlurTransitionSpeed);
            }

            if (vignette != null)
            {
                targetVignetteIntensity = originalVignetteIntensity;
            }
        }
    }

    private void OnDisable()
    {
        if (isBlurActive)
        {
            EndBlur();
        }
    }
}
