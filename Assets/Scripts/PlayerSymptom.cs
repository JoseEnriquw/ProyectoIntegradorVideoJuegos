using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UHFPS.Runtime;
using UHFPS.Rendering;
using System.Collections.Generic; // For DualKawaseBlur, Scanlines

public class PlayerSymptom : MonoBehaviour
{
    public enum SymptomType { None, Blur, BlackAndWhite, VHS, Drunk, Whispers, Rain }
    [Header("General Settings")]
    public bool EnableSymptoms = true;

    [Header("First Symptom Event")]
    [Tooltip("Evento que se dispara la primera vez que un síntoma se presenta en el juego.")]
    public UnityEvent OnFirstSymptomActivated;

    private bool hasTriggeredFirstSymptom = false;

    [Header("Timing Settings")]
    [Tooltip("Time in seconds before a new random symptom appears (used if UseRandomTimeRange is false).")]
    public float TimeBetweenSymptoms = 180f;
    [Tooltip("¿Usar un rango de tiempo aleatorio en minutos entre síntomas?")]
    public bool UseRandomTimeRange = true;
    [Tooltip("Tiempo mínimo en minutos antes de que aparezca un nuevo síntoma.")]
    public float MinMinutesBetweenSymptoms = 1f;
    [Tooltip("Tiempo máximo en minutos antes de que aparezca un nuevo síntoma.")]
    public float MaxMinutesBetweenSymptoms = 3f;

    [Header("Progression Settings")]
    [Tooltip("If true, the symptoms will gradually increase over time.")]
    public bool EnableProgression = true;
    [Tooltip("How many minutes it takes to reach the maximum intensity.")]
    public float MinutesToMaxIntensity = 5f;    
    [Header("Symptom Types")]
    public bool EnableBlurAndTunnel = true;
    public bool EnableBlackAndWhite = true;
    public bool EnableVHSGlitch = true;
    public bool EnableDrunkMotion = true;
    public bool EnableWhispers = true;
    public bool EnableRain = true;
    
    [Header("Blur & Tunnel Intensities")]
    [Range(0f, 15f)]
    public float MaxBlurIntensity = 2.5f;
    [Range(0f, 1f)]
    public float MaxTunnelIntensity = 0.6f;
    
    [Header("Colors Intensities")]
    [Range(-100f, 0f)]
    public float MinSaturation = -100f; 

    [Header("VHS & Glitch Intensities")]
    [Range(0f, 2f)] 
    public float MaxScanlinesStrength = 1f;
    [Range(0f, 1f)] 
    public float MaxGlitchIntensity = 0.3f;
    [Range(0f, 1f)] 
    public float MaxChromaticAberration = 0.8f;
    [Range(-1f, 1f)] 
    public float MaxLensDistortion = 0.35f;

    [Header("Drunk Motion Intensities")]
    [Range(0f, 30f)]
    public float MaxDrunkSwayAngle = 5f; 
    [Range(0f, 90f)]
    public float MaxDrunkSpinAngle = 25f; 
    [Range(0f, 5f)]
    public float DrunkSwaySpeed = 1.5f; 
    [Range(0f, 50f)]
    public float MaxDrunkSidewaysForce = 15f; 
    [Range(0f, 100f)]
    public float DrunkStumbleMultiplier = 35f; // Amplificador de inercia para "pasos dobles" al costado
    [Range(0f, 1f)]
    public float MaxMotionBlur = 1f; // Difumina el giro creando estela de "velocidad"
    
    [Header("Rain (Film Grain) Intensities")]
    [Range(0f, 1f)]
    public float MaxRainIntensity = 1f;
    public FilmGrainLookup RainType = FilmGrainLookup.Large01;

    // Restauramos el nombre original de la variable para recuperar el ajuste de velocidad
    public float BlurTransitionSpeed = 1f;

    [Header("Symptom Sounds")]
    public AudioClip[] BlurSounds;
    public AudioClip[] BlackAndWhiteSounds;
    public AudioClip[] VHSSounds;
    public AudioClip[] DrunkSounds;
    public AudioClip[] WhispersSounds;
    public AudioClip[] RainSounds;
    [Range(0f, 1f)]
    public float SymptomsAudioVolume = 0.8f;

    [Header("Cure Settings")]
    public AudioClip CureSound;
    public AudioClip CureSoundSecondary;
    [Range(0f, 1f)]
    public float CureSoundVolume = 1f;

    [Header("Intro Sequence")]
    [Tooltip("Tiempo que espera al iniciar la escena para lanzar el síntoma (útil para saltar pantallas negras de carga)")]
    public float IntroSymptomDelay = 2f;
    public DialogueTrigger IntroDialogue;

    [Header("Player Voice Reactions")]
    [Tooltip("Audios que el PJ puede decir JUSTO ANTES de que arranque el síntoma.")]
    public AudioClip[] PreSymptomVoices;
    [Tooltip("Probabilidad (0 a 1) de que diga algo antes del síntoma.")]
    [Range(0f, 1f)] public float PreSymptomVoiceChance = 0.5f;

