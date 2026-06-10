using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UHFPS.Runtime;

public static class SetupControlsPanel
{
    [MenuItem("Tools/Generar Panel de Controles")]
    public static void GenerateControlsPanel()
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

        // Buscar si ya existe ControlsDialogContainer para evitar duplicados
        Transform existingContainer = parentObj.transform.Find("ControlsDialogContainer");
        if (existingContainer != null)
        {
            Undo.DestroyObjectImmediate(existingContainer.gameObject);
        }

        // Cargar fuente TMP por defecto
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

        // Sprite por defecto del circulo
        Sprite defaultDot = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // 1. Crear ControlsDialogContainer
        GameObject mainContainer = new GameObject("ControlsDialogContainer", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        mainContainer.transform.SetParent(parentObj.transform, false);
        Undo.RegisterCreatedObjectUndo(mainContainer, "Crear ControlsDialogContainer");

        RectTransform containerRect = mainContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.pivot = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = Vector2.zero;
        containerRect.sizeDelta = new Vector2(1300, 850);

        Image containerImage = mainContainer.GetComponent<Image>();
        containerImage.color = new Color(0.06f, 0.06f, 0.06f, 0.98f); // Fondo oscuro traslúcido
        
        var outline = mainContainer.AddComponent<Outline>();
        outline.effectColor = new Color(0.3f, 0.3f, 0.3f, 0.4f); // Borde crema suave
        outline.effectDistance = new Vector2(2, -2);

        // 2. Cabecera (Header)
        GameObject headerObj = new GameObject("Header", typeof(RectTransform));
        headerObj.transform.SetParent(mainContainer.transform, false);
        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 0.82f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.offsetMin = Vector2.zero;
        headerRect.offsetMax = Vector2.zero;

        // Título CONTROLES
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(headerObj.transform, false);
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0f, 0.45f);
        titleRect.anchorMax = new Vector2(1f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        TextMeshProUGUI titleTMP = titleObj.GetComponent<TextMeshProUGUI>();
        titleTMP.text = "CONTROLES";
        titleTMP.fontSize = 46;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = new Color(0.85f, 0.85f, 0.8f, 1f); // Crema
        if (defaultFont != null) titleTMP.font = defaultFont;

        // Subtítulo
        GameObject subtitleObj = new GameObject("SubtitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        subtitleObj.transform.SetParent(headerObj.transform, false);
        RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
        subtitleRect.anchorMin = new Vector2(0f, 0.15f);
        subtitleRect.anchorMax = new Vector2(1f, 0.45f);
        subtitleRect.offsetMin = Vector2.zero;
        subtitleRect.offsetMax = Vector2.zero;

        TextMeshProUGUI subtitleTMP = subtitleObj.GetComponent<TextMeshProUGUI>();
        subtitleTMP.text = "Domina tus movimientos. Sobrevive más tiempo.";
        subtitleTMP.fontSize = 20;
        subtitleTMP.alignment = TextAlignmentOptions.Center;
        subtitleTMP.color = new Color(0.65f, 0.65f, 0.6f, 1f);
        if (defaultFont != null) subtitleTMP.font = defaultFont;

        // Línea roja ornamental
        GameObject separatorObj = new GameObject("SeparatorLine", typeof(RectTransform), typeof(Image));
        separatorObj.transform.SetParent(headerObj.transform, false);
        RectTransform separatorRect = separatorObj.GetComponent<RectTransform>();
        separatorRect.anchorMin = new Vector2(0.3f, 0.05f);
        separatorRect.anchorMax = new Vector2(0.7f, 0.08f);
        separatorRect.offsetMin = Vector2.zero;
        separatorRect.offsetMax = Vector2.zero;

        Image separatorImg = separatorObj.GetComponent<Image>();
        separatorImg.color = new Color(0.55f, 0.12f, 0.12f, 0.8f); // Rojo oscuro

        // 3. Dos Columnas Cuerpo
        GameObject bodyObj = new GameObject("Body", typeof(RectTransform));
        bodyObj.transform.SetParent(mainContainer.transform, false);
        RectTransform bodyRect = bodyObj.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0f, 0.22f);
        bodyRect.anchorMax = new Vector2(1f, 0.80f);
        bodyRect.offsetMin = Vector2.zero;
        bodyRect.offsetMax = Vector2.zero;

        // 3a. Columna Izquierda (Controles teclado)
        GameObject leftCol = new GameObject("LeftColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
        leftCol.transform.SetParent(bodyObj.transform, false);
        RectTransform leftColRect = leftCol.GetComponent<RectTransform>();
        leftColRect.anchorMin = new Vector2(0.04f, 0f);
        leftColRect.anchorMax = new Vector2(0.62f, 1f);
        leftColRect.offsetMin = Vector2.zero;
        leftColRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = leftCol.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 12;
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.MiddleLeft;

        // Estilo común de fila de control
        TMP_Text movementTitle = null, movementDesc = null;
        TMP_Text jumpTitle = null, jumpDesc = null;
        TMP_Text crouchTitle = null, crouchDesc = null;
        TMP_Text interactTitle = null, interactDesc = null;
        TMP_Text inventoryTitle = null, inventoryDesc = null;
        TMP_Text runTitle = null, runDesc = null;

        Image movementIconImg = null;
        Image jumpIconImg = null;
        Image crouchIconImg = null;
        Image interactIconImg = null;
        Image inventoryIconImg = null;
        Image runIconImg = null;

        // Crear las 6 filas
        for (int i = 0; i < 6; i++)
        {
            string rowName = "";
            string actTitle = "";
            string actDesc = "";
            string[] keys = null;

            switch (i)
            {
                case 0:
                    rowName = "Row_Movement";
                    actTitle = "MOVIMIENTO";
                    actDesc = "Muévete en todas direcciones.";
                    keys = new string[] { "W", "A", "S", "D" };
                    break;
                case 1:
                    rowName = "Row_Jump";
                    actTitle = "SALTAR";
                    actDesc = "Salta para superar obstáculos.";
                    keys = new string[] { "SPACE" };
                    break;
                case 2:
                    rowName = "Row_Crouch";
                    actTitle = "AGACHARSE";
                    actDesc = "Agáchate para pasar desapercibido.";
                    keys = new string[] { "CTRL" };
                    break;
                case 3:
                    rowName = "Row_Interact";
                    actTitle = "INTERACTUAR";
                    actDesc = "Usa, abre o recoge objetos.";
                    keys = new string[] { "E" };
                    break;
                case 4:
                    rowName = "Row_Inventory";
                    actTitle = "INVENTARIO";
                    actDesc = "Abre el inventario.";
                    keys = new string[] { "TAB" };
                    break;
                case 5:
                    rowName = "Row_Run";
                    actTitle = "CORRER";
                    actDesc = "Corre para moverte más rápido.";
                    keys = new string[] { "SHIFT" };
                    break;
            }

            GameObject rowObj = new GameObject(rowName, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            rowObj.transform.SetParent(leftCol.transform, false);
            RectTransform rowRect = rowObj.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(720, 60);

            HorizontalLayoutGroup hlg = rowObj.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 15;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childAlignment = TextAnchor.MiddleLeft;

            // Fila: Icono decorativo rojo
            GameObject rowIcon = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            rowIcon.transform.SetParent(rowObj.transform, false);
            rowIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(30, 30);
            Image rIconImg = rowIcon.GetComponent<Image>();
            rIconImg.color = new Color(0.7f, 0.15f, 0.15f, 0.8f);
            
            // Sprite por defecto del circulo para que no se vea blanco si no hay asset
            if (defaultDot != null) rIconImg.sprite = defaultDot;

            // Fila: Contenedor de Tecla(s)
            GameObject keysContainer = new GameObject("KeysContainer", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            keysContainer.transform.SetParent(rowObj.transform, false);
            RectTransform keysContRect = keysContainer.GetComponent<RectTransform>();
            keysContRect.sizeDelta = new Vector2(210, 45);

            HorizontalLayoutGroup keysHlg = keysContainer.GetComponent<HorizontalLayoutGroup>();
            keysHlg.spacing = 6;
            keysHlg.childControlWidth = false;
            keysHlg.childControlHeight = false;
            keysHlg.childForceExpandWidth = false;
            keysHlg.childForceExpandHeight = false;
            keysHlg.childAlignment = TextAnchor.MiddleLeft;

            foreach (var keyName in keys)
            {
                GameObject keyBox = new GameObject("Key_" + keyName, typeof(RectTransform), typeof(Image));
                keyBox.transform.SetParent(keysContainer.transform, false);
                RectTransform kBoxRect = keyBox.GetComponent<RectTransform>();
                
                // Ajustar ancho de caja de tecla
                float width = 40;
                if (keyName == "SPACE") width = 110;
                else if (keyName == "SHIFT" || keyName == "CTRL") width = 70;
                kBoxRect.sizeDelta = new Vector2(width, 40);

                Image kBoxImg = keyBox.GetComponent<Image>();
                kBoxImg.color = new Color(0.08f, 0.08f, 0.08f, 1f);
                Sprite kSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                if (kSprite != null) kBoxImg.sprite = kSprite;

                var kBoxOutline = keyBox.AddComponent<Outline>();
                kBoxOutline.effectColor = new Color(0.6f, 0.6f, 0.55f, 0.5f);
                kBoxOutline.effectDistance = new Vector2(1, -1);

                // Texto dentro de la tecla
                GameObject keyTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                keyTextObj.transform.SetParent(keyBox.transform, false);
                RectTransform kTextRect = keyTextObj.GetComponent<RectTransform>();
                kTextRect.anchorMin = Vector2.zero;
                kTextRect.anchorMax = Vector2.one;
                kTextRect.offsetMin = Vector2.zero;
                kTextRect.offsetMax = Vector2.zero;

                TextMeshProUGUI kTextTMP = keyTextObj.GetComponent<TextMeshProUGUI>();
                kTextTMP.text = keyName;
                kTextTMP.fontSize = width > 40 ? 18 : 20;
                kTextTMP.fontStyle = FontStyles.Bold;
                kTextTMP.alignment = TextAlignmentOptions.Center;
                kTextTMP.color = Color.white;
                if (defaultFont != null) kTextTMP.font = defaultFont;
            }

            // Fila: Bloque de Texto (Título y Descripción)
            GameObject txtBlock = new GameObject("TextBlock", typeof(RectTransform));
            txtBlock.transform.SetParent(rowObj.transform, false);
            RectTransform txtBlockRect = txtBlock.GetComponent<RectTransform>();
            txtBlockRect.sizeDelta = new Vector2(430, 50);

            VerticalLayoutGroup txtVlg = txtBlock.AddComponent<VerticalLayoutGroup>();
            txtVlg.spacing = 2;
            txtVlg.childControlWidth = false;
            txtVlg.childControlHeight = false;
            txtVlg.childForceExpandWidth = false;
            txtVlg.childForceExpandHeight = false;
            txtVlg.childAlignment = TextAnchor.MiddleLeft;

            // Fila: Título de la acción
            GameObject actTitleObj = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            actTitleObj.transform.SetParent(txtBlock.transform, false);
            actTitleObj.GetComponent<RectTransform>().sizeDelta = new Vector2(430, 22);
            TextMeshProUGUI actTitleTMP = actTitleObj.GetComponent<TextMeshProUGUI>();
            actTitleTMP.text = actTitle;
            actTitleTMP.fontSize = 18;
            actTitleTMP.fontStyle = FontStyles.Bold;
            actTitleTMP.color = new Color(0.65f, 0.15f, 0.15f, 1f); // Rojo oscuro
            if (defaultFont != null) actTitleTMP.font = defaultFont;

            // Fila: Descripción de la acción
            GameObject actDescObj = new GameObject("Description", typeof(RectTransform), typeof(TextMeshProUGUI));
            actDescObj.transform.SetParent(txtBlock.transform, false);
            actDescObj.GetComponent<RectTransform>().sizeDelta = new Vector2(430, 22);
            TextMeshProUGUI actDescTMP = actDescObj.GetComponent<TextMeshProUGUI>();
            actDescTMP.text = actDesc;
            actDescTMP.fontSize = 15;
            actDescTMP.color = new Color(0.8f, 0.8f, 0.75f, 1f); // Crema
            if (defaultFont != null) actDescTMP.font = defaultFont;

            // Guardar referencias para el binding
            switch (i)
            {
                case 0:
                    movementTitle = actTitleTMP;
                    movementDesc = actDescTMP;
                    movementIconImg = rIconImg;
                    break;
                case 1:
                    jumpTitle = actTitleTMP;
                    jumpDesc = actDescTMP;
                    jumpIconImg = rIconImg;
                    break;
                case 2:
                    crouchTitle = actTitleTMP;
                    crouchDesc = actDescTMP;
                    crouchIconImg = rIconImg;
                    break;
                case 3:
                    interactTitle = actTitleTMP;
                    interactDesc = actDescTMP;
                    interactIconImg = rIconImg;
                    break;
                case 4:
                    inventoryTitle = actTitleTMP;
                    inventoryDesc = actDescTMP;
                    inventoryIconImg = rIconImg;
                    break;
                case 5:
                    runTitle = actTitleTMP;
                    runDesc = actDescTMP;
                    runIconImg = rIconImg;
                    break;
            }
        }

        // 3b. Columna Derecha (Mouse Girar)
        GameObject rightCol = new GameObject("RightColumn", typeof(RectTransform));
        rightCol.transform.SetParent(bodyObj.transform, false);
        RectTransform rightColRect = rightCol.GetComponent<RectTransform>();
        rightColRect.anchorMin = new Vector2(0.66f, 0f);
        rightColRect.anchorMax = new Vector2(0.96f, 1f);
        rightColRect.offsetMin = Vector2.zero;
        rightColRect.offsetMax = Vector2.zero;

        // Título MOUSE
        GameObject mouseTitleObj = new GameObject("MouseTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        mouseTitleObj.transform.SetParent(rightCol.transform, false);
        RectTransform mouseTitleRect = mouseTitleObj.GetComponent<RectTransform>();
        mouseTitleRect.anchorMin = new Vector2(0f, 0.85f);
        mouseTitleRect.anchorMax = new Vector2(1f, 0.95f);
        mouseTitleRect.offsetMin = Vector2.zero;
        mouseTitleRect.offsetMax = Vector2.zero;

        TextMeshProUGUI mouseTitleTMP = mouseTitleObj.GetComponent<TextMeshProUGUI>();
        mouseTitleTMP.text = "MOUSE";
        mouseTitleTMP.fontSize = 22;
        mouseTitleTMP.fontStyle = FontStyles.Bold;
        mouseTitleTMP.alignment = TextAlignmentOptions.Center;
        mouseTitleTMP.color = new Color(0.65f, 0.15f, 0.15f, 1f);
        if (defaultFont != null) mouseTitleTMP.font = defaultFont;

        // Subtítulo GIRAR
        GameObject mouseSubObj = new GameObject("MouseSubtitle", typeof(RectTransform), typeof(TextMeshProUGUI));
        mouseSubObj.transform.SetParent(rightCol.transform, false);
        RectTransform mouseSubRect = mouseSubObj.GetComponent<RectTransform>();
        mouseSubRect.anchorMin = new Vector2(0f, 0.75f);
        mouseSubRect.anchorMax = new Vector2(1f, 0.85f);
        mouseSubRect.offsetMin = Vector2.zero;
        mouseSubRect.offsetMax = Vector2.zero;

        TextMeshProUGUI mouseSubTMP = mouseSubObj.GetComponent<TextMeshProUGUI>();
        mouseSubTMP.text = "GIRAR";
        mouseSubTMP.fontSize = 18;
        mouseSubTMP.fontStyle = FontStyles.Bold;
        mouseSubTMP.alignment = TextAlignmentOptions.Center;
        mouseSubTMP.color = new Color(0.8f, 0.8f, 0.75f, 1f);
        if (defaultFont != null) mouseSubTMP.font = defaultFont;

        // Descripción de girar mouse
        GameObject mouseDescObj = new GameObject("MouseDescription", typeof(RectTransform), typeof(TextMeshProUGUI));
        mouseDescObj.transform.SetParent(rightCol.transform, false);
        RectTransform mouseDescRect = mouseDescObj.GetComponent<RectTransform>();
        mouseDescRect.anchorMin = new Vector2(0f, 0.60f);
        mouseDescRect.anchorMax = new Vector2(1f, 0.75f);
        mouseDescRect.offsetMin = Vector2.zero;
        mouseDescRect.offsetMax = Vector2.zero;

        TextMeshProUGUI mouseDescTMP = mouseDescObj.GetComponent<TextMeshProUGUI>();
        mouseDescTMP.text = "Mueve el mouse para girar la cámara.";
        mouseDescTMP.fontSize = 16;
        mouseDescTMP.alignment = TextAlignmentOptions.Center;
        mouseDescTMP.color = new Color(0.7f, 0.7f, 0.65f, 1f);
        if (defaultFont != null) mouseDescTMP.font = defaultFont;

        // Contenedor Ilustración de Ratón (Image de fondo y brillo)
        GameObject mouseIllustration = new GameObject("MouseIllustration", typeof(RectTransform), typeof(Image));
        mouseIllustration.transform.SetParent(rightCol.transform, false);
        RectTransform mouseIllRect = mouseIllustration.GetComponent<RectTransform>();
        mouseIllRect.anchorMin = new Vector2(0.5f, 0.28f);
        mouseIllRect.anchorMax = new Vector2(0.5f, 0.28f);
        mouseIllRect.pivot = new Vector2(0.5f, 0.5f);
        mouseIllRect.anchoredPosition = Vector2.zero;
        mouseIllRect.sizeDelta = new Vector2(200, 200);

        Image mouseIllImg = mouseIllustration.GetComponent<Image>();
        mouseIllImg.color = new Color(1f, 1f, 1f, 0.15f); // Brillo suave
        Sprite circleBg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        if (circleBg != null) mouseIllImg.sprite = circleBg;

        // Imagen de ratón en sí (sobrepuesta al brillo)
        GameObject mouseImageObj = new GameObject("MouseIcon", typeof(RectTransform), typeof(Image));
        mouseImageObj.transform.SetParent(mouseIllustration.transform, false);
        RectTransform mouseImgRect = mouseImageObj.GetComponent<RectTransform>();
        mouseImgRect.anchorMin = new Vector2(0.5f, 0.5f);
        mouseImgRect.anchorMax = new Vector2(0.5f, 0.5f);
        mouseImgRect.pivot = new Vector2(0.5f, 0.5f);
        mouseImgRect.anchoredPosition = Vector2.zero;
        mouseImgRect.sizeDelta = new Vector2(120, 160);

        Image mouseImg = mouseImageObj.GetComponent<Image>();
        mouseImg.color = Color.white;
        mouseImg.preserveAspect = true;

        // 4. Panel Inferior (Advertencia e Imagen)
        GameObject footerObj = new GameObject("Footer", typeof(RectTransform), typeof(Image));
        footerObj.transform.SetParent(mainContainer.transform, false);
        RectTransform footerRect = footerObj.GetComponent<RectTransform>();
        footerRect.anchorMin = new Vector2(0.04f, 0.10f);
        footerRect.anchorMax = new Vector2(0.96f, 0.21f);
        footerRect.offsetMin = Vector2.zero;
        footerRect.offsetMax = Vector2.zero;

        Image footerBg = footerObj.GetComponent<Image>();
        footerBg.color = new Color(0.04f, 0.04f, 0.04f, 0.95f);
        
        var footerOutline = footerObj.AddComponent<Outline>();
        footerOutline.effectColor = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        footerOutline.effectDistance = new Vector2(1, -1);

        // Icono de advertencia inferior
        GameObject warningIconObj = new GameObject("WarningIcon", typeof(RectTransform), typeof(Image));
        warningIconObj.transform.SetParent(footerObj.transform, false);
        RectTransform warningIconRect = warningIconObj.GetComponent<RectTransform>();
        warningIconRect.anchorMin = new Vector2(0.02f, 0.5f);
        warningIconRect.anchorMax = new Vector2(0.02f, 0.5f);
        warningIconRect.pivot = new Vector2(0f, 0.5f);
        warningIconRect.anchoredPosition = Vector2.zero;
        warningIconRect.sizeDelta = new Vector2(45, 40);

        Image warningIconImg = warningIconObj.GetComponent<Image>();
        warningIconImg.type = Image.Type.Simple;
        warningIconImg.preserveAspect = true;
        warningIconImg.color = new Color(0.7f, 0.15f, 0.15f, 1f); // Rojo oscuro para advertencias

        // Texto de advertencia inferior
        GameObject warningTextObj = new GameObject("WarningText", typeof(RectTransform), typeof(TextMeshProUGUI));
        warningTextObj.transform.SetParent(footerObj.transform, false);
        RectTransform warningTextRect = warningTextObj.GetComponent<RectTransform>();
        warningTextRect.anchorMin = new Vector2(0.08f, 0f);
        warningTextRect.anchorMax = new Vector2(0.70f, 1f);
        warningTextRect.offsetMin = Vector2.zero;
        warningTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI warningTextTMP = warningTextObj.GetComponent<TextMeshProUGUI>();
        warningTextTMP.text = "Conoce tus controles. La práctica es la clave para <color=#E63333>sobrevivir</color>.";
        warningTextTMP.fontSize = 18;
        warningTextTMP.alignment = TextAlignmentOptions.MidlineLeft;
        warningTextTMP.color = new Color(0.75f, 0.75f, 0.7f, 1f);
        if (defaultFont != null) warningTextTMP.font = defaultFont;

        // Imagen horizontal decorativa a la derecha del footer
        GameObject footerDecorImageObj = new GameObject("FooterDecorImage", typeof(RectTransform), typeof(Image));
        footerDecorImageObj.transform.SetParent(footerObj.transform, false);
        RectTransform footerDecorRect = footerDecorImageObj.GetComponent<RectTransform>();
        footerDecorRect.anchorMin = new Vector2(0.72f, 0f);
        footerDecorRect.anchorMax = new Vector2(1f, 1f);
        footerDecorRect.offsetMin = Vector2.zero;
        footerDecorRect.offsetMax = Vector2.zero;

        Image footerDecorImg = footerDecorImageObj.GetComponent<Image>();
        footerDecorImg.color = Color.white;
        footerDecorImg.preserveAspect = false; // Ajustar a los bordes rectangulares
        if (defaultDot != null) footerDecorImg.sprite = defaultDot; // Default placeholder

        // 5. Botón de Aceptar (ActionButton)
        GameObject btnObj = new GameObject("ActionButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(mainContainer.transform, false);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0f);
        btnRect.anchorMax = new Vector2(0.5f, 0f);
        btnRect.pivot = new Vector2(0.5f, 0f);
        btnRect.anchoredPosition = new Vector2(0, 22);
        btnRect.sizeDelta = new Vector2(260, 50);

        Image btnImg = btnObj.GetComponent<Image>();
        btnImg.color = new Color(0.12f, 0.12f, 0.12f, 1f);
        Sprite defaultBtnBg = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (defaultBtnBg != null) btnImg.sprite = defaultBtnBg;

        var btnOutline = btnObj.AddComponent<Outline>();
        btnOutline.effectColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
        btnOutline.effectDistance = new Vector2(1, -1);

        Button btnComp = btnObj.GetComponent<Button>();
        ColorBlock cb = btnComp.colors;
        cb.normalColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        cb.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        cb.pressedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        cb.selectedColor = cb.normalColor;
        btnComp.colors = cb;

        // Texto del botón
        GameObject btnTextObj = new GameObject("ActionButtonText", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTextObj.transform.SetParent(btnObj.transform, false);
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        TextMeshProUGUI btnTextTMP = btnTextObj.GetComponent<TextMeshProUGUI>();
        btnTextTMP.text = "ACEPTAR";
        btnTextTMP.fontSize = 20;
        btnTextTMP.fontStyle = FontStyles.Bold;
        btnTextTMP.alignment = TextAlignmentOptions.Center;
        btnTextTMP.color = Color.white;
        if (defaultFont != null) btnTextTMP.font = defaultFont;

        // 6. Asignar las referencias al script ControlsPopupPanel en PopUpPanel
        ControlsPopupPanel panelScript = parentObj.GetComponent<ControlsPopupPanel>();
        if (panelScript == null)
        {
            panelScript = Undo.AddComponent<ControlsPopupPanel>(parentObj);
        }

        panelScript.PanelCanvasGroup = parentObj.GetComponent<CanvasGroup>();
        if (panelScript.PanelCanvasGroup == null)
        {
            panelScript.PanelCanvasGroup = Undo.AddComponent<CanvasGroup>(parentObj);
        }

        panelScript.DialogContainer = mainContainer; // ControlsDialogContainer

        panelScript.TitleText = titleTMP;
        panelScript.SubtitleText = subtitleTMP;
        
        panelScript.MovementTitle = movementTitle;
        panelScript.MovementDesc = movementDesc;
        
        panelScript.JumpTitle = jumpTitle;
        panelScript.JumpDesc = jumpDesc;
        
        panelScript.CrouchTitle = crouchTitle;
        panelScript.CrouchDesc = crouchDesc;
        
        panelScript.InteractTitle = interactTitle;
        panelScript.InteractDesc = interactDesc;
        
        panelScript.InventoryTitle = inventoryTitle;
        panelScript.InventoryDesc = inventoryDesc;
        
        panelScript.RunTitle = runTitle;
        panelScript.RunDesc = runDesc;

        panelScript.MovementIconImg = movementIconImg;
        panelScript.JumpIconImg = jumpIconImg;
        panelScript.CrouchIconImg = crouchIconImg;
        panelScript.InteractIconImg = interactIconImg;
        panelScript.InventoryIconImg = inventoryIconImg;
        panelScript.RunIconImg = runIconImg;

        panelScript.MouseTitle = mouseTitleTMP;
        panelScript.MouseSubtitle = mouseSubTMP;
        panelScript.MouseDesc = mouseDescTMP;

        panelScript.WarningText = warningTextTMP;
        panelScript.WarningIconImg = warningIconImg;
        panelScript.MouseImage = mouseImg;
        panelScript.FooterDecorImage = footerDecorImg;
        panelScript.ActionButton = btnComp;
        panelScript.ActionButtonText = btnTextTMP;

        // Marcar objetos como sucios
        EditorUtility.SetDirty(parentObj);
        EditorUtility.SetDirty(panelScript);

        // Dejar el contenedor secundario activo para visualización, 
        // pero asegurar que el panel raíz PopUpPanel empiece activo (su ocultación es por CanvasGroup)
        parentObj.SetActive(true);

        // Seleccionar el contenedor en el inspector para visualización
        Selection.activeGameObject = mainContainer;

        Debug.Log("SUCCESS: Creado y vinculado el Panel de Controles (ControlsPopupPanel) correctamente.");
    }
}
