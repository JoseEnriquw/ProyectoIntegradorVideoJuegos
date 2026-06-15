using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MainMenuGlitchEffect : MonoBehaviour
{
    public Volume volume;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private FilmGrain filmGrain;
    
    private float defaultCAIntensity = 0f;
    private float defaultLDIntensity = 0f;
    
    private Light thunderLight;
    private bool wasLightEnabled = false;
    
    private float glitchTimer = 0f;
    private float nextGlitchTime = 5f;
    
    void Start()
    {
        if (volume == null) volume = GetComponent<Volume>();
        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out chromaticAberration);
            volume.profile.TryGet(out lensDistortion);
            volume.profile.TryGet(out filmGrain);
        }
        
        if (chromaticAberration != null) defaultCAIntensity = chromaticAberration.intensity.value;
        if (lensDistortion != null) defaultLDIntensity = lensDistortion.intensity.value;
        
        // Find ThunderManager light safely
        var thunderManager = Object.FindAnyObjectByType<AdvancedHorrorFPS.ThunderManager>();
        if (thunderManager != null)
        {
            thunderLight = thunderManager.GetComponent<Light>();
        }
        
        ResetGlitchTime();
    }
    
    void Update()
    {
        bool isLightning = false;
        if (thunderLight != null)
        {
            bool lightEnabled = thunderLight.enabled && thunderLight.intensity > 0.1f;
            if (lightEnabled && !wasLightEnabled)
            {
                // Lightning just started! Trigger a heavy glitch
                StartCoroutine(TriggerHeavyGlitch());
            }
            wasLightEnabled = lightEnabled;
            isLightning = lightEnabled;
        }
        
        // Random micro-glitches when not in lightning
        if (!isLightning)
        {
            glitchTimer += Time.deltaTime;
            if (glitchTimer >= nextGlitchTime)
            {
                StartCoroutine(TriggerMicroGlitch());
                ResetGlitchTime();
            }
        }
    }
    
    void ResetGlitchTime()
    {
        glitchTimer = 0f;
        nextGlitchTime = Random.Range(4f, 12f);
    }
    
    IEnumerator TriggerHeavyGlitch()
    {
        if (chromaticAberration == null || lensDistortion == null) yield break;
        
        // Spike values
        chromaticAberration.intensity.Override(1.0f);
        lensDistortion.intensity.Override(-0.30f);
        if (filmGrain != null) filmGrain.intensity.Override(0.85f);
        
        yield return new WaitForSeconds(Random.Range(0.08f, 0.22f));
        
        // Restore partially
        chromaticAberration.intensity.Override(defaultCAIntensity + 0.15f);
        lensDistortion.intensity.Override(defaultLDIntensity - 0.08f);
        
        yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
        
        // Glitch again
        chromaticAberration.intensity.Override(0.75f);
        lensDistortion.intensity.Override(0.12f);
        
        yield return new WaitForSeconds(0.08f);
        
        // Reset to default
        chromaticAberration.intensity.Override(defaultCAIntensity);
        lensDistortion.intensity.Override(defaultLDIntensity);
        if (filmGrain != null) filmGrain.intensity.Override(0.15f); // Restore base film grain
    }
    
    IEnumerator TriggerMicroGlitch()
    {
        if (chromaticAberration == null || lensDistortion == null) yield break;
        
        float duration = Random.Range(0.06f, 0.14f);
        float elapsed = 0f;
        
        float targetCA = Random.Range(0.35f, 0.55f);
        float targetLD = Random.Range(-0.12f, 0.08f);
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            chromaticAberration.intensity.Override(Mathf.Lerp(defaultCAIntensity, targetCA, t));
            lensDistortion.intensity.Override(Mathf.Lerp(defaultLDIntensity, targetLD, t));
            yield return null;
        }
        
        chromaticAberration.intensity.Override(defaultCAIntensity);
        lensDistortion.intensity.Override(defaultLDIntensity);
    }
}