    [Tooltip("Diálogos (con subtítulos) que el PJ puede decir MIENTRAS dure el síntoma.")]
    public UHFPS.Scriptable.DialogueAsset[] DuringSymptomVoices;
    [Tooltip("Probabilidad (0 a 1) de que diga algo durante el síntoma.")]
    [Range(0f, 1f)] public float DuringSymptomVoiceChance = 0.5f;
    public float MinDuringSymptomDelay = 5f;
    public float MaxDuringSymptomDelay = 15f;

    [Header("Fatigue Settings")]
    [Tooltip("Tiempo máximo (en segundos) que el jugador puede correr antes de cansarse.")]
    public float MaxSprintDuration = 5f;
    [Tooltip("Tiempo de recuperación (en segundos) que debe pasar para volver a correr después de cansarse.")]
    public float FatigueRecoveryTime = 3f;
    [Tooltip("Sonido de cansancio que se reproduce cuando el jugador se agota por correr.")]
    public AudioClip FatigueSound;
    [Tooltip("Volumen del sonido de cansancio.")]
    [Range(0f, 1f)] public float FatigueVolume = 0.8f;

    private AudioSource voiceAudioSource;
    private bool isStartingSymptom = false;
    private Coroutine symptomRoutine;
    private Coroutine duringVoiceRoutine;
    private DialogueTrigger[] duringSymptomTriggers;

    private float timer;
    private float timeAlive = 0f;
    private SymptomType currentActiveSymptom = SymptomType.None;
    private float whispersWeight = 0f;
    private AudioSource symptomAudioSource;

    private float sprintDurationElapsed = 0f;
    private float fatigueCooldownTimer = 0f;
    private bool isFatigued = false;

    // Utilizamos volúmenes separados internamente para que no haya conflictos de compatibilidad
    private GameObject blurVolumeObject;
    private Volume blurVolume;
    private DualKawaseBlur symptomBlur;
    private Vignette symptomVignette;

    private GameObject bwVolumeObject;
    private Volume bwVolume;
    private ColorAdjustments symptomColorAdj;

    private GameObject vhsVolumeObject;
    private Volume vhsVolume;
    private Scanlines symptomScanlines;
    private ChromaticAberration symptomChromaticAberration;
    private LensDistortion symptomLensDistortion;

    private GameObject drunkVolumeObject;
    private Volume drunkVolume;
    private MotionBlur symptomMotionBlur;

    private GameObject rainVolumeObject;
    private Volume rainVolume;
    private FilmGrain symptomGrain;
    private ColorAdjustments symptomRainColorAdj;
    private Scanlines symptomRainScanlines;
    private ParticleSystem rainParticleSystem;

    private PlayerStateMachine playerStateMachine;

    public static PlayerSymptom Instance { get; private set; }
    public SymptomType CurrentSymptom => currentActiveSymptom;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    [ContextMenu("Forzar Síntoma: Lluvia")]
    public void DebugForceRain()
    {
        if (!Application.isPlaying) return;
        StopVoiceRoutines();
        currentActiveSymptom = SymptomType.Rain;
        timeAlive = MinutesToMaxIntensity * 60f; 
        if (rainVolume != null) rainVolume.weight = 1f;
        PlaySymptomSound(SymptomType.Rain);
        Debug.Log("[PlayerSymptom] Debug: Lluvia forzada.");
    }

