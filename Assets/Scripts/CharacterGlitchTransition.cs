using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UHFPS.Runtime;
using UHFPS.Rendering;

public class CharacterGlitchTransition : MonoBehaviour
{
    [Tooltip("El personaje (o modelo) que va a desaparecer")]
    public GameObject targetCharacter;
    
    public enum TransitionActionEnum { HideTarget, ShowTarget, DoNothing }
    [Header("Climax Action")]
    [Tooltip("¿Qué acción realizar sobre el targetCharacter en el punto climax del glitch?")]
    public TransitionActionEnum transitionAction = TransitionActionEnum.HideTarget;
    
    [Tooltip("Duración total del efecto de post-procesado en segundos")]
    public float duration = 0.25f;

    [Header("Efectos de Post-Procesado (URP)")]
    [Tooltip("Qué tanta aberración cromática se aplicará en el pico del glitch")]
    public float maxChromaticAberration = 1.5f;
    [Tooltip("Distorsión de lente para el efecto de jalón de cámara")]
    public float maxLensDistortion = -0.7f;
    [Tooltip("Exposición de color para simular un flash o destello en la mente")]
    public float maxColorExposure = 2.5f; 
    
    [Header("Integración UHFPS (Miedo y Sacudida)")]
    [Tooltip("¿Aplicar sacudida de cámara (wobble) usando el sistema de UHFPS?")]
    public bool applyWobble = true;
    [Tooltip("La fuerza del mareo/balanceo de la cámara")]
    public float wobbleAmplitude = 2f;
    [Tooltip("La frecuencia (velocidad) del balanceo. Valores bajos (1-2) causan mareo lento, valores altos causan temblor rápido.")]
    public float wobbleFrequency = 1.2f;
    [Tooltip("Duración del efecto de mareo en segundos")]
    public float wobbleDuration = 2.5f;

    [Header("Look At (Mirar al Personaje)")]
    [Tooltip("¿Forzar al jugador a mirar al personaje durante la transición?")]
    public bool lookAtTarget = true;
    [Tooltip("El transform al que mirará el jugador. Si está vacío, mirará al targetCharacter.")]
    public Transform customLookTarget;
    [Tooltip("Duración de la rotación de cámara hacia el objetivo")]
    public float lookAtDuration = 0.5f;
    [Tooltip("¿Bloquear los controles de vista del jugador durante la rotación?")]
    public bool lockPlayerRotation = true;

    [Tooltip("¿Aplicar efecto de pánico/tentáculos de UHFPS?")]
    public bool applyFearTentacles = true;
    [Range(0f, 1f)] public float tentaclesIntensity = 0.7f;
    public float tentaclesSpeed = 1.5f;
    [Range(0f, 1f)] public float vignetteStrength = 0.85f;
    [Tooltip("Duración que permanecerá la viñeta de miedo antes de desvanecerse")]
    public float fearEffectDuration = 3.0f;

    [Header("Parpadeo de Luces (Atmósfera)")]
    [Tooltip("¿Hacer parpadear las luces cercanas cuando ocurra el glitch?")]
    public bool flickerLights = true;
    [Tooltip("Radio de búsqueda de luces en metros")]
    public float lightFlickerRadius = 15f;
    [Tooltip("¿Apagar las luces permanentemente después del glitch (simula fundido de bombillas)?")]
    public bool turnLightsOffPermanently = false;

    [Header("Audio (Tensión)")]
    [Tooltip("Sonido principal que suena justo al iniciar el glitch (ej. creepy-distortion o un impacto)")]
    public AudioClip glitchSound;
    [Tooltip("Sonido secundario que suena tras la desaparición (ej. whisper, suspiro, heartbeat)")]
    public AudioClip lingeringSound;
    [Range(0f, 1f)] public float lingeringSoundVolume = 0.7f;

    [Header("Trigger automático")]
    [Tooltip("¿Activar la transición automáticamente cuando el jugador entra al trigger? (Requiere un Collider con IsTrigger activo)")]
    public bool triggerOnPlayerEnter = true;
    [Tooltip("¿Solo activar una vez?")]
    public bool triggerOnlyOnce = true;

    [Header("Eventos")]
    [Tooltip("Eventos que se ejecutan al iniciar el efecto de transición (justo al disparar el glitch)")]
    public UnityEvent onEffectStart;
    
    [Tooltip("Eventos que se ejecutan cuando la transición ha terminado por completo (glitch volumen en 0)")]
    public UnityEvent onEffectEnded;

