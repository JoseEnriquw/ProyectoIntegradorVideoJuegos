using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UHFPS.Runtime;
using TMPro;

public class HidingTutorialTrigger : MonoBehaviour
{
    private DialogueTrigger dialogueTrigger;
    private bool popupShown = false;
    private GameObject popupCanvasInstance;

    [Header("Tutorial Customization")]
    [Tooltip("El Sprite que se mostrará en el cartel. Si arrastras una imagen aquí, se usará directamente.")]
    public Sprite tutorialSprite;

    [Tooltip("Nombre de la imagen en la carpeta Resources (usado como fallback si no se asigna un Sprite).")]
    public string spriteResourceName = "letrero_escondite";

    [Header("UI Layout Customization")]
    [Tooltip("Tamaño de la imagen en pantalla (por defecto es 16:9).")]
    public Vector2 imageSize = new Vector2(1024, 576);

    [Tooltip("Posición del botón de aceptar relativa al centro del cartel.")]
    public Vector2 buttonPosition = new Vector2(0, -300);

    [Tooltip("Tamaño del botón de aceptar.")]
    public Vector2 buttonSize = new Vector2(200, 50);

    [Header("UI Prefab Override")]
    [Tooltip("Opcional: Puedes diseñar el Canvas del tutorial visualmente en el Editor de Unity, convertirlo en Prefab y arrastrarlo aquí.")]
    public GameObject tutorialPrefab;

