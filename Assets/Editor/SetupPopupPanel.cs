using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UHFPS.Runtime;

public static class SetupPopupPanel
{
    [MenuItem("Tools/Generar Componentes PopupPanel")]
    public static void GeneratePopupComponents()
    {
        // Buscar PopUpPanel en la escena activa (incluyendo inactivos)
        GameObject parentObj = null;
        GameObject[] rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in rootObjs)
        {
            if (root.name == "PopUpPanel")
            {
                parentObj = root;
                break;
            }
            Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);
            foreach (var t in allChildren)
            {
                if (t.name == "PopUpPanel")
                {
                    parentObj = t.gameObject;
                    break;
                }
            }
            if (parentObj != null) break;
        }

        if (parentObj == null)
        {
            GameObject[] allGOs = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var go in allGOs)
            {
                if (go.name == "PopUpPanel" && go.scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene())
                {
                    parentObj = go;
                    break;
                }
            }
        }

        if (parentObj == null)
        {
            Debug.LogError("Error: No se encontró el GameObject 'PopUpPanel' en la escena activa. Asegúrate de tener la escena correcta abierta en el editor.");
            return;
        }

        // Verificar si MainDialogContainer ya existe para evitar duplicar
        Transform existingContainer = parentObj.transform.Find("MainDialogContainer");
        if (existingContainer != null)
        {
            Undo.DestroyObjectImmediate(existingContainer.gameObject);
        }

        // 1. Crear MainDialogContainer
        GameObject mainContainer = new GameObject("MainDialogContainer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        mainContainer.transform.SetParent(parentObj.transform, false);
        Undo.RegisterCreatedObjectUndo(mainContainer, "Crear MainDialogContainer");

        RectTransform containerRect = mainContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(1300, 800);

        Image containerImage = mainContainer.GetComponent<Image>();
        containerImage.color = new Color(0.06f, 0.06f, 0.06f, 0.98f);
        
        var outline = mainContainer.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 0.4f);
        outline.effectDistance = new Vector2(2, -2);

        // 2. Crear LeftColumn
        GameObject leftCol = new GameObject("LeftColumn", typeof(RectTransform));
        leftCol.transform.SetParent(mainContainer.transform, false);
        Undo.RegisterCreatedObjectUndo(leftCol, "Crear LeftColumn");

        RectTransform leftColRect = leftCol.GetComponent<RectTransform>();
        leftColRect.anchorMin = new Vector2(0f, 0.15f);
        leftColRect.anchorMax = new Vector2(0.5f, 0.95f);
        leftColRect.offsetMin = Vector2.zero;
        leftColRect.offsetMax = Vector2.zero;

        // 3. Crear TitleText (TMP)
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(leftCol.transform, false);
        Undo.RegisterCreatedObjectUndo(titleObj, "Crear TitleText");
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.85f);
        titleRect.anchorMax = new Vector2(1f, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "TÍTULO DEL POPUP";
        titleTMP.fontSize = 40;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;

        // 4. Crear TopicIcon (Image)
        GameObject iconObj = new GameObject("TopicIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObj.transform.SetParent(leftCol.transform, false);
        Undo.RegisterCreatedObjectUndo(iconObj, "Crear TopicIcon");

        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.65f);
        iconRect.anchorMax = new Vector2(0.5f, 0.65f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = Vector2.zero;
        iconRect.sizeDelta = new Vector2(120, 120);

        Image iconImage = iconObj.GetComponent<Image>();
        iconImage.color = Color.white;

        Sprite circleSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        if (circleSprite != null) iconImage.sprite = circleSprite;

        // 5. Crear DescriptionText (TMP)
        GameObject descObj = new GameObject("DescriptionText", typeof(RectTransform), typeof(TextMeshProUGUI));
        descObj.transform.SetParent(leftCol.transform, false);
        Undo.RegisterCreatedObjectUndo(descObj, "Crear DescriptionText");

        RectTransform descRect = descObj.GetComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.05f, 0.35f);
        descRect.anchorMax = new Vector2(0.95f, 0.55f);
        descRect.offsetMin = Vector2.zero;
        descRect.offsetMax = Vector2.zero;

        TextMeshProUGUI descTMP = descObj.GetComponent<TextMeshProUGUI>();
        descTMP.text = "Aquí va la descripción del popup.";
        descTMP.fontSize = 24;
        descTMP.alignment = TextAlignmentOptions.MidlineLeft; // Alineado a la izquierda para coincidir con la tecla y la advertencia
        descTMP.color = Color.white;

        // 6. Crear KeyPromptContainer
        GameObject keyPromptObj = new GameObject("KeyPromptContainer", typeof(RectTransform));
        keyPromptObj.transform.SetParent(leftCol.transform, false);
        Undo.RegisterCreatedObjectUndo(keyPromptObj, "Crear KeyPromptContainer");

        RectTransform keyPromptRect = keyPromptObj.GetComponent<RectTransform>();
        keyPromptRect.anchorMin = new Vector2(0.05f, 0.20f);
        keyPromptRect.anchorMax = new Vector2(0.95f, 0.30f);
        keyPromptRect.offsetMin = Vector2.zero;
        keyPromptRect.offsetMax = Vector2.zero;

        // Configurar HorizontalLayoutGroup para alinear el Icono de la tecla y el texto descriptivo
        HorizontalLayoutGroup layoutGroup = keyPromptObj.AddComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 15;
        layoutGroup.childControlWidth = false;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;

        // 6a. Crear KeyIconImage (Fondo del botón de la tecla)
        GameObject keyIconImageObj = new GameObject("KeyIconImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        keyIconImageObj.transform.SetParent(keyPromptObj.transform, false);
        Undo.RegisterCreatedObjectUndo(keyIconImageObj, "Crear KeyIconImage");

        RectTransform keyIconImageRect = keyIconImageObj.GetComponent<RectTransform>();
        keyIconImageRect.sizeDelta = new Vector2(50, 50);

        Image keyIconImg = keyIconImageObj.GetComponent<Image>();
        keyIconImg.color = new Color(0.08f, 0.08f, 0.08f, 1f); // Gris oscuro

        Sprite btnSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (btnSprite != null) keyIconImg.sprite = btnSprite;

        var keyIconOutline = keyIconImageObj.AddComponent<Outline>();
        keyIconOutline.effectColor = new Color(0.8f, 0.8f, 0.8f, 0.6f); // Borde claro
        keyIconOutline.effectDistance = new Vector2(1, -1);

        // 6b. Crear KeyIconText (Texto de la tecla, ej. "E")
        GameObject keyIconTextObj = new GameObject("KeyIconText", typeof(RectTransform), typeof(TextMeshProUGUI));
        keyIconTextObj.transform.SetParent(keyIconImageObj.transform, false);
        Undo.RegisterCreatedObjectUndo(keyIconTextObj, "Crear KeyIconText");

        RectTransform keyIconTextRect = keyIconTextObj.GetComponent<RectTransform>();
        keyIconTextRect.anchorMin = Vector2.zero;
        keyIconTextRect.anchorMax = Vector2.one;
        keyIconTextRect.offsetMin = Vector2.zero;
        keyIconTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI keyIconTextTMP = keyIconTextObj.GetComponent<TextMeshProUGUI>();
        keyIconTextTMP.text = "E";
        keyIconTextTMP.fontSize = 24;
        keyIconTextTMP.fontStyle = FontStyles.Bold;
        keyIconTextTMP.alignment = TextAlignmentOptions.Center;
        keyIconTextTMP.color = Color.white;

        // 6c. Crear KeyPromptText (El texto descriptivo de la acción)
        GameObject keyPromptTextObj = new GameObject("KeyPromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
        keyPromptTextObj.transform.SetParent(keyPromptObj.transform, false);
        Undo.RegisterCreatedObjectUndo(keyPromptTextObj, "Crear KeyPromptText");

        RectTransform keyPromptTextRect = keyPromptTextObj.GetComponent<RectTransform>();
        keyPromptTextRect.sizeDelta = new Vector2(350, 50);

        TextMeshProUGUI keyPromptTextTMP = keyPromptTextObj.GetComponent<TextMeshProUGUI>();
        keyPromptTextTMP.text = "para guardar.";
        keyPromptTextTMP.fontSize = 22;
        keyPromptTextTMP.alignment = TextAlignmentOptions.MidlineLeft;
        keyPromptTextTMP.color = Color.white;

        // 7. Crear WarningContainer
        GameObject warningObj = new GameObject("WarningContainer", typeof(RectTransform));
        warningObj.transform.SetParent(leftCol.transform, false);
        Undo.RegisterCreatedObjectUndo(warningObj, "Crear WarningContainer");

        RectTransform warningRect = warningObj.GetComponent<RectTransform>();
        warningRect.anchorMin = new Vector2(0.05f, 0.02f);
        warningRect.anchorMax = new Vector2(0.95f, 0.15f);
        warningRect.offsetMin = Vector2.zero;
        warningRect.offsetMax = Vector2.zero;

        // Configurar HorizontalLayoutGroup para alinear el Icono de advertencia y el texto descriptivo
        HorizontalLayoutGroup warningLayoutGroup = warningObj.AddComponent<HorizontalLayoutGroup>();
        warningLayoutGroup.spacing = 15;
        warningLayoutGroup.childControlWidth = false;
        warningLayoutGroup.childControlHeight = false;
        warningLayoutGroup.childForceExpandWidth = false;
        warningLayoutGroup.childForceExpandHeight = false;
        warningLayoutGroup.childAlignment = TextAnchor.MiddleLeft;

        // 7a. Crear WarningIcon (Imagen del triángulo rojo)
        GameObject warningIconObj = new GameObject("WarningIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        warningIconObj.transform.SetParent(warningObj.transform, false);
        Undo.RegisterCreatedObjectUndo(warningIconObj, "Crear WarningIcon");

        RectTransform warningIconRect = warningIconObj.GetComponent<RectTransform>();
        warningIconRect.sizeDelta = new Vector2(40, 40);

        Image warningIconImg = warningIconObj.GetComponent<Image>();
        warningIconImg.type = Image.Type.Simple;
        warningIconImg.preserveAspect = true;
        warningIconImg.color = new Color(0.9f, 0.2f, 0.2f, 1f); // Rojo para advertencias
        
        Sprite warningOutlineSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIOutline.psd");
        if (warningOutlineSprite != null) warningIconImg.sprite = warningOutlineSprite;

        // 7b. Crear WarningText (El texto de la advertencia)
        GameObject warningTextObj = new GameObject("WarningText", typeof(RectTransform), typeof(TextMeshProUGUI));
        warningTextObj.transform.SetParent(warningObj.transform, false);
        Undo.RegisterCreatedObjectUndo(warningTextObj, "Crear WarningText");

        RectTransform warningTextRect = warningTextObj.GetComponent<RectTransform>();
        warningTextRect.sizeDelta = new Vector2(350, 60);

        TextMeshProUGUI warningTextTMP = warningTextObj.GetComponent<TextMeshProUGUI>();
        warningTextTMP.text = "Guarda con frecuencia. Nunca sabes lo que puede pasar.";
        warningTextTMP.fontSize = 18;
        warningTextTMP.alignment = TextAlignmentOptions.MidlineLeft;
        warningTextTMP.color = new Color(0.9f, 0.3f, 0.3f, 1f);

        // 8. Crear PreviewImage
        GameObject previewObj = new GameObject("PreviewImage", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        previewObj.transform.SetParent(mainContainer.transform, false);
        Undo.RegisterCreatedObjectUndo(previewObj, "Crear PreviewImage");

        RectTransform previewRect = previewObj.GetComponent<RectTransform>();
        previewRect.anchorMin = new Vector2(0.52f, 0.15f);
        previewRect.anchorMax = new Vector2(0.98f, 0.95f);
        previewRect.offsetMin = Vector2.zero;
        previewRect.offsetMax = Vector2.zero;

        Image previewImage = previewObj.GetComponent<Image>();
        previewImage.preserveAspect = true;
        previewImage.color = Color.white;

        Sprite defaultPanelBg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        if (defaultPanelBg != null) previewImage.sprite = defaultPanelBg;

        // 9. Crear ActionButton
        GameObject btnObj = new GameObject("ActionButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(mainContainer.transform, false);
        Undo.RegisterCreatedObjectUndo(btnObj, "Crear ActionButton");

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = new Vector2(0, 40);
        btnRect.sizeDelta = new Vector2(260, 60);

        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        Sprite defaultBtnBg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (defaultBtnBg != null) btnImg.sprite = defaultBtnBg;

        Button btnComp = btnObj.GetComponent<Button>();
        ColorBlock cb = btnComp.colors;
        cb.normalColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        cb.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        cb.pressedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        cb.selectedColor = cb.normalColor;
        btnComp.colors = cb;

        // ActionButtonText
        GameObject btnTextObj = new GameObject("ActionButtonText", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTextObj.transform.SetParent(btnObj.transform, false);
        Undo.RegisterCreatedObjectUndo(btnTextObj, "Crear ActionButtonText");

        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI btnTextTMP = btnTextObj.GetComponent<TextMeshProUGUI>();
        btnTextTMP.text = "ACEPTAR";
        btnTextTMP.fontSize = 22;
        btnTextTMP.fontStyle = FontStyles.Bold;
        btnTextTMP.alignment = TextAlignmentOptions.Center;
        btnTextTMP.color = Color.white;

        // 10. Vincular las referencias al script en PopUpPanel
        PopupInfoPanel panelScript = parentObj.GetComponent<PopupInfoPanel>();
        if (panelScript == null)
        {
            panelScript = Undo.AddComponent<PopupInfoPanel>(parentObj);
        }

        panelScript.PanelCanvasGroup = parentObj.GetComponent<CanvasGroup>();
        if (panelScript.PanelCanvasGroup == null)
        {
            panelScript.PanelCanvasGroup = Undo.AddComponent<CanvasGroup>(parentObj);
        }

        panelScript.TitleText = titleTMP;
        panelScript.TopicIcon = iconImage;
        panelScript.DescriptionText = descTMP;
        panelScript.KeyPromptContainer = keyPromptObj;
        panelScript.KeyIconImage = keyIconImg;
        panelScript.KeyIconText = keyIconTextTMP;
        panelScript.KeyPromptText = keyPromptTextTMP;
        panelScript.WarningContainer = warningObj;
        panelScript.WarningText = warningTextTMP;
        panelScript.PreviewImage = previewImage;
        panelScript.ActionButton = btnComp;
        panelScript.ActionButtonText = btnTextTMP;

        // Marcar el script y el panel como dirty para guardar los cambios
        EditorUtility.SetDirty(parentObj);
        EditorUtility.SetDirty(panelScript);

        // Desactivar el panel principal para que empiece oculto por defecto
        parentObj.SetActive(false);

        // Seleccionar el objeto en el inspector
        Selection.activeGameObject = parentObj;

        Debug.Log("SUCCESS: Creado y vinculado todo el popup en PopUpPanel correctamente.");
    }
}
