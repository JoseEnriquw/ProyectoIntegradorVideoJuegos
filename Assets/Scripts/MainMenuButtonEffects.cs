using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MainMenuButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private TMP_Text buttonText;
    private Button button;
    private AudioSource localAudioSource;
    
    [Header("Visual Settings")]
    public Color hoverColor = new Color(0.7f, 0.05f, 0.05f); // Crimson red
    private Color normalColor = Color.white;
    public float hoverScale = 1.08f;
    public float scaleSpeed = 6f;
    
    [Header("Audio Settings")]
    public AudioClip hoverClip;
    public AudioClip clickClip;
    public AudioClip whisperClip;
    
    [Header("3D Light References")]
    public Light flashlightSpot;    // Projector light down the road (New Game hover)
    public Light playerLantern;     // Lantern light in foreground (Quit hover)
    
    private Vector3 targetScale = Vector3.one;
    private bool isHovered = false;
    private Coroutine lightCoroutine;
    
    private float initialLanternIntensity = 5.5f;
    
    void Start()
    {
        buttonText = GetComponentInChildren<TMP_Text>();
        button = GetComponent<Button>();
        
        // Ensure AudioSource exists
        localAudioSource = GetComponent<AudioSource>();
        if (localAudioSource == null)
        {
            localAudioSource = gameObject.AddComponent<AudioSource>();
        }
        localAudioSource.playOnAwake = false;
        localAudioSource.spatialBlend = 0f; // 2D Sound
        localAudioSource.loop = false;
        
        if (buttonText != null)
        {
            normalColor = buttonText.color;
        }
        
        if (playerLantern != null)
        {
            initialLanternIntensity = playerLantern.intensity;
        }
        
        // Hide flashlight by default
        if (flashlightSpot != null)
        {
            flashlightSpot.enabled = false;
        }
    }
    
    void Update()
    {
        // Smoothly interpolate scale
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        
        isHovered = true;
        targetScale = new Vector3(hoverScale, hoverScale, hoverScale);
        
        if (buttonText != null)
        {
            buttonText.color = hoverColor;
        }
        
        // Play hover SFX
        if (hoverClip != null && localAudioSource != null)
        {
            localAudioSource.PlayOneShot(hoverClip, 0.45f);
        }
        
        // AAA Interaction: Hover effects for specific buttons
        if (name == "NewGame" && flashlightSpot != null)
        {
            flashlightSpot.enabled = true;
        }
        else if (name == "Quit" && playerLantern != null)
        {
            if (lightCoroutine != null) StopCoroutine(lightCoroutine);
            lightCoroutine = StartCoroutine(FlickerLanternOut(true));
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        targetScale = Vector3.one;
        
        if (buttonText != null)
        {
            buttonText.color = normalColor;
        }
        
        // Disable New Game flashlight
        if (name == "NewGame" && flashlightSpot != null)
        {
            flashlightSpot.enabled = false;
        }
        // Restore Quit lantern
        else if (name == "Quit" && playerLantern != null)
        {
            if (lightCoroutine != null) StopCoroutine(lightCoroutine);
            lightCoroutine = StartCoroutine(FlickerLanternOut(false));
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (button != null && !button.interactable) return;
        
        if (clickClip != null && localAudioSource != null)
        {
            localAudioSource.PlayOneShot(clickClip, 1.0f);
        }
    }
    
    IEnumerator FlickerLanternOut(bool turnOff)
    {
        if (turnOff)
        {
            // Play whisper sound
            if (whisperClip != null && localAudioSource != null)
            {
                localAudioSource.PlayOneShot(whisperClip, 0.7f);
            }
            
            // Rapidly flicker the lantern down to zero
            int steps = 6;
            for (int i = 0; i < steps; i++)
            {
                playerLantern.intensity = Random.Range(0.1f, 1f) * initialLanternIntensity * (1f - (float)i / steps);
                yield return new WaitForSecondsRealtime(Random.Range(0.04f, 0.1f));
            }
            playerLantern.enabled = false;
        }
        else
        {
            // Restore lantern flickering back up
            playerLantern.enabled = true;
            int steps = 5;
            for (int i = 0; i < steps; i++)
            {
                playerLantern.intensity = initialLanternIntensity * ((float)i / steps);
                yield return new WaitForSecondsRealtime(Random.Range(0.03f, 0.08f));
            }
            playerLantern.intensity = initialLanternIntensity;
        }
    }
}
