using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UHFPS.Runtime;
using UHFPS.Tools;

public class SurvivalTimer : MonoBehaviour
{
    public static SurvivalTimer Instance { get; private set; }
    [Header("Timer Settings")]
    [Tooltip("Initial hours for the countdown.")]
    public float StartingHours = 0f;
    [Tooltip("Initial minutes for the countdown.")]
    public float StartingMinutes = 2f;
    
    [Header("UI References")]
    public Slider TimerSlider;
    public TMP_Text TimerText;
    public Image HeartbeatImage;

    [Header("Heartbeat Settings")]
    [Tooltip("How fast the heartbeat is when time is at zero.")]
    public float LowTimePulse = 5f;
    [Tooltip("How fast the heartbeat is when time is full.")]
    public float NormalPulse = 1f;

    [Header("Low Time Audio Settings")]
    [Tooltip("Audio clip to play when the remaining time is low.")]
    public AudioClip LowTimeAudio;
    [Tooltip("Maximum volume for the low time audio at 0 seconds.")]
    [Range(0f, 1f)]
    public float LowTimeAudioVolume = 0.8f;
    [Tooltip("Time in minutes remaining when the low time audio starts playing.")]
    public float WarningTimeMinutes = 10f;
    
    [Header("Status")]
    public bool TimerRunning = true;
    
    private float timeRemaining;
    private float maxTime;
    private float lastLogTime;
    private bool playerIsDead = false;
    private Material heartbeatMat;
    private AudioSource lowTimeAudioSource;

    /// <summary>
    /// Returns the current remaining time in seconds.
    /// </summary>
    public void start() { 
        QualitySettings.vSyncCount = 0; // Disable VSync for more accurate timing (optional)
       Application.targetFrameRate = -1; // Set a target frame rate (optional)

    }
    public float TimeRemaining => timeRemaining;

    /// <summary>
    /// Returns the remaining time formatted as HH:MM:SS.
    /// </summary>
    public string TimeFormatted
    {
        get
        {
            int hours = Mathf.FloorToInt(timeRemaining / 3600);
            int minutes = Mathf.FloorToInt((timeRemaining % 3600) / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            transform.SetParent(null); // Importante: DontDestroyOnLoad solo funciona en objetos raíz
            DontDestroyOnLoad(gameObject);

            // Initialize timer values on Awake only for the first instance
            maxTime = (StartingHours * 3600f) + (StartingMinutes * 60f);
            timeRemaining = maxTime;
            lastLogTime = timeRemaining;
        }
        else
        {
            // Ya existe un timer de la escena anterior.
            // Le pasamos las nuevas referencias de UI de la nueva escena antes de destruirnos.
            Instance.TimerSlider = this.TimerSlider;
            Instance.TimerText = this.TimerText;
            Instance.HeartbeatImage = this.HeartbeatImage;
            Instance.RebindUI();

            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (Instance == this)
        {
            Debug.Log($"[SurvivalTimer] Timer started with {TimeFormatted} ({timeRemaining} seconds)");
            
            // Setup dynamic AudioSource for low time warning
            GameObject audioObj = new GameObject("LowTimeAudioSource");
            audioObj.transform.SetParent(transform);
            audioObj.transform.localPosition = Vector3.zero;
            lowTimeAudioSource = audioObj.AddComponent<AudioSource>();
            lowTimeAudioSource.spatialBlend = 0f; // 2D Sound
            lowTimeAudioSource.volume = 0f; // Starts muted
            lowTimeAudioSource.clip = LowTimeAudio;
            lowTimeAudioSource.loop = true;
            lowTimeAudioSource.playOnAwake = false;

            RebindUI();
        }
    }

    public void RebindUI()
    {
        // Re-vinculamos la UI con la de la nueva escena
        if (TimerSlider != null)
        {
            TimerSlider.maxValue = 1f;
        }

        if (HeartbeatImage != null)
        {
            heartbeatMat = HeartbeatImage.material;
        }
        
        UpdateUI(); // Forzamos una actualización inmediata para que la UI no se vea vacía
    }

    void Update()
    {
        // Update low time audio always to allow smooth fade out/ins under all conditions (e.g. death, pause, time changes)
        UpdateLowTimeAudio();

        if (!TimerRunning || playerIsDead) return;

        // Check if game is paused through UHFPS GameManager
        if (GameManager.HasReference && GameManager.Instance.IsPaused) return;

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            
            // Update UI
            UpdateUI();

            // Console Debug every 10 seconds
            if (lastLogTime - timeRemaining >= 10f)
            {
                Debug.Log($"[SurvivalTimer] Time remaining: {TimeFormatted}");
                lastLogTime = timeRemaining;
            }

            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                UpdateUI();
                OnTimerEnd();
            }
        }
    }

