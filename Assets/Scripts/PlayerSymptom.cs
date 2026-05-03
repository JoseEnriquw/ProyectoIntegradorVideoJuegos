using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UHFPS.Runtime;
using UHFPS.Rendering; // For DualKawaseBlur, Scanlines

public class PlayerSymptom : MonoBehaviour
{
    public enum SymptomType { None, Blur, BlackAndWhite, VHS, Drunk }
    [Header("General Settings")]
    public bool EnableSymptoms = true;

    [Header("Timing Settings")]
    [Tooltip("Time in seconds before a new random symptom appears (e.g., 180 para 3 minutos).")]
    public float TimeBetweenSymptoms = 180f;

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
    
    // Restauramos el nombre original de la variable para recuperar el ajuste de velocidad
    public float BlurTransitionSpeed = 1f;

    [Header("Symptom Sounds")]
    public AudioClip[] BlurSounds;
    public AudioClip[] BlackAndWhiteSounds;
    public AudioClip[] VHSSounds;
    public AudioClip[] DrunkSounds;
    [Range(0f, 1f)]
    public float SymptomsAudioVolume = 0.8f;

    [Header("Intro Sequence")]
    [Tooltip("Tiempo que espera al iniciar la escena para lanzar el síntoma (útil para saltar pantallas negras de carga)")]
    public float IntroSymptomDelay = 2f;
    public DialogueTrigger IntroDialogue;

    private float timer;
    private float timeAlive = 0f;
    private SymptomType currentActiveSymptom = SymptomType.None;
    private AudioSource symptomAudioSource;

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

    private PlayerStateMachine playerStateMachine;

    public static PlayerSymptom Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        timer = TimeBetweenSymptoms;
        playerStateMachine = GetComponent<PlayerStateMachine>();

        // --- 0. CONFIGURAR FUENTE DE AUDIO ---
        GameObject audioObj = new GameObject("SymptomAudioSource");
        audioObj.transform.SetParent(transform);
        audioObj.transform.localPosition = Vector3.zero;
        symptomAudioSource = audioObj.AddComponent<AudioSource>();
        symptomAudioSource.spatialBlend = 0f; // Sonido 2D (en la cabeza del jugador)
        symptomAudioSource.volume = SymptomsAudioVolume;
        symptomAudioSource.playOnAwake = false;

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

/*
        if (EnableSymptoms && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "IntroHouse")
        {
            StartCoroutine(IntroBlurRoutine());
        }
*/
    }

    public void TriggerIntroSymptom()
    {
        StartCoroutine(IntroBlurRoutine());
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

        if (BlurSounds != null && BlurSounds.Length > 0)
        {
            AudioClip clip = BlurSounds[Random.Range(0, BlurSounds.Length)];
            if (clip != null)
            {
                symptomAudioSource.clip = clip;
                symptomAudioSource.Play();
            }
        }

        yield return new WaitForSeconds(1.07f);

        if (currentActiveSymptom == SymptomType.Blur)
        {
            RelieveSymptomsTemporarily();
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

            if (available.Count > 0)
            {
                int index = Random.Range(0, available.Count);
                currentActiveSymptom = available[index];
                Debug.Log("[PlayerSymptom] Nuevo síntoma activado: " + currentActiveSymptom);
            }
            else
            {
                currentActiveSymptom = SymptomType.None;
            }
        }
       
    }

    void Update()
    {
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

        float targetWeight = 0f;

        if (!EnableSymptoms || (!EnableBlurAndTunnel && !EnableBlackAndWhite && !EnableVHSGlitch && !EnableDrunkMotion))
        {
            // RESET
            timer = TimeBetweenSymptoms;
            timeAlive = 0f;
            currentActiveSymptom = SymptomType.None;
            targetWeight = 0f;
        }
        else
        {
            if (currentActiveSymptom == SymptomType.None)
            {
                // Esperar a que pase el tiempo para el próximo síntoma aleatorio
                // Evitamos que el sistema aleatorio corra en la introhouse
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "1 IntroHouse")
                {
                    timer -= Time.deltaTime;
                    if (timer <= 0f)
                    {
                        ChooseRandomSymptom();
                        timeAlive = 0f; // Reiniciar tiempo activo para el nuevo síntoma
                    }
                }
                targetWeight = 0f;
            }
            else
            {
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

        float currentDrunkWeight = drunkVolume != null ? drunkVolume.weight : 0f;

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

            symptomAudioSource.volume = maxActiveWeight * SymptomsAudioVolume;
            
            // Pausar completamente para no gastar recursos si no hay síntoma visual
            if (maxActiveWeight <= 0.001f && symptomAudioSource.isPlaying)
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
        EnableSymptoms = false;
        currentActiveSymptom = SymptomType.None;
        timeAlive = 0f;
    }

    /// <summary>
    /// Alivia temporalmente los síntomas.
    /// Resetea el tiempo para el próximo síntoma.
    /// </summary>
    public void RelieveSymptomsTemporarily()
    {
        currentActiveSymptom = SymptomType.None;
        timeAlive = 0f;
        timer = Mathf.Max(TimeBetweenSymptoms, 1f); // Aseguramos usar al menos 1 segundo en caso de despiste
        Debug.Log($"[PlayerSymptom] Síntomas curados. Esperando {timer} segundos para el próximo!");
    }

    void OnDestroy()
    {
        if (symptomAudioSource != null) Destroy(symptomAudioSource.gameObject);
        if (blurVolumeObject != null) Destroy(blurVolumeObject);
        if (bwVolumeObject != null) Destroy(bwVolumeObject);
        if (vhsVolumeObject != null) Destroy(vhsVolumeObject);
        if (drunkVolumeObject != null) Destroy(drunkVolumeObject);
    }
}
