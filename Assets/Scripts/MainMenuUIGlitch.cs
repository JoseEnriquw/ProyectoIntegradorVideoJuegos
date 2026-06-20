using UnityEngine;
using TMPro;

public class MainMenuUIGlitch : MonoBehaviour
{
    private TMP_Text textComponent;
    private Vector3 originalPosition;
    private Vector3 originalRotation;
    private Color originalColor;
    
    private Light thunderLight;
    private MainMenuHorrorEvents horrorEvents;

    [Header("Glitch Force")]
    public float maxPositionOffset = 5f;
    public float maxRotationOffset = 1.5f;
    [Range(0f, 1f)]
    public float glitchChance = 0.35f;

    void Start()
    {
        textComponent = GetComponent<TMP_Text>();
        originalPosition = transform.localPosition;
        originalRotation = transform.localEulerAngles;
        if (textComponent != null)
        {
            originalColor = textComponent.color;
        }

        // Find ThunderManager
        var thunderMgr = Object.FindAnyObjectByType<AdvancedHorrorFPS.ThunderManager>();
        if (thunderMgr != null)
        {
            thunderLight = thunderMgr.GetComponent<Light>();
        }

        // Find MainMenuHorrorEvents
        horrorEvents = Object.FindAnyObjectByType<MainMenuHorrorEvents>();
    }

    void Update()
    {
        if (textComponent == null) return;

        bool isThunderActive = (thunderLight != null && thunderLight.enabled && thunderLight.intensity > 0.1f);
        bool isHorrorEventActive = (horrorEvents != null && horrorEvents.IsEventRunning);

        if ((isThunderActive || isHorrorEventActive) && Random.value < glitchChance)
        {
            // Position glitch
            float offsetX = Random.Range(-maxPositionOffset, maxPositionOffset);
            float offsetY = Random.Range(-maxPositionOffset, maxPositionOffset);
            transform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            // Rotation Z glitch
            float offsetRot = Random.Range(-maxRotationOffset, maxRotationOffset);
            transform.localEulerAngles = originalRotation + new Vector3(0f, 0f, offsetRot);

            // Color glitch: flicker text color between dim gray, bright white, and original color
            float colorRand = Random.value;
            if (colorRand < 0.3f)
            {
                textComponent.color = new Color(0.35f, 0.35f, 0.35f, originalColor.a);
            }
            else if (colorRand < 0.6f)
            {
                textComponent.color = new Color(0.95f, 0.95f, 0.95f, originalColor.a);
            }
            else
            {
                textComponent.color = originalColor;
            }
            
            // Random character spacing glitch
            textComponent.characterSpacing = Random.Range(0.5f, 3.5f);
        }
        else
        {
            // Restore original values
            if (transform.localPosition != originalPosition)
            {
                transform.localPosition = originalPosition;
                transform.localEulerAngles = originalRotation;
                textComponent.color = originalColor;
                textComponent.characterSpacing = 1.5f; // Default spacing
            }
        }
    }
}
