using UnityEngine;

public class MainMenuCrowReaction : MonoBehaviour
{
    public Animator crowAnimator;
    public AudioSource crowAudio;
    
    private Light thunderLight;
    private bool wasThunderActive = false;
    private float lastReactionTime = 0f;
    private const float REACTION_COOLDOWN = 6f;

    void Start()
    {
        if (crowAnimator == null)
        {
            crowAnimator = GetComponentInChildren<Animator>();
        }
        
        // Find 3D_CrowCall in parent or siblings
        if (crowAudio == null)
        {
            crowAudio = GameObject.Find("Crow_Sign/3D_CrowCall")?.GetComponent<AudioSource>();
        }
        
        // Find ThunderManager light
        var thunderMgr = Object.FindAnyObjectByType<AdvancedHorrorFPS.ThunderManager>();
        if (thunderMgr != null)
        {
            thunderLight = thunderMgr.GetComponent<Light>();
        }
    }

    void Update()
    {
        if (thunderLight == null) return;

        bool isThunderActive = (thunderLight.enabled && thunderLight.intensity > 0.1f);

        // Detect lightning start trigger
        if (isThunderActive && !wasThunderActive)
        {
            if (Time.time > lastReactionTime + REACTION_COOLDOWN)
            {
                ReactToLightning();
            }
        }

        wasThunderActive = isThunderActive;
    }

    public void ReactToHover()
    {
        // 35% chance of screaming and fluttering when hovered
        if (Time.time > lastReactionTime + 3f && Random.value < 0.35f)
        {
            TriggerScare(pitchMin: 0.95f, pitchMax: 1.15f);
        }
    }

    public void ReactToLightning()
    {
        // 75% chance of screaming when struck by lightning
        if (Random.value < 0.75f)
        {
            TriggerScare(pitchMin: 0.8f, pitchMax: 1.0f);
        }
    }

    private void TriggerScare(float pitchMin, float pitchMax)
    {
        lastReactionTime = Time.time;

        if (crowAudio != null && !crowAudio.isPlaying)
        {
            crowAudio.pitch = Random.Range(pitchMin, pitchMax);
            crowAudio.Play();
        }

        if (crowAnimator != null)
        {
            // Play wing-flapping takeoff animation from beginning
            crowAnimator.Play("TakeOff", 0, 0f);
        }
    }
}
