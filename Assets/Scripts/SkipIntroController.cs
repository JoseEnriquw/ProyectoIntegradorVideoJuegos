using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;
using UHFPS.Runtime;

public class SkipIntroController : MonoBehaviour
{
    private bool isSkipping = false;
    private GameObject canvasGo;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        SceneManager.sceneLoaded += OnStaticSceneLoaded;

        // If playing directly from the Editor, the scene is already active,
        // so we spawn the controller immediately.
        if (SceneManager.GetActiveScene().name == "1 IntroCutScene")
        {
            SpawnController();
        }
    }

    private static void OnStaticSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "1 IntroCutScene")
        {
            SpawnController();
        }
    }

    private static void SpawnController()
    {
        if (FindObjectOfType<SkipIntroController>() == null)
        {
            Debug.Log("[SkipIntroController] Intro CutScene loaded, spawning skip controller.");
            GameObject skipGo = new GameObject("SkipIntroController");
            skipGo.AddComponent<SkipIntroController>();
        }
    }

    private void Start()
    {
        CreateEventSystemIfNeeded();
        CreateSkipUI();
    }

    private void CreateEventSystemIfNeeded()
    {
        if (EventSystem.current == null && FindObjectOfType<EventSystem>() == null)
        {
            Debug.Log("[SkipIntroController] EventSystem not found. Creating EventSystem dynamically to handle UI clicks.");
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        SceneManager.sceneLoaded += OnOtherSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.sceneLoaded -= OnOtherSceneLoaded;
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        if (newScene.name != "1 IntroCutScene")
        {
            CleanupAndDestroy();
        }
    }

    private void OnOtherSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "1 IntroCutScene")
        {
            CleanupAndDestroy();
        }
    }

    private void CleanupAndDestroy()
    {
        Debug.Log("[SkipIntroController] Scene transition detected. Hiding button and destroying controller.");
        if (canvasGo != null)
        {
            canvasGo.SetActive(false);
            Destroy(canvasGo);
        }
        Destroy(gameObject);
    }

    private void Update()
    {
        // Force the cursor to be visible and unlocked so the player can interact with the skip button
        if (Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if (!Cursor.visible)
        {
            Cursor.visible = true;
        }
    }

    private void CreateSkipUI()
    {
        // 1. Create the Canvas Game Object
        canvasGo = new GameObject("SkipIntroCanvas");
        canvasGo.transform.SetParent(transform, false);

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; // Ensure it renders on top of dialogue panels and other overlays

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();

        // 2. Create the Button Game Object
        GameObject buttonGo = new GameObject("SkipButton");
        buttonGo.transform.SetParent(canvasGo.transform, false);

        // Styling the button background (semi-transparent dark charcoal)
        Image buttonImg = buttonGo.AddComponent<Image>();
        buttonImg.color = new Color(0.05f, 0.05f, 0.05f, 0.7f);

        // Attempt to load rounded corner background from UHFPS assets
        Sprite roundedSprite = null;
#if UNITY_EDITOR
        roundedSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThunderWire Studio/UHFPS/Content/Art/Textures/UI/SoftMasks/SoftMask_Circle.png");
#else
        Sprite[] sprites = Resources.FindObjectsOfTypeAll<Sprite>();
        foreach (var s in sprites)
        {
            if (s.name == "SoftMask_Circle")
            {
                roundedSprite = s;
                break;
            }
        }
#endif
        if (roundedSprite != null)
        {
            buttonImg.sprite = roundedSprite;
            buttonImg.type = Image.Type.Sliced;
        }

        // Thin gray outline around the button matching the reference design
        Outline outline = buttonGo.AddComponent<Outline>();
        outline.effectColor = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        outline.effectDistance = new Vector2(1.2f, 1.2f);

        Button buttonComp = buttonGo.AddComponent<Button>();
        buttonComp.onClick.AddListener(SkipIntro);

        // Hover & click background visual transitions
        ColorBlock colors = buttonComp.colors;
        colors.normalColor = new Color(0.05f, 0.05f, 0.05f, 0.7f);
        colors.highlightedColor = new Color(0.12f, 0.12f, 0.12f, 0.85f); // Lighter on hover
        colors.pressedColor = new Color(0.02f, 0.02f, 0.02f, 0.9f);       // Darker on press
        colors.selectedColor = colors.normalColor;
        colors.fadeDuration = 0.15f;
        buttonComp.colors = colors;

        // Position at the bottom-right corner with responsive anchors
        RectTransform rect = buttonGo.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0); // Bottom Right
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(1, 0);
        rect.sizeDelta = new Vector2(240, 55); // Container size
        rect.anchoredPosition = new Vector2(-70, 70); // Inset from edge

        // 3. Create the Text inside the Button
        GameObject textGo = new GameObject("ButtonText");
        textGo.transform.SetParent(buttonGo.transform, false);

        TextMeshProUGUI textComp = textGo.AddComponent<TextMeshProUGUI>();
        textComp.text = "OMITIR INTRO";
        textComp.fontSize = 19; // Increased font size as requested for better visibility
        textComp.characterSpacing = 4f; // Spaced out lettering for cinematic look
        textComp.fontStyle = FontStyles.Normal;
        textComp.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        textComp.alignment = TextAlignmentOptions.Center;

        // Load Roboto Condensed font from assets for consistency with UHFPS UI
        TMP_FontAsset mainFont = null;
#if UNITY_EDITOR
        mainFont = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/ThunderWire Studio/UHFPS/Content/Fonts/Roboto Condensed/TMP/RobotoCondensed-Regular SDF.asset");
#else
        TMP_Text[] allTexts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach (var t in allTexts)
        {
            if (t.font != null && t.font.name.Contains("RobotoCondensed"))
            {
                mainFont = t.font;
                break;
            }
        }
        if (mainFont == null && allTexts.Length > 0)
        {
            mainFont = allTexts[0].font;
        }
#endif
        if (mainFont != null)
        {
            textComp.font = mainFont;
        }

        // Make the text expand and fill the entire button boundaries
        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = new Vector2(2, 0); // Center adjustment for character spacing offset

        // 4. Load Audio Clip for Click (no hover sound)
        AudioClip clickClip = null;
#if UNITY_EDITOR
        clickClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/AssetsDescargados/AdvancedMobileHorror/Sounds/Audio_ButtonClick.wav");
#else
        AudioClip[] clips = Resources.FindObjectsOfTypeAll<AudioClip>();
        foreach (var clip in clips)
        {
            if (clip.name == "Audio_ButtonClick") clickClip = clip;
        }
#endif

        // Add hover effects for border, text color, and scale
        SkipButtonHoverEffects hoverEffects = buttonGo.AddComponent<SkipButtonHoverEffects>();
        hoverEffects.Init(
            outline, 
            textComp, 
            new Color(0.2f, 0.2f, 0.2f, 0.7f), // Normal border
            new Color(1f, 1f, 1f, 0.85f),      // Hover border (white highlight)
            new Color(0.9f, 0.9f, 0.9f, 1f),   // Normal text
            Color.white,                       // Hover text
            clickClip
        );
    }

    public void HideButton()
    {
        if (canvasGo != null)
        {
            canvasGo.SetActive(false);
            Destroy(canvasGo);
        }
    }

    public void SkipIntro()
    {
        if (isSkipping) return;
        isSkipping = true;

        Debug.Log("[SkipIntroController] Skip button clicked. Transitioning to next scene...");

        // Stop all background sounds and audio sources in the scene immediately
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

        // Hide the skip canvas immediately so the button disappears instantly
        if (canvasGo != null)
        {
            canvasGo.SetActive(false);
        }

        // Disable input locking to ensure everything cleans up properly
        if (GameManager.HasReference)
        {
            GameManager.Instance.LockInput(false);
        }

        // Locate the CinematicSceneLoader component
        var sceneLoader = FindObjectOfType<CinematicSceneLoader>();
        if (sceneLoader != null)
        {
            if (GameManager.HasReference)
            {
                StartCoroutine(SkipRoutine(sceneLoader));
            }
            else
            {
                sceneLoader.LoadNextSceneDirect();
            }
        }
        else
        {
            // Fallback load next scene directly if loader is not found
            Debug.LogWarning("[SkipIntroController] CinematicSceneLoader not found. Loading 1 IntroHouse directly.");
            SceneManager.LoadScene("1 IntroHouse");
        }
    }

    private IEnumerator SkipRoutine(CinematicSceneLoader loader)
    {
        // Smoothly fade to black before switching scenes for a premium transition
        yield return GameManager.Instance.StartBackgroundFade(false, fadeSpeed: 1.5f);
        loader.LoadNextSceneDirect();
    }
}

