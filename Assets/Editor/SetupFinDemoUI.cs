using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SetupFinDemoUI
{
    [MenuItem("Tools/Generar UI Fin de Demo")]
    public static void GenerateUI()
    {
        // 1. Create EventSystem if none exists
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        // 2. Create Canvas
        GameObject canvasGO = new GameObject("Canvas_FinDemo");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Add Fade Script and CanvasGroup
        canvasGO.AddComponent<CanvasGroup>();
        FinDemoAnimacion animScript = canvasGO.AddComponent<FinDemoAnimacion>();

        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Fin Demo UI");

        // 3. Background Panel
        GameObject background = new GameObject("Background");
        background.transform.SetParent(canvasGO.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.05f, 0.05f, 0.08f, 1f); // Dark cinematic color
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 4. Text - Thank you
        GameObject textGO = new GameObject("Text_ThankYou");
        textGO.transform.SetParent(canvasGO.transform, false);
        TextMeshProUGUI tmpText = textGO.AddComponent<TextMeshProUGUI>();
        tmpText.text = "¡GRACIAS POR JUGAR\nESTA DEMO!";
        tmpText.alignment = TextAlignmentOptions.Center;
        tmpText.fontSize = 120;
        tmpText.color = new Color(1f, 0.85f, 0.4f, 1f); // Gold-ish color
        tmpText.fontStyle = FontStyles.Bold;
        
        // Try to load default TMP font
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont != null) tmpText.font = defaultFont;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(1600, 400);
        textRect.anchoredPosition = new Vector2(0, 100);

        // 5. Button - Exit
        GameObject buttonGO = new GameObject("Button_Exit");
        buttonGO.transform.SetParent(canvasGO.transform, false);
        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);
        Button btn = buttonGO.AddComponent<Button>();
        btn.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = btn.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.25f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.4f, 1f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        colors.colorMultiplier = 1f;
        btn.colors = colors;

        RectTransform btnRect = buttonGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.sizeDelta = new Vector2(400, 100);
        btnRect.anchoredPosition = new Vector2(0, -200);

        // Hook up the button to the quit method
        if (animScript != null)
        {
            UnityEngine.Events.UnityAction action = new UnityEngine.Events.UnityAction(animScript.SalirDelJuego);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, action);
        }

        // Button Text
        GameObject btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(buttonGO.transform, false);
        TextMeshProUGUI btnTextTmp = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnTextTmp.text = "SALIR DEL JUEGO";
        btnTextTmp.alignment = TextAlignmentOptions.Center;
        btnTextTmp.fontSize = 40;
        btnTextTmp.color = Color.white;
        btnTextTmp.fontStyle = FontStyles.Bold;
        if (defaultFont != null) btnTextTmp.font = defaultFont;

        RectTransform btnTextRect = btnTextGO.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        // Ensure Canvas is selected
        Selection.activeGameObject = canvasGO;
        
        Debug.Log("UI Fin de Demo generada exitosamente. Asegúrate de guardar la escena.");
    }
}