    private void UpdateUI()
    {
        float percent = timeRemaining / maxTime;

        if (TimerSlider != null)
        {
            // Smooth update (optional, matching health bar style)
            TimerSlider.value = Mathf.Lerp(TimerSlider.value, percent, Time.deltaTime * 5f);
        }

        if (TimerText != null)
        {
            TimerText.text = TimeFormatted;
        }

        if (HeartbeatImage != null && heartbeatMat != null)
        {
            // Map percentage to pulse speed (remap 0..1 to lowTimePulse..normalPulse)
            float pulse = GameTools.Remap(0f, 1f, LowTimePulse, NormalPulse, percent);
            heartbeatMat.SetFloat("_PulseMultiplier", pulse);

            // Handle extinction keyword
            if (timeRemaining <= 0)
            {
                heartbeatMat.EnableKeyword("ZERO_PULSE");
            }
            else
            {
                heartbeatMat.DisableKeyword("ZERO_PULSE");
            }
        }
    }

    private void OnTimerEnd()
    {
        Debug.Log("<color=red>[SurvivalTimer] Time is up! Triggering player death.</color>");
        
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.ApplyDamageMax();
            playerIsDead = true;
        }
        else
        {
            Debug.LogError("[SurvivalTimer] Could not find PlayerHealth component in the scene!");
        }
    }

    /// <summary>
    /// Call this to add or subtract time (in seconds).
    /// </summary>
    public void AddTime(float seconds)
    {
        timeRemaining += seconds;
        if (timeRemaining < 0) timeRemaining = 0;
        if (timeRemaining > maxTime) maxTime = timeRemaining; // Expand max if time added beyond start
        lastLogTime = timeRemaining; // Reset log sync
    }

    private void UpdateLowTimeAudio()
    {
        if (lowTimeAudioSource == null || LowTimeAudio == null) return;

        // Check pause state
        bool isPaused = GameManager.HasReference && GameManager.Instance.IsPaused;

        float thresholdSeconds = WarningTimeMinutes * 60f;
        float targetVolume = 0f;

        if (timeRemaining <= thresholdSeconds && timeRemaining > 0 && TimerRunning && !playerIsDead && !isPaused)
        {
            float progress = 1f - (timeRemaining / thresholdSeconds); // 0 at threshold, 1 at 0 seconds
            targetVolume = progress * LowTimeAudioVolume;

            if (!lowTimeAudioSource.isPlaying)
            {
                lowTimeAudioSource.clip = LowTimeAudio;
                lowTimeAudioSource.Play();
            }
        }

        // Smoothly transition volume to avoid sudden changes (fade in/out)
        lowTimeAudioSource.volume = Mathf.MoveTowards(lowTimeAudioSource.volume, targetVolume, Time.deltaTime * 0.5f);

        // Manage pause/resume states
        if (isPaused && lowTimeAudioSource.isPlaying)
        {
            lowTimeAudioSource.Pause();
        }
        else if (!isPaused && !lowTimeAudioSource.isPlaying && targetVolume > 0f)
        {
            lowTimeAudioSource.UnPause();
        }

        // Stop playing to free resources when faded out
        if (targetVolume <= 0f && lowTimeAudioSource.volume <= 0.001f && lowTimeAudioSource.isPlaying)
        {
            lowTimeAudioSource.Stop();
        }
    }
}