    void Start()
    {
        timer = GetRandomTimeBetweenSymptoms();
        playerStateMachine = GetComponent<PlayerStateMachine>();

        // --- 0. CONFIGURAR FUENTE DE AUDIO ---
        GameObject audioObj = new GameObject("SymptomAudioSource");
        audioObj.transform.SetParent(transform);
        audioObj.transform.localPosition = Vector3.zero;
        symptomAudioSource = audioObj.AddComponent<AudioSource>();
        symptomAudioSource.spatialBlend = 0f; // Sonido 2D (en la cabeza del jugador)
        symptomAudioSource.volume = SymptomsAudioVolume;
        symptomAudioSource.playOnAwake = false;
        symptomAudioSource.loop = true;

        GameObject voiceObj = new GameObject("PlayerVoiceSource");
        voiceObj.transform.SetParent(transform);
        voiceObj.transform.localPosition = Vector3.zero;
        voiceAudioSource = voiceObj.AddComponent<AudioSource>();
        voiceAudioSource.spatialBlend = 0f; // 2D (en la cabeza del jugador)
        voiceAudioSource.playOnAwake = false;
        voiceAudioSource.loop = false;

        // Instanciar los Triggers ocultos para usar DialogueAssets en el during
        if (DuringSymptomVoices != null && DuringSymptomVoices.Length > 0)
        {
            duringSymptomTriggers = new DialogueTrigger[DuringSymptomVoices.Length];
            for (int i = 0; i < DuringSymptomVoices.Length; i++)
            {
                duringSymptomTriggers[i] = CrearTriggerOculto(DuringSymptomVoices[i], $"DuringSymptom_{i}");
            }
        }

        // --- 1. CONFIGURAR VOLUMEN DE BLUR ---
        blurVolumeObject = new GameObject("SymptomVolume_Blur");
        blurVolumeObject.transform.SetParent(transform);
        blurVolumeObject.transform.localPosition = Vector3.zero;
        blurVolumeObject.layer = 0; 

        blurVolume = blurVolumeObject.AddComponent<Volume>();
        blurVolume.isGlobal = true;
        blurVolume.priority = 50; 
        blurVolume.weight = 0f; 

        VolumeProfile blurProfile = ScriptableObject.CreateInstance<VolumeProfile>();

        symptomBlur = blurProfile.Add<DualKawaseBlur>(true);
        symptomBlur.active = true;
        symptomBlur.BlurRadius.overrideState = true;
        symptomBlur.BlurRadius.value = MaxBlurIntensity;

        symptomVignette = blurProfile.Add<Vignette>(true);
        symptomVignette.active = true;
        symptomVignette.intensity.overrideState = true;
        symptomVignette.intensity.value = MaxTunnelIntensity;
        symptomVignette.color.overrideState = true;
        symptomVignette.color.value = Color.black;

        blurVolume.profile = blurProfile;


        // --- 2. CONFIGURAR VOLUMEN DE BLANCO Y NEGRO ---
        bwVolumeObject = new GameObject("SymptomVolume_BW");
        bwVolumeObject.transform.SetParent(transform);
        bwVolumeObject.transform.localPosition = Vector3.zero;
        bwVolumeObject.layer = 0; 

        bwVolume = bwVolumeObject.AddComponent<Volume>();
        bwVolume.isGlobal = true;
        bwVolume.priority = 49; 
        bwVolume.weight = 0f; 

        VolumeProfile bwProfile = ScriptableObject.CreateInstance<VolumeProfile>();

        symptomColorAdj = bwProfile.Add<ColorAdjustments>(true);
        symptomColorAdj.active = true;
        symptomColorAdj.saturation.overrideState = true;
        symptomColorAdj.saturation.value = MinSaturation;

        bwVolume.profile = bwProfile;


        // --- 3. CONFIGURAR VOLUMEN DE VHS Y GLITCH ---
        vhsVolumeObject = new GameObject("SymptomVolume_VHS_Glitch");
        vhsVolumeObject.transform.SetParent(transform);
        vhsVolumeObject.transform.localPosition = Vector3.zero;
        vhsVolumeObject.layer = 0; 

        vhsVolume = vhsVolumeObject.AddComponent<Volume>();
        vhsVolume.isGlobal = true;
        vhsVolume.priority = 51; 
        vhsVolume.weight = 0f; 

        VolumeProfile vhsProfile = ScriptableObject.CreateInstance<VolumeProfile>();

        symptomScanlines = vhsProfile.Add<Scanlines>(true);
        symptomScanlines.active = true;
        symptomScanlines.ScanlinesStrength.overrideState = true;
        symptomScanlines.ScanlinesStrength.value = MaxScanlinesStrength;
        symptomScanlines.GlitchIntensity.overrideState = true;
        symptomScanlines.GlitchIntensity.value = MaxGlitchIntensity;
        
        symptomScanlines.ScanlinesFrequency.overrideState = true;
        symptomScanlines.ScanlinesFrequency.value = 5f;
        symptomScanlines.GlitchFrequency.overrideState = true;
        symptomScanlines.GlitchFrequency.value = 1f;

        symptomChromaticAberration = vhsProfile.Add<ChromaticAberration>(true);
        symptomChromaticAberration.active = true;
        symptomChromaticAberration.intensity.overrideState = true;
        symptomChromaticAberration.intensity.value = MaxChromaticAberration;

        symptomLensDistortion = vhsProfile.Add<LensDistortion>(true);
        symptomLensDistortion.active = true;
        symptomLensDistortion.intensity.overrideState = true;
        symptomLensDistortion.intensity.value = MaxLensDistortion;
        symptomLensDistortion.scale.overrideState = true;
        symptomLensDistortion.scale.value = 1.07f; 

        vhsVolume.profile = vhsProfile;


        // --- 4. CONFIGURAR VOLUMEN DRUNK (Velocidad visual / ESTELA) ---
        drunkVolumeObject = new GameObject("SymptomVolume_Drunk_Visuals");
        drunkVolumeObject.transform.SetParent(transform);
        drunkVolumeObject.transform.localPosition = Vector3.zero;
        drunkVolumeObject.layer = 0; 

        drunkVolume = drunkVolumeObject.AddComponent<Volume>();
        drunkVolume.isGlobal = true;
        drunkVolume.priority = 48; 
        drunkVolume.weight = 0f; 

        VolumeProfile drunkProfile = ScriptableObject.CreateInstance<VolumeProfile>();

        symptomMotionBlur = drunkProfile.Add<MotionBlur>(true);
        symptomMotionBlur.active = true;
        symptomMotionBlur.intensity.overrideState = true;
        symptomMotionBlur.intensity.value = MaxMotionBlur;
        symptomMotionBlur.quality.overrideState = true;
        symptomMotionBlur.quality.value = MotionBlurQuality.High;

        drunkVolume.profile = drunkProfile;

        // --- 5. CONFIGURAR VOLUMEN DE LLUVIA (FILM GRAIN + BW) ---
        rainVolumeObject = new GameObject("SymptomVolume_Rain");
        rainVolumeObject.transform.SetParent(transform);
        rainVolumeObject.transform.localPosition = Vector3.zero;
        rainVolumeObject.layer = 0; 

        rainVolume = rainVolumeObject.AddComponent<Volume>();
        rainVolume.isGlobal = true;
        rainVolume.priority = 52; 
        rainVolume.weight = 0f; 

        VolumeProfile rainProfile = ScriptableObject.CreateInstance<VolumeProfile>();
        symptomGrain = rainProfile.Add<FilmGrain>(true);
        symptomGrain.active = true;
        symptomGrain.type.overrideState = true;
        symptomGrain.intensity.overrideState = true;

        symptomRainColorAdj = rainProfile.Add<ColorAdjustments>(true);
        symptomRainColorAdj.active = true;
        symptomRainColorAdj.saturation.overrideState = true;
        symptomRainColorAdj.saturation.value = -100f;

        symptomRainScanlines = rainProfile.Add<Scanlines>(true);
        symptomRainScanlines.active = true;
        symptomRainScanlines.ScanlinesStrength.overrideState = true;
        symptomRainScanlines.GlitchIntensity.overrideState = true;

        rainVolume.profile = rainProfile;

        // --- 6. CONFIGURAR SISTEMA DE PARTÍCULAS (RAYONES BLANCOS) ---
        GameObject psObj = new GameObject("Symptom_RainParticles");
        
        Camera mainCam = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
        if (mainCam != null) psObj.transform.SetParent(mainCam.transform);
        else psObj.transform.SetParent(transform);

        psObj.transform.localPosition = new Vector3(0, 0, 0.3f); // Muy cerca de la cámara
        psObj.transform.localRotation = Quaternion.identity;

        rainParticleSystem = psObj.AddComponent<ParticleSystem>();
        var main = rainParticleSystem.main;
        main.startLifetime = 0.1f;
        main.startSpeed = 0f;
        main.startSize3D = true; // Habilitar dimensiones separadas
        main.startSizeX = new ParticleSystem.MinMaxCurve(0.1f, 0.3f); // Más largas
        main.startSizeY = 0.001f; // Mucho más finas (hilo blanco)
        main.startColor = Color.white;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = 100;

        var emission = rainParticleSystem.emission;
        emission.enabled = false;
        emission.rateOverTime = 60;

        var shape = rainParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(1.2f, 0.8f, 0.1f);

        var renderer = psObj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        
        // Fix para el color rosa en URP: Asignar un material Unlit básico
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        renderer.material = new Material(shader);
        renderer.material.color = Color.white;
        
        rainParticleSystem.Stop();

/*
        if (EnableSymptoms && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "IntroHouse")
        {
            StartCoroutine(IntroBlurRoutine());
        }
*/
    }

