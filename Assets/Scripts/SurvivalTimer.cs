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
    
    [Header("Status")]
    public bool TimerRunning = true;
    
    private float timeRemaining;
    private float maxTime;
    private float lastLogTime;
    private bool playerIsDead = false;
    private Material heartbeatMat;

    /// <summary>
    /// Returns the current remaining time in seconds.
    /// </summary>
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize timer values on Awake only for the first instance
        maxTime = (StartingHours * 3600f) + (StartingMinutes * 60f);
        timeRemaining = maxTime;
        lastLogTime = timeRemaining;
    }

    void Start()
    {
        Debug.Log($"[SurvivalTimer] Timer started with {TimeFormatted} ({timeRemaining} seconds)");
        
        // Initialize UI
        if (TimerSlider != null)
        {
            TimerSlider.maxValue = 1f;
            TimerSlider.value = 1f;
        }

        if (HeartbeatImage != null)
        {
            heartbeatMat = HeartbeatImage.material;
        }
    }

    void Update()
    {
        if (!TimerRunning || playerIsDead) return;

        // Check if game is paused through UHFPS GameManager
        if (GameManager.Instance != null && GameManager.Instance.IsPaused) return;

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
}
