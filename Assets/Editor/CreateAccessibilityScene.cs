using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;

public class CreateAccessibilityScene : EditorWindow
{
    [MenuItem("Tools/Create Accessibility Scene")]
    public static void GenerateScene()
    {
        // 1. Create a new scene
        Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // Configure Camera
        GameObject mainCam = GameObject.Find("Main Camera");
        if (mainCam != null)
        {
            Camera cam = mainCam.GetComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }

        // Remove Directional Light since it's a 2D UI splash screen
        GameObject dirLight = GameObject.Find("Directional Light");
        if (dirLight != null) DestroyImmediate(dirLight);

        // 2. Find Font Asset (RobotoCondensed or NotoSerif) from the project
        TMP_FontAsset fontAsset = null;
        string[] fontGuids = AssetDatabase.FindAssets("RobotoCondensed-Regular SDF t:TMP_FontAsset");
        if (fontGuids.Length > 0)
        {
            string fontPath = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        }
        else
        {
            // Fallback search for any NotoSerif SDF font
            fontGuids = AssetDatabase.FindAssets("NotoSerif-Regular SDF t:TMP_FontAsset");
            if (fontGuids.Length > 0)
            {
                string fontPath = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
                fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            }
        }

        if (fontAsset == null)
        {
            // Ultimate fallback to default TMPro font
            fontAsset = TMP_Settings.defaultFontAsset;
        }

        // 3. Find Custom Cursor Texture from the project
        Texture2D customCursorTexture = null;
        string[] cursorGuids = AssetDatabase.FindAssets("Cursor t:Texture2D");
        foreach (var guid in cursorGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.Contains("Decals/Cursor.png") || path.Contains("Cursor"))
            {
                customCursorTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (customCursorTexture != null) break;
            }
        }

        // 4. Load and configure warning icon sprite
        Sprite warningSprite = null;
        string warningSpritePath = "Assets/Sprites/letreros/warning.png";
        if (File.Exists(warningSpritePath))
        {
            TextureImporter importer = AssetImporter.GetAtPath(warningSpritePath) as TextureImporter;
            if (importer != null && importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.SaveAndReimport();
            }
            warningSprite = AssetDatabase.LoadAssetAtPath<Sprite>(warningSpritePath);
        }

        // 5. Create Canvas
        GameObject canvasGo = new GameObject("Canvas_Accessibility");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
        CanvasGroup mainCg = canvasGo.AddComponent<CanvasGroup>();

        // 6. Create Background
        GameObject bgGo = new GameObject("Background");
        bgGo.transform.SetParent(canvasGo.transform, false);
        Image bgImg = bgGo.AddComponent<Image>();
        bgImg.color = Color.black; // Pitch black background
        RectTransform bgRect = bgGo.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 7. Create Warning Panel (CanvasGroup)
        GameObject warningPanel = new GameObject("WarningPanel");
        warningPanel.AddComponent<RectTransform>();
        warningPanel.transform.SetParent(canvasGo.transform, false);
        CanvasGroup warningCg = warningPanel.AddComponent<CanvasGroup>();
        RectTransform warningRect = warningPanel.GetComponent<RectTransform>();
        warningRect.anchorMin = Vector2.zero;
        warningRect.anchorMax = Vector2.one;
        warningRect.sizeDelta = Vector2.zero;

        // 7a. Create Warning Icon
        if (warningSprite != null)
        {
            GameObject iconGo = new GameObject("WarningIcon");
            iconGo.AddComponent<RectTransform>();
            iconGo.transform.SetParent(warningPanel.transform, false);
            
            Image iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = warningSprite;
            iconImg.preserveAspect = true;
            
            RectTransform iconRect = iconGo.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.76f);
            iconRect.anchorMax = new Vector2(0.5f, 0.76f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(110f, 110f); // Sleek medium warning icon
        }

        // 7b. Create "ADVERTENCIA" Header
        GameObject headerGo = new GameObject("WarningHeader");
        headerGo.AddComponent<RectTransform>();
        headerGo.transform.SetParent(warningPanel.transform, false);
        
        TextMeshProUGUI headerText = headerGo.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null)
        {
            headerText.font = fontAsset;
        }
        headerText.text = "ADVERTENCIA";
        headerText.fontSize = 54;
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.color = new Color(0.9f, 0.15f, 0.15f, 1f); // Bright red warning color
        headerText.fontStyle = FontStyles.Bold;
        
        RectTransform headerRect = headerGo.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.1f, 0.62f);
        headerRect.anchorMax = new Vector2(0.9f, 0.72f);
        headerRect.sizeDelta = Vector2.zero;

        // 7c. Create Warning Text Component
        GameObject warningTextGo = new GameObject("WarningText");
        warningTextGo.AddComponent<RectTransform>();
        warningTextGo.transform.SetParent(warningPanel.transform, false);
        
        TextMeshProUGUI warningText = warningTextGo.AddComponent<TextMeshProUGUI>();
        if (fontAsset != null)
        {
            warningText.font = fontAsset;
        }
        warningText.text = "Este juego contiene escenas de terror psicológico, imágenes perturbadoras y efectos audiovisuales intensos. Se recomienda discreción para personas sensibles a este tipo de contenido.";
        warningText.fontSize = 40; // Increased size for better readability
        warningText.lineSpacing = 16f;
        warningText.alignment = TextAlignmentOptions.Center;
        warningText.color = new Color(0.88f, 0.88f, 0.88f, 1f); // Soft premium white
        
        RectTransform textRect = warningTextGo.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.15f, 0.2f);
        textRect.anchorMax = new Vector2(0.85f, 0.55f);
        textRect.sizeDelta = Vector2.zero;

        // 8. Add Custom Cursor Handler if texture is found
        if (customCursorTexture != null)
        {
            MainMenuCustomCursor customCursor = canvasGo.AddComponent<MainMenuCustomCursor>();
            customCursor.cursorTexture = customCursorTexture;
            customCursor.hotspot = new Vector2(10f, 6f);
            customCursor.cursorMode = CursorMode.Auto;
        }

        // 9. Create EventSystem if not exists
        if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 10. Attach AccessibilityScreen controller
        UHFPS.Runtime.AccessibilityScreen controller = canvasGo.AddComponent<UHFPS.Runtime.AccessibilityScreen>();
        controller.ScreenCanvasGroup = mainCg;
        controller.WarningCanvasGroup = warningCg;
        controller.WarningDisplayTime = 6.0f; // Wait time for reading slightly longer text
        controller.FadeSpeed = 1.2f;          // Smooth cinematic transitions

        // 11. Save the Scene
        string sceneDirectory = "Assets/Scenes";
        if (!Directory.Exists(sceneDirectory))
        {
            Directory.CreateDirectory(sceneDirectory);
        }
        string scenePath = sceneDirectory + "/0 AccessibilitySplash.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        Debug.Log("[Tools] Accessibility scene updated successfully: " + scenePath);

        // 12. Add scene to Build Settings as index 0 (if not already there)
        var currentScenes = EditorBuildSettings.scenes.ToList();
        currentScenes.RemoveAll(s => s.path.Contains("0 AccessibilitySplash.unity"));
        currentScenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = currentScenes.ToArray();
        
        Debug.Log("[Tools] Accessibility scene set as Build Index 0!");
        
        // Open the scene
        EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
    }
}