public class SkipButtonHoverEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Outline outline;
    private TextMeshProUGUI textComp;
    private RectTransform rectTransform;
    private AudioSource audioSource;

    private Color normalOutlineColor;
    private Color hoverOutlineColor;
    private Color normalTextColor;
    private Color hoverTextColor;

    private AudioClip clickSound;

    private Vector3 normalScale = Vector3.one;
    private Vector3 hoverScale = new Vector3(1.03f, 1.03f, 1.03f); // Subtle, professional zoom

    private bool isHovered = false;

    public void Init(Outline outline, TextMeshProUGUI textComp, Color normalOutline, Color hoverOutline, Color normalText, Color hoverText, AudioClip clickClip)
    {
        this.outline = outline;
        this.textComp = textComp;
        this.rectTransform = GetComponent<RectTransform>();

        this.normalOutlineColor = normalOutline;
        this.hoverOutlineColor = hoverOutline;
        this.normalTextColor = normalText;
        this.hoverTextColor = hoverText;

        this.clickSound = clickClip;

        // Add AudioSource for UI sound feedback
        this.audioSource = gameObject.AddComponent<AudioSource>();
        this.audioSource.playOnAwake = false;
        this.audioSource.spatialBlend = 0f; // 2D UI Sound
        this.audioSource.volume = 0.5f;
    }

    private void Update()
    {
        // Smoothly transition scale
        Vector3 targetScale = isHovered ? hoverScale : normalScale;
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * 10f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if (outline != null) outline.effectColor = hoverOutlineColor;
        if (textComp != null) textComp.color = hoverTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if (outline != null) outline.effectColor = normalOutlineColor;
        if (textComp != null) textComp.color = normalTextColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
    }
}
