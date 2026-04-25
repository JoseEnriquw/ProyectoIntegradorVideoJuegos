using UnityEngine;
using TMPro;
using System.Collections;
using UHFPS.Tools;
using UHFPS.Runtime;

public class SurvivalTimerAnnouncement : MonoBehaviour
{
    public static SurvivalTimerAnnouncement Instance { get; private set; }

    [Header("UI References")]
    public CanvasGroup AnnouncementGroup;
    public TMP_Text MessageText;
    [Header("Settings")]
    public GString MessageGloc;
    public float DisplayDuration = 5f;
    public float FadeSpeed = 2f;

    private string localizedTemplate;
    private bool isLocalized = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (AnnouncementGroup != null)
        {
            AnnouncementGroup.alpha = 0f;
            AnnouncementGroup.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        // Subscribe to localization system to get the template message
        MessageGloc.SubscribeGloc(text => 
        {
            localizedTemplate = text;
            isLocalized = true;
        });
    }

    /// <summary>
    /// Triggers the large announcement on screen.
    /// </summary>
    public void Show()
    {
        if (AnnouncementGroup == null || MessageText == null)
        {
            Debug.LogWarning("[SurvivalTimerAnnouncement] Missing UI references!");
            return;
        }
        
        StopAllCoroutines();
        StartCoroutine(AnnouncementRoutine());
    }

    private IEnumerator AnnouncementRoutine()
    {
        // Use the localized template if available, otherwise fallback to GString value
        string template = isLocalized ? localizedTemplate : MessageGloc.Value;
        
        if (string.IsNullOrEmpty(template))
        {
            template = "Tienes {0} hasta que la enfermedad te mate..."; // Emergency fallback
        }

        // Update text with current time from SurvivalTimer
        if (SurvivalTimer.Instance != null)
        {
            try 
            {
                MessageText.text = string.Format(template, SurvivalTimer.Instance.TimeFormatted);
            }
            catch (System.FormatException)
            {
                Debug.LogError($"[SurvivalTimerAnnouncement] The localization string '{template}' does not contain '{{0}}' for formatting!");
                MessageText.text = template;
            }
        }
        else
        {
            MessageText.text = template;
        }

        // Fade In
        yield return CanvasGroupFader.StartFade(AnnouncementGroup, true, FadeSpeed);

        // Wait
        yield return new WaitForSeconds(DisplayDuration);

        // Fade Out
        yield return CanvasGroupFader.StartFade(AnnouncementGroup, false, FadeSpeed, () =>
        {
            AnnouncementGroup.gameObject.SetActive(false);
        });
    }
}