    [Header("Custom Callback Events")]
    [Tooltip("Eventos que se ejecutarán cuando el jugador presione ACEPTAR y se cierre el cartel.")]
    public UnityEngine.Events.UnityEvent OnPopupDismissed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the scene is the town entrance
        if (scene.name.Contains("Ingreso_Pueblo") || scene.name.Contains("Pueblo"))
        {
            SetupTutorialTrigger();
        }
    }

    private static void SetupTutorialTrigger()
    {
        Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (Transform t in allTransforms)
        {
            if (t.name == "esconderse" && t.gameObject.scene == SceneManager.GetActiveScene())
            {
                if (t.gameObject.GetComponent<HidingTutorialTrigger>() == null)
                {
                    t.gameObject.AddComponent<HidingTutorialTrigger>();
                    Debug.Log("[HidingTutorial] Injected HidingTutorialTrigger into 'esconderse' GameObject.");
                }
                break;
            }
        }
    }

    private DialogueTrigger.TriggerTypeEnum originalTriggerType;

    private void Awake()
    {
        dialogueTrigger = GetComponent<DialogueTrigger>();
    }

    private void Start()
    {
        if (dialogueTrigger != null)
        {
            // Store the original trigger type and switch to Event to prevent triggering on collision.
            // This allows the DialogueTrigger's Start() to run and initialize DialogueData.
            originalTriggerType = dialogueTrigger.TriggerType;
            dialogueTrigger.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (popupShown) return;

        if (other.CompareTag("Player"))
        {
            popupShown = true;
            ShowTutorialPopup();
        }
    }

    private void ShowTutorialPopup()
    {
        // 1. Freeze player and pause physics/game updates
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FreezePlayer(true, true);
        }
        Time.timeScale = 0f;

        // 2. Instantiate custom prefab if available, otherwise fall back to code-generated UI
        if (tutorialPrefab != null)
        {
            popupCanvasInstance = Instantiate(tutorialPrefab);
            
            // Auto-configure the button inside the prefab
            Button btn = popupCanvasInstance.GetComponentInChildren<Button>(true);
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(OnContinuePressed);
            }
            else
            {
                Debug.LogWarning("[HidingTutorial] No Button found in the custom tutorial prefab!", popupCanvasInstance);
            }
        }
        else
        {
            popupCanvasInstance = CreatePopupUI();
        }
    }

    private GameObject CreatePopupUI()
    {
        // Try to load horroroid SDF font from memory
        TMP_FontAsset horroroidFont = null;
        TMP_FontAsset[] loadedFonts = Resources.FindObjectsOfTypeAll<TMP_FontAsset>();
        foreach (var f in loadedFonts)
        {
            if (f.name.Contains("horroroid"))
            {
                horroroidFont = f;
                break;
            }
        }

        // Create Root Canvas
        GameObject canvasObj = new GameObject("HidingTutorialCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999; 

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Ensure EventSystem is present
        if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        // Background dark overlay (pure cinematic dark overlay)
        GameObject bgObj = new GameObject("BackgroundOverlay");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.9f);
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Centered Letrero Image (Visual containing instructions and tutorial)
        GameObject letreroObj = new GameObject("LetreroImage");
        letreroObj.transform.SetParent(canvasObj.transform, false);
        Image letreroImg = letreroObj.AddComponent<Image>();
        
        Sprite loadedSprite = tutorialSprite;
        if (loadedSprite == null)
        {
            loadedSprite = Resources.Load<Sprite>(spriteResourceName);
            if (loadedSprite == null && !string.IsNullOrEmpty(spriteResourceName))
            {
                // Fallback: Try loading as Texture2D and dynamically creating a Sprite
                Texture2D texture = Resources.Load<Texture2D>(spriteResourceName);
                if (texture != null)
                {
                    loadedSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
        }

        if (loadedSprite != null)
        {
            letreroImg.sprite = loadedSprite;
            letreroImg.color = Color.white; // Ensure no color tint
            letreroImg.preserveAspect = true;
        }
        else
        {
            letreroImg.color = new Color(0.2f, 0.05f, 0.05f, 1f); // Dark red placeholder if missing
        }

        RectTransform letreroRect = letreroObj.GetComponent<RectTransform>();
        letreroRect.anchorMin = new Vector2(0.5f, 0.5f);
        letreroRect.anchorMax = new Vector2(0.5f, 0.5f);
        letreroRect.pivot = new Vector2(0.5f, 0.5f);
        letreroRect.anchoredPosition = new Vector2(0, 40); // Shifted slightly up to leave room for the button
        letreroRect.sizeDelta = imageSize; // Set custom image size

        // Button Outer Outline (Black contour)
        GameObject btnOutlineObj = new GameObject("ButtonOutline");
        btnOutlineObj.transform.SetParent(canvasObj.transform, false);
        Image btnOutlineImg = btnOutlineObj.AddComponent<Image>();
        btnOutlineImg.color = Color.black; // Contour of the button is black
        RectTransform btnOutlineRect = btnOutlineObj.GetComponent<RectTransform>();
        btnOutlineRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnOutlineRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnOutlineRect.pivot = new Vector2(0.5f, 0.5f);
        btnOutlineRect.anchoredPosition = buttonPosition; // Set custom button position
        btnOutlineRect.sizeDelta = buttonSize; // Set custom outline size

        // Aceptar Button Interactive Object
        GameObject buttonObj = new GameObject("AceptarButton");
        buttonObj.transform.SetParent(btnOutlineObj.transform, false);
        Image btnImg = buttonObj.AddComponent<Image>();
        btnImg.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark grey button background (#1A1A1A)
        RectTransform btnRect = buttonObj.GetComponent<RectTransform>();
        btnRect.anchorMin = Vector2.zero;
        btnRect.anchorMax = Vector2.one;
        btnRect.offsetMin = new Vector2(2, 2);
        btnRect.offsetMax = new Vector2(-2, -2);

        Button buttonComp = buttonObj.AddComponent<Button>();
        buttonComp.onClick.AddListener(OnContinuePressed);

        // Set highlight/press transition colors matching main menu text-hover effects
        ColorBlock cb = buttonComp.colors;
        cb.normalColor = new Color(0.1f, 0.1f, 0.1f, 1f);
        cb.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1f); // Lighter grey on hover
        cb.pressedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        cb.selectedColor = cb.normalColor;
        buttonComp.colors = cb;

        // Button Text label
        GameObject btnTextObj = new GameObject("Label");
        btnTextObj.transform.SetParent(buttonObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        if (horroroidFont != null) btnText.font = horroroidFont;
        btnText.text = "ACEPTAR";
        btnText.fontSize = 22;
        btnText.fontStyle = FontStyles.Bold;
        btnText.color = Color.white; // Text color is white
        btnText.alignment = TextAlignmentOptions.Center;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        return canvasObj;
    }

    private void OnContinuePressed()
    {
        // 1. Clean up UI popup Canvas first to ensure it disappears instantly
        if (popupCanvasInstance != null)
        {
            Destroy(popupCanvasInstance);
        }

        // 2. Resume game time and update loops
        Time.timeScale = 1f;

        // 3. Unfreeze player controls and hide cursor
        if (GameManager.Instance != null)
        {
            GameManager.Instance.FreezePlayer(false, false);
        }

        // 4. Restore the dialogue trigger type and trigger it
        try
        {
            if (dialogueTrigger != null)
            {
                dialogueTrigger.TriggerType = originalTriggerType;
                dialogueTrigger.TriggerDialogue();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[HidingTutorial] Error triggering dialogue: {ex.Message}\n{ex.StackTrace}");
        }

        // 5. Fire custom events
        OnPopupDismissed?.Invoke();
    }
}