    public void TriggerIntroSymptom()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "1 IntroHouse")
        {
            StartCoroutine(IntroBlurRoutine());
        }
    }

    private System.Collections.IEnumerator IntroBlurRoutine()
    {
        // Dale tiempo a la escena a quitar su pantalla de carga/negro inicial.
        /*
        if (IntroSymptomDelay > 0f)
        {
            yield return new WaitForSeconds(IntroSymptomDelay);
        }
        */

        currentActiveSymptom = SymptomType.Blur;
        // Forzamos un timeAlive muy alto para que el targetWeight sea 1 (intensidad máxima). 
        timeAlive = MinutesToMaxIntensity * 60f;
        
        // ¡Forzamos el volumen visual de inmediato! Así sonido e imagen arrancan violentamente al mismo tiempo.
        if (blurVolume != null) blurVolume.weight = 1f;

        if(EnableBlurAndTunnel )PlaySymptomSound(SymptomType.Blur);

        yield return new WaitForSeconds(1.07f);

        if (currentActiveSymptom == SymptomType.Blur)
        {
            RelieveSymptomsTemporarily(0f, false); // No reproducimos el sonido del suero aquí
            if (IntroDialogue != null) IntroDialogue.TriggerDialogue();
            // Quitamos el Stop() brusco aquí. El sonido se desvanecerá naturalmente junto a la visión en Update.
        }
    }

    private void ChooseRandomSymptom()
    {
        if (EnableSymptoms && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "1 IntroHouse")
        {
            System.Collections.Generic.List<SymptomType> available = new();
            if (EnableBlurAndTunnel) available.Add(SymptomType.Blur);
            if (EnableBlackAndWhite) available.Add(SymptomType.BlackAndWhite);
            if (EnableVHSGlitch) available.Add(SymptomType.VHS);
            if (EnableDrunkMotion) available.Add(SymptomType.Drunk);
            if (EnableWhispers) available.Add(SymptomType.Whispers);
            if (EnableRain) available.Add(SymptomType.Rain);

            if (available.Count > 0)
            {
                int index = Random.Range(0, available.Count);
                currentActiveSymptom = available[index];
                Debug.Log("[PlayerSymptom] Nuevo síntoma activado: " + currentActiveSymptom);

                PlaySymptomSound(currentActiveSymptom);
            }
            else
            {
                currentActiveSymptom = SymptomType.None;
            }
        }
       
    }

    private void PlaySymptomSound(SymptomType symptomType)
    {
        AudioClip[] soundArray = null;

        switch (symptomType)
        {
            case SymptomType.Blur:
                soundArray = BlurSounds;
                break;
            case SymptomType.BlackAndWhite:
                soundArray = BlackAndWhiteSounds;
                break;
            case SymptomType.VHS:
                soundArray = VHSSounds;
                break;
            case SymptomType.Drunk:
                soundArray = DrunkSounds;
                break;
            case SymptomType.Whispers:
                soundArray = WhispersSounds;
                break;
            case SymptomType.Rain:
                soundArray = RainSounds;
                break;
        }

        if (soundArray != null && soundArray.Length > 0)
        {
            AudioClip clip = soundArray[Random.Range(0, soundArray.Length)];
            if (clip != null)
            {
                symptomAudioSource.clip = clip;
                symptomAudioSource.Play();
            }
        }
    }

    void Update()
    {
        float targetWeight = 0f;
        // Actualización dinámica en tiempo real
        if (symptomBlur != null) symptomBlur.BlurRadius.value = MaxBlurIntensity;
        if (symptomVignette != null) symptomVignette.intensity.value = MaxTunnelIntensity;
        if (symptomColorAdj != null) symptomColorAdj.saturation.value = MinSaturation;

        if (symptomScanlines != null)
        {
            symptomScanlines.ScanlinesStrength.value = MaxScanlinesStrength;
            symptomScanlines.GlitchIntensity.value = MaxGlitchIntensity;
        }
        if (symptomChromaticAberration != null) symptomChromaticAberration.intensity.value = MaxChromaticAberration;
        if (symptomLensDistortion != null) symptomLensDistortion.intensity.value = MaxLensDistortion;
        if (symptomMotionBlur != null) symptomMotionBlur.intensity.value = MaxMotionBlur;

        if (symptomGrain != null)
        {
            symptomGrain.type.value = RainType;
            symptomGrain.intensity.value = MaxRainIntensity;
        }
        if (symptomRainColorAdj != null) symptomRainColorAdj.saturation.value = MinSaturation;

        if (symptomRainScanlines != null)
        {
            symptomRainScanlines.ScanlinesStrength.value = 0.2f; 
            symptomRainScanlines.GlitchIntensity.value = 0.1f;
        }


        targetWeight = 0f;

        if (!EnableSymptoms || (!EnableBlurAndTunnel && !EnableBlackAndWhite && !EnableVHSGlitch && !EnableDrunkMotion && !EnableWhispers && !EnableRain))
        {
            // RESET
            timer = GetRandomTimeBetweenSymptoms();
            timeAlive = 0f;
            currentActiveSymptom = SymptomType.None;
            targetWeight = 0f;
            StopVoiceRoutines();
        }
        else
        {
            if (currentActiveSymptom == SymptomType.None)
            {
                // Esperar a que pase el tiempo para el próximo síntoma aleatorio
                // Evitamos que el sistema aleatorio corra en la introhouse
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "1 IntroHouse")
                {
                    if (!isStartingSymptom)
                    {
                        timer -= Time.deltaTime;
                        if (timer <= 0f)
                        {
                            symptomRoutine = StartCoroutine(StartRandomSymptomRoutine());
                        }
                    }
                }
                targetWeight = 0f;
            }
            else
            {
                if (!hasTriggeredFirstSymptom)
                {
                    hasTriggeredFirstSymptom = true;
                    OnFirstSymptomActivated?.Invoke();
                }

                // Un síntoma en curso: Aumentar su intensidad si EnableProgression es true
                timeAlive += Time.deltaTime;
                float progressMultiplier = 1f;
                if (EnableProgression)
                {
                    float limitSeconds = MinutesToMaxIntensity * 60f;
                    progressMultiplier = Mathf.Clamp01(timeAlive / limitSeconds);
                }
                targetWeight = progressMultiplier;
            }
        }

        // Control del sistema de partículas de lluvia
        if (rainParticleSystem != null)
        {
            bool isRainActive = (currentActiveSymptom == SymptomType.Rain && targetWeight > 0.1f);
            var emission = rainParticleSystem.emission;
            
            if (isRainActive && !rainParticleSystem.isPlaying)
            {
                rainParticleSystem.Play();
                emission.enabled = true;
            }
            else if (!isRainActive && rainParticleSystem.isPlaying)
            {
                rainParticleSystem.Stop();
                emission.enabled = false;
            }

            if (isRainActive)
            {
                emission.rateOverTime = 60f * targetWeight;
                
                // Forzamos rotación horizontal (0 o 180) con una pequeña variación aleatoria
                var main = rainParticleSystem.main;
                float angle = (Random.value > 0.5f) ? 0f : 180f;
                main.startRotation = (angle + Random.Range(-5f, 5f)) * Mathf.Deg2Rad;
            }
        }

        // Aplicar pesos de manera independiente (permitimos que los desactivados bajen a 0 suavemente)
        float targetBlur = (currentActiveSymptom == SymptomType.Blur) ? targetWeight : 0f;
        if (blurVolume != null)
        {
            blurVolume.weight = Mathf.MoveTowards(blurVolume.weight, targetBlur, Time.deltaTime * BlurTransitionSpeed);
            if (!EnableBlurAndTunnel && blurVolume.weight > 0) blurVolume.weight = 0f;
        }

        float targetBW = (currentActiveSymptom == SymptomType.BlackAndWhite) ? targetWeight : 0f;
        if (bwVolume != null)
        {
            bwVolume.weight = Mathf.MoveTowards(bwVolume.weight, targetBW, Time.deltaTime * BlurTransitionSpeed);
            if (!EnableBlackAndWhite && bwVolume.weight > 0) bwVolume.weight = 0f;
        }

        float targetVHS = (currentActiveSymptom == SymptomType.VHS) ? targetWeight : 0f;
        if (vhsVolume != null)
        {
            vhsVolume.weight = Mathf.MoveTowards(vhsVolume.weight, targetVHS, Time.deltaTime * BlurTransitionSpeed);
            if (!EnableVHSGlitch && vhsVolume.weight > 0) vhsVolume.weight = 0f;
        }

        float targetDrunk = (currentActiveSymptom == SymptomType.Drunk) ? targetWeight : 0f;
        if (drunkVolume != null)
        {
            drunkVolume.weight = Mathf.MoveTowards(drunkVolume.weight, targetDrunk, Time.deltaTime * BlurTransitionSpeed);
            if (!EnableDrunkMotion && drunkVolume.weight > 0) drunkVolume.weight = 0f;
        }

        float targetRain = (currentActiveSymptom == SymptomType.Rain) ? targetWeight : 0f;
        if (rainVolume != null)
        {
            rainVolume.weight = Mathf.MoveTowards(rainVolume.weight, targetRain, Time.deltaTime * BlurTransitionSpeed);
            if (!EnableRain && rainVolume.weight > 0) rainVolume.weight = 0f;
        }

        float targetWhispers = (currentActiveSymptom == SymptomType.Whispers) ? targetWeight : 0f;
        whispersWeight = Mathf.MoveTowards(whispersWeight, targetWhispers, Time.deltaTime * BlurTransitionSpeed);
        if (!EnableWhispers && whispersWeight > 0) whispersWeight = 0f;

        float currentDrunkWeight = drunkVolume != null ? drunkVolume.weight : 0f;

        // --- GESTIÓN DE LA HABILIDAD DE CORRER Y CANSANCIO ---
        if (playerStateMachine != null)
        {
            bool hasSymptom = (currentActiveSymptom != SymptomType.None);

            if (EnableSymptoms)
            {
                // Si el síntoma está activo en general, controlamos el cansancio al correr
                bool isRunning = playerStateMachine.IsCurrent(PlayerStateMachine.RUN_STATE);

                if (isRunning)
                {
                    sprintDurationElapsed += Time.deltaTime;
                    if (sprintDurationElapsed >= MaxSprintDuration)
                    {
                        isFatigued = true;
                        fatigueCooldownTimer = 0f;
                        PlayFatigueSound();
                        
                        // Forzamos a dejar de correr
                        playerStateMachine.ChangeToIdle();
                    }
                }
                else
                {
                    if (isFatigued)
                    {
                        fatigueCooldownTimer += Time.deltaTime;
                        if (fatigueCooldownTimer >= FatigueRecoveryTime)
                        {
                            isFatigued = false;
                            sprintDurationElapsed = 0f;
                        }
                    }
                    else
                    {
                        // Recuperación gradual del cansancio cuando no está corriendo
                        sprintDurationElapsed = Mathf.Max(0f, sprintDurationElapsed - Time.deltaTime);
                    }
                }

                // El jugador puede correr si no tiene síntoma activo Y no está fatigado
                bool canRun = !hasSymptom && !isFatigued;
                playerStateMachine.SetStateEnabled(PlayerStateMachine.RUN_STATE, canRun);

                if (!canRun && playerStateMachine.IsCurrent(PlayerStateMachine.RUN_STATE))
                {
                    playerStateMachine.ChangeToIdle();
                }
            }
            else
            {
                // Comportamiento por defecto si el sistema de síntomas está desactivado
                isFatigued = false;
                sprintDurationElapsed = 0f;
                playerStateMachine.SetStateEnabled(PlayerStateMachine.RUN_STATE, !hasSymptom);

                if (hasSymptom && playerStateMachine.IsCurrent(PlayerStateMachine.RUN_STATE))
                {
                    playerStateMachine.ChangeToIdle();
                }
            }
        }

        // Físicas del movimiento de borracho: torpeza y forcejeos direccionales
        if (EnableDrunkMotion && playerStateMachine != null && currentDrunkWeight > 0f)
        {
            // Tambaleo natural que lo empuja suavemente en zigzag sin apretar teclas
            float passiveSideDrift = Mathf.Sin(Time.time * DrunkSwaySpeed * 0.7f) * MaxDrunkSidewaysForce;
            
            // Torpeza: "Me multiplico por dos cuando doy el paso al costado"
            // Leemos el movimiento horizontal (x) que intenta hacer el jugador
            Vector2 playerCurrentInput = playerStateMachine.Input;
            float stumbleInertia = playerCurrentInput.x * DrunkStumbleMultiplier;
            
            // Combinamos ambos para empujarlo forzosamente usando el AddForce.
            // Si camina a un costado, el stumbleInertia explotará esa inercia y lo arrastrará de más a los lados.
            playerStateMachine.AddForce(transform.right * (passiveSideDrift + stumbleInertia) * currentDrunkWeight, ForceMode.Force);
        }

        // --- SINCRONIA: El volumen del audio sigue el desvanecimiento visual ---
        if (symptomAudioSource != null)
        {
            float maxActiveWeight = 0f;
            if (blurVolume != null && blurVolume.weight > maxActiveWeight) maxActiveWeight = blurVolume.weight;
            if (bwVolume != null && bwVolume.weight > maxActiveWeight) maxActiveWeight = bwVolume.weight;
            if (vhsVolume != null && vhsVolume.weight > maxActiveWeight) maxActiveWeight = vhsVolume.weight;
            if (drunkVolume != null && drunkVolume.weight > maxActiveWeight) maxActiveWeight = drunkVolume.weight;
            if (rainVolume != null && rainVolume.weight > maxActiveWeight) maxActiveWeight = rainVolume.weight;
            if (whispersWeight > maxActiveWeight) maxActiveWeight = whispersWeight;

            // Curva cuadrática (al cuadrado) para que el audio desaparezca más rápido en la cola y sincronice mejor con lo visual
            symptomAudioSource.volume = Mathf.Pow(maxActiveWeight, 2f) * SymptomsAudioVolume;
            
            // Pausar completamente para no gastar recursos si no hay síntoma activo y su peso visual ya bajó a cero
            if (currentActiveSymptom == SymptomType.None && maxActiveWeight <= 0.001f && symptomAudioSource.isPlaying)
            {
                symptomAudioSource.Stop();
                symptomAudioSource.clip = null;
            }
        }
    }

    // Efecto visual: aplicamos tambaleo a la cámara al final del frame para que el LookController no lo sobreescriba.
    private void LateUpdate()
    {
        float currentDrunkWeight = drunkVolume != null ? drunkVolume.weight : 0f;
        if (EnableDrunkMotion && currentDrunkWeight > 0f && Camera.main != null)
        {
            float time = Time.time * DrunkSwaySpeed;
            // Movimientos matemáticos ondulantes para pitch, yaw y roll
            float pitch = Mathf.Sin(time) * MaxDrunkSwayAngle * currentDrunkWeight;
            float yaw = Mathf.Sin(time * 0.8f) * MaxDrunkSwayAngle * currentDrunkWeight;
            
            // Vértigo/Vueltas: Mezclamos el movimiento rápido lateral con una inclinación muy pesada y lenta
            float fastRoll = Mathf.Sin(time * 1.2f) * MaxDrunkSwayAngle;
            float deepSpin = Mathf.Sin(time * 0.3f) * MaxDrunkSpinAngle; 
            float finalRoll = (fastRoll + deepSpin) * currentDrunkWeight;

            Camera.main.transform.localRotation *= Quaternion.Euler(pitch, yaw, finalRoll);
        }
    }

    /// <summary>
    /// Cura los síntomas permanentemente (Apaga el sistema por completo).
    /// </summary>
    public void CureSymptomsFully()
    {
        StopVoiceRoutines();
        EnableSymptoms = false;
        currentActiveSymptom = SymptomType.None;
        timeAlive = 0f;
    }

    /// <summary>
    /// Alivia temporalmente los síntomas.
    /// extraWaitTime: tiempo adicional que se suma al TimeBetweenSymptoms base.
    /// </summary>
    public void RelieveSymptomsTemporarily(float extraWaitTime = 0f, bool playCureSound = true)
    {
        StopVoiceRoutines();
        currentActiveSymptom = SymptomType.None;
        timeAlive = 0f;
        
        // Sumamos el tiempo base más el extra que venga del objeto curativo
        timer = Mathf.Max(GetRandomTimeBetweenSymptoms() + extraWaitTime, 1f); 
        isStartingSymptom = false; // Reset por si estaba en proceso de aviso

        if (playCureSound && (CureSound != null || CureSoundSecondary != null))
        {
            StartCoroutine(PlayCureSoundsRoutine());
        }

        Debug.Log($"[PlayerSymptom] Síntomas curados. Esperando {timer} segundos para el próximo síntoma.");
    }

    private System.Collections.IEnumerator PlayCureSoundsRoutine()
    {
        float delayForSecond = 0f;

        if (CureSound != null)
        {
            GameObject tempAudioObj = new GameObject("CureSoundTemp1");
            AudioSource tempSource = tempAudioObj.AddComponent<AudioSource>();
            tempSource.spatialBlend = 0f; // Sonido 2D
            tempSource.volume = CureSoundVolume;
            tempSource.clip = CureSound;
            tempSource.Play();
            Destroy(tempAudioObj, CureSound.length + 0.1f);

            delayForSecond = CureSound.length;
        }

        if (delayForSecond > 0f)
        {
            yield return new WaitForSeconds(delayForSecond);
        }

        if (CureSoundSecondary != null)
        {
            GameObject tempAudioObj2 = new GameObject("CureSoundTemp2");
            AudioSource tempSource2 = tempAudioObj2.AddComponent<AudioSource>();
            tempSource2.spatialBlend = 0f; // Sonido 2D
            tempSource2.volume = CureSoundVolume;
            tempSource2.clip = CureSoundSecondary;
            tempSource2.Play();
            Destroy(tempAudioObj2, CureSoundSecondary.length + 0.1f);
        }
    }

    private System.Collections.IEnumerator StartRandomSymptomRoutine()
    {
        isStartingSymptom = true;

        // --- 1. AUDIO PRE-SÍNTOMA ---
        if (PreSymptomVoices != null && PreSymptomVoices.Length > 0 && Random.value <= PreSymptomVoiceChance)
        {
            AudioClip clip = PreSymptomVoices[Random.Range(0, PreSymptomVoices.Length)];
            if (clip != null)
            {
                voiceAudioSource.clip = clip;
                voiceAudioSource.Play();
                yield return new WaitForSeconds(clip.length);
            }
        }

        // --- 2. ELEGIR SÍNTOMA REAL ---
        List<SymptomType> availableSymptoms = new List<SymptomType>();
        if (EnableBlurAndTunnel) availableSymptoms.Add(SymptomType.Blur);
        if (EnableBlackAndWhite) availableSymptoms.Add(SymptomType.BlackAndWhite);
        if (EnableVHSGlitch) availableSymptoms.Add(SymptomType.VHS);
        if (EnableDrunkMotion) availableSymptoms.Add(SymptomType.Drunk);
        if (EnableWhispers) availableSymptoms.Add(SymptomType.Whispers);
        // Descomenta si quieres que la lluvia también sea un síntoma de larga duración:
        // if (EnableRain) availableSymptoms.Add(SymptomType.Rain);

        if (availableSymptoms.Count > 0)
        {
            currentActiveSymptom = availableSymptoms[Random.Range(0, availableSymptoms.Count)];
            timeAlive = 0f;
            PlaySymptomSound(currentActiveSymptom);
        }

        isStartingSymptom = false;

        // --- 4. AUDIO DURANTE EL SÍNTOMA ---
        if (currentActiveSymptom != SymptomType.None && DuringSymptomVoices != null && DuringSymptomVoices.Length > 0 && Random.value <= DuringSymptomVoiceChance)
        {
            float delay = Random.Range(MinDuringSymptomDelay, MaxDuringSymptomDelay);
            duringVoiceRoutine = StartCoroutine(PlayDuringSymptomVoiceRoutine(delay));
        }

        symptomRoutine = null;
    }

    private DialogueTrigger CrearTriggerOculto(UHFPS.Scriptable.DialogueAsset asset, string nombre)
    {
        if (asset == null) return null;

        GameObject go = new GameObject($"HiddenDialogue_{nombre}");
        go.transform.SetParent(transform);
        
        DialogueTrigger dt = go.AddComponent<DialogueTrigger>();
        dt.Dialogue = asset;
        dt.DialogueAudio = voiceAudioSource;
        dt.DialogueType = DialogueTrigger.DialogueTypeEnum.Local;
        dt.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;
        
        return dt;
    }

    private System.Collections.IEnumerator PlayDuringSymptomVoiceRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentActiveSymptom != SymptomType.None) // Por si se curó mientras esperaba
        {
            if (duringSymptomTriggers != null && duringSymptomTriggers.Length > 0)
            {
                DialogueTrigger dt = duringSymptomTriggers[Random.Range(0, duringSymptomTriggers.Length)];
                if (dt != null)
                {
                    if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsPlaying)
                    {
                        DialogueSystem.Instance.StopDialogue();
                    }
                    dt.TriggerDialogue();
                }
            }
        }
    }

    private void StopVoiceRoutines()
    {
        if (symptomRoutine != null) StopCoroutine(symptomRoutine);
        if (duringVoiceRoutine != null) StopCoroutine(duringVoiceRoutine);
        isStartingSymptom = false;
        
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsPlaying)
        {
            DialogueSystem.Instance.StopDialogue();
        }
        else if (voiceAudioSource != null && voiceAudioSource.isPlaying)
        {
            voiceAudioSource.Stop();
        }
    }

    private void PlayFatigueSound()
    {
        if (FatigueSound != null && voiceAudioSource != null)
        {
            voiceAudioSource.PlayOneShot(FatigueSound, FatigueVolume);
        }
    }

    private float GetRandomTimeBetweenSymptoms()
    {
        if (UseRandomTimeRange)
        {
            return Random.Range(MinMinutesBetweenSymptoms, MaxMinutesBetweenSymptoms) * 60f;
        }
        return TimeBetweenSymptoms;
    }

    void OnDestroy()
    {
        if (voiceAudioSource != null) Destroy(voiceAudioSource.gameObject);
        if (symptomAudioSource != null) Destroy(symptomAudioSource.gameObject);
        if (blurVolumeObject != null) Destroy(blurVolumeObject);
        if (bwVolumeObject != null) Destroy(bwVolumeObject);
        if (vhsVolumeObject != null) Destroy(vhsVolumeObject);
        if (drunkVolumeObject != null) Destroy(drunkVolumeObject);
        if (rainVolumeObject != null) Destroy(rainVolumeObject);
    }
}
