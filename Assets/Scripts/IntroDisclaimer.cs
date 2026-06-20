using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroDisclaimer : MonoBehaviour
{
    [Header("Configuración de Texto")]
    [Tooltip("Mensaje que se mostrará en pantalla")]
    [TextArea(3, 5)]
    public string disclaimerText = "Este juego está inspirado en hechos reales.";

    [Tooltip("Tamaño de la fuente para el texto")]
    public float fontSize = 32f;

    [Header("Configuración de Fondo")]
    [Tooltip("¿Mostrar un fondo oscuro translúcido detrás del texto?")]
    public bool showBackground = true;

    [Tooltip("Color y opacidad del fondo")]
    public Color backgroundColor = new Color(0f, 0f, 0f, 0.5f);

    [Header("Tiempos (Segundos)")]
    [Tooltip("Espera inicial antes de empezar a mostrar el texto")]
    public float delayBeforeShow = 1.0f;

    [Tooltip("Duración de la animación de aparición (Fade In)")]
    public float fadeInDuration = 1.5f;

    [Tooltip("Duración en pantalla con visibilidad completa")]
    public float displayDuration = 4.0f;

    [Tooltip("Duración de la animación de desaparición (Fade Out)")]
    public float fadeOutDuration = 1.5f;

    [Header("Posicionamiento")]
    [Tooltip("Límite inferior vertical del texto en pantalla (de 0 a 1)")]
    public float anchorYMin = 0.6f;

    [Tooltip("Límite superior vertical del texto en pantalla (de 0 a 1)")]
    public float anchorYMax = 0.9f;

    private GameObject canvasGo;
    private TextMeshProUGUI textComp;
    private Image bgImage;

    private void Start()
    {
        CreateDisclaimerUI();
        StartCoroutine(DisclaimerSequence());
    }

    private void CreateDisclaimerUI()
    {
        // 1. Crear el objeto Canvas (sin emparentar para que sea independiente de movimientos del personaje)
        canvasGo = new GameObject("IntroDisclaimerCanvas");

        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9998; // Se renderiza justo por debajo del botón de omitir (9999)

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // 2. Crear panel de fondo si está activado
        GameObject panelGo = new GameObject("DisclaimerBackground");
        panelGo.transform.SetParent(canvasGo.transform, false);
        
        bgImage = panelGo.AddComponent<Image>();
        if (showBackground)
        {
            bgImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0f); // Inicia invisible
        }
        else
        {
            bgImage.color = Color.clear;
        }

        RectTransform panelRect = panelGo.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;
        panelRect.anchoredPosition = Vector2.zero;

        // 3. Crear el objeto de Texto
        GameObject textGo = new GameObject("DisclaimerText");
        textGo.transform.SetParent(panelGo.transform, false);

        textComp = textGo.AddComponent<TextMeshProUGUI>();
        textComp.text = disclaimerText;
        textComp.fontSize = fontSize;
        textComp.color = new Color(1f, 1f, 1f, 0f); // Inicia invisible
        textComp.alignment = TextAlignmentOptions.Center;

        // Intentar buscar una fuente TMP por defecto o asignar una si existe.
        // Si no se asigna, TMPro automáticamente usa la fuente predeterminada (Liberation Sans), lo cual es seguro.

        RectTransform textRect = textGo.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.1f, anchorYMin);
        textRect.anchorMax = new Vector2(0.9f, anchorYMax);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
    }

    private IEnumerator DisclaimerSequence()
    {
        yield return new WaitForSeconds(delayBeforeShow);

        // --- Fade In ---
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            
            if (textComp != null)
                textComp.color = new Color(1f, 1f, 1f, alpha);
            
            if (showBackground && bgImage != null)
                bgImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, alpha * backgroundColor.a);
            
            yield return null;
        }

        if (textComp != null)
            textComp.color = Color.white;
        
        if (showBackground && bgImage != null)
            bgImage.color = backgroundColor;

        // --- Pantalla Activa ---
        yield return new WaitForSeconds(displayDuration);

        // --- Fade Out ---
        elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsed / fadeOutDuration));
            
            if (textComp != null)
                textComp.color = new Color(1f, 1f, 1f, alpha);
            
            if (showBackground && bgImage != null)
                bgImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, alpha * backgroundColor.a);
            
            yield return null;
        }

        if (textComp != null)
            textComp.color = new Color(1f, 1f, 1f, 0f);
        
        if (showBackground && bgImage != null)
            bgImage.color = new Color(backgroundColor.r, backgroundColor.g, backgroundColor.b, 0f);

        // --- Limpieza ---
        Destroy(canvasGo);
        
        // Si hay otros componentes en este GameObject (como Animator, etc.), solo destruimos el script.
        // Si es un objeto vacío creado solo para albergar este script, destruimos el GameObject completo.
        Component[] components = GetComponents<Component>();
        bool hasOtherComponents = false;
        foreach (var c in components)
        {
            if (c != null && c != this && !(c is Transform))
            {
                hasOtherComponents = true;
                break;
            }
        }

        if (hasOtherComponents)
        {
            Destroy(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