    private Volume glitchVolume;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;
    private ColorAdjustments colorAdjustments;
    private AudioSource audioSource;
    private AudioSource lingeringAudioSource;
    private bool hasTriggered = false;

    private struct LightState
    {
        public Light lightObj;
        public bool originalEnabled;
        public float originalIntensity;
    }
    private List<LightState> affectedLights = new List<LightState>();

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnPlayerEnter && !hasTriggered && other.CompareTag("Player"))
        {
            hasTriggered = true;
            TriggerTransition();
        }
    }

    void Awake()
    {
        // Crear un volumen global temporal con alta prioridad para controlar el post-procesado
        GameObject volumeObject = new GameObject("GlitchTransitionVolume");
        volumeObject.transform.SetParent(transform);
        
        glitchVolume = volumeObject.AddComponent<Volume>();
        glitchVolume.isGlobal = true;
        glitchVolume.priority = 9999;
        glitchVolume.weight = 0f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        
        chromaticAberration = profile.Add<ChromaticAberration>(true);
        lensDistortion = profile.Add<LensDistortion>(true);
        colorAdjustments = profile.Add<ColorAdjustments>(true);

        glitchVolume.profile = profile;

        if (glitchSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (lingeringSound != null)
        {
            lingeringAudioSource = gameObject.AddComponent<AudioSource>();
            lingeringAudioSource.playOnAwake = false;
            lingeringAudioSource.loop = false;
        }
    }

    /// <summary>
    /// Llama a esta función para hacer desaparecer al personaje con todos los efectos de terror.
    /// </summary>
    public void TriggerTransition()
    {
        // No retornamos temprano para permitir transiciones basadas únicamente en eventos o customLookTarget
        if (targetCharacter == null && customLookTarget == null)
        {
            Debug.LogWarning("GlitchTransition: No hay un Target Character ni un Custom Look Target asignado.");
        }
        
        StopAllCoroutines();
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // 1. Aplicar Look At usando el LookController de UHFPS primero
        if (lookAtTarget && GameManager.Instance != null && GameManager.Instance.PlayerPresence != null)
        {
            try
            {
                Transform targetToLook = customLookTarget != null ? customLookTarget : (targetCharacter != null ? targetCharacter.transform : null);
                if (targetToLook != null)
                {
                    var lookController = GameManager.Instance.PlayerPresence.LookController;
                    if (lookController != null)
                    {
                        lookController.LerpRotation(targetToLook, lookAtDuration, lockPlayerRotation);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("GlitchTransition: No se pudo aplicar el LookAt del jugador. " + ex.Message);
            }
            
            // Esperar a que la cámara termine de girar hacia el personaje antes de empezar el glitch y el evento
            yield return new WaitForSeconds(lookAtDuration);
        }

        float halfDuration = duration / 2f;
        float elapsed = 0f;

        // Invocar evento de inicio (ahora se ejecuta justo después de terminar la rotación del LookAt)
        onEffectStart?.Invoke();

        // 2. Sonidos
        if (audioSource != null && glitchSound != null)
        {
            audioSource.PlayOneShot(glitchSound);
        }

        // 3. Aplicar Wobble/Shake de cámara usando UHFPS
        if (applyWobble && GameManager.Instance != null && GameManager.Instance.PlayerPresence != null)
        {
            try
            {
                var playerManager = GameManager.Instance.PlayerPresence.PlayerManager;
                var wobbleMotion = playerManager.MotionController.GetDefaultMotion<WobbleMotion>();
                if (wobbleMotion != null)
                {
                    wobbleMotion.ApplyWobble(wobbleAmplitude, wobbleFrequency, wobbleDuration);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("GlitchTransition: No se pudo aplicar el Wobble de cámara de UHFPS. " + ex.Message);
            }
        }

        // 3. Aplicar Efecto de Miedo/Tentáculos de UHFPS
        if (applyFearTentacles && GameManager.Instance != null)
        {
            try
            {
                var fearTentacles = GameManager.Instance.GetStack<FearTentancles>();
                if (fearTentacles != null)
                {
                    fearTentacles.TentaclesPosition.value = Mathf.Lerp(0f, 0.2f, tentaclesIntensity);
                    fearTentacles.TentaclesSpeed.value = tentaclesSpeed;
                    fearTentacles.VignetteStrength.value = vignetteStrength;
                    
                    StartCoroutine(FearFadeRoutine(fearTentacles));
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("GlitchTransition: No se pudo aplicar el Miedo/Tentáculos de UHFPS. " + ex.Message);
            }
        }

        // 4. Capturar luces locales cercanas
        if (flickerLights)
        {
            affectedLights.Clear();
            Light[] allLights = FindObjectsOfType<Light>();
            foreach (var l in allLights)
            {
                if (l != null && l.gameObject.activeInHierarchy)
                {
                    float dist = Vector3.Distance(l.transform.position, transform.position);
                    if (dist <= lightFlickerRadius)
                    {
                        affectedLights.Add(new LightState
                        {
                            lightObj = l,
                            originalEnabled = l.enabled,
                            originalIntensity = l.intensity
                        });
                    }
                }
            }
        }

        // Fase 1: Aumentar el Glitch bruscamente y hacer parpadear luces
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            t = t * t; // EaseIn para entrada brusca

            glitchVolume.weight = t;
            chromaticAberration.intensity.Override(Mathf.Lerp(0f, maxChromaticAberration, t));
            lensDistortion.intensity.Override(Mathf.Lerp(0f, maxLensDistortion, t));
            colorAdjustments.postExposure.Override(Mathf.Lerp(0f, maxColorExposure, t));

            if (flickerLights)
            {
                foreach (var ls in affectedLights)
                {
                    if (ls.lightObj != null)
                    {
                        ls.lightObj.enabled = Random.value > 0.4f; // Parpadeo
                        ls.lightObj.intensity = ls.originalIntensity * Random.Range(0.2f, 1.3f);
                    }
                }
            }
            
            yield return null;
        }

        // --- PUNTO CLÍMAX: Acción sobre el personaje ---
        if (targetCharacter != null)
        {
            if (transitionAction == TransitionActionEnum.HideTarget)
                targetCharacter.SetActive(false);
            else if (transitionAction == TransitionActionEnum.ShowTarget)
                targetCharacter.SetActive(true);
        }

        // Reproducir sonido ambiental que se queda (susurro, latido)
        if (lingeringAudioSource != null && lingeringSound != null)
        {
            lingeringAudioSource.volume = lingeringSoundVolume;
            lingeringAudioSource.PlayOneShot(lingeringSound);
        }

        elapsed = 0f;

        // Fase 2: Reducir el Glitch y volver a la normalidad (o apagar luces)
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeOutT = 1f - (1f - t) * (1f - t); // EaseOut

            glitchVolume.weight = 1f - easeOutT;
            chromaticAberration.intensity.Override(Mathf.Lerp(maxChromaticAberration, 0f, easeOutT));
            lensDistortion.intensity.Override(Mathf.Lerp(maxLensDistortion, 0f, easeOutT));
            colorAdjustments.postExposure.Override(Mathf.Lerp(maxColorExposure, 0f, easeOutT));

            if (flickerLights && !turnLightsOffPermanently)
            {
                foreach (var ls in affectedLights)
                {
                    if (ls.lightObj != null)
                    {
                        ls.lightObj.enabled = Random.value > 0.2f;
                        ls.lightObj.intensity = Mathf.Lerp(ls.lightObj.intensity, ls.originalIntensity, easeOutT);
                    }
                }
            }
            
            yield return null;
        }

        // Desactivar post-procesado temporal por completo
        glitchVolume.weight = 0f;

        // Estado final de las luces
        if (flickerLights)
        {
            foreach (var ls in affectedLights)
            {
                if (ls.lightObj != null)
                {
                    if (turnLightsOffPermanently)
                    {
                        ls.lightObj.enabled = false;
                    }
                    else
                    {
                        ls.lightObj.enabled = ls.originalEnabled;
                        ls.lightObj.intensity = ls.originalIntensity;
                    }
                }
            }
        }

        // Invocar evento de finalización
        onEffectEnded?.Invoke();
    }

    private IEnumerator FearFadeRoutine(FearTentancles fear)
    {
        // Fundido a tope de viñeta y tentáculos
        float elapsed = 0f;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            fear.EffectFade.value = Mathf.Lerp(0f, 1f, elapsed / 0.15f);
            yield return null;
        }
        fear.EffectFade.value = 1f;

        // Mantener la viñeta de miedo activa un tiempo
        yield return new WaitForSeconds(fearEffectDuration);

        // Desvanecimiento lento de la viñeta de miedo
        elapsed = 0f;
        float fadeOutDuration = 1.5f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            fear.EffectFade.value = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        fear.EffectFade.value = 0f;
    }
}
