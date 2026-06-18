using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UHFPS.Runtime;

public class ImproveControlsPopup : EditorWindow
{
    [MenuItem("Tools/Improve Controls Popup")]
    public static void ImprovePopup()
    {
        Debug.Log("[ImproveControlsPopup] Starting advanced UX/UI improvements...");

        // 1. Corregir y configurar Texturas en el Asset Database
        FixTextureImporter("Assets/Sprites/letreros/controls/movement_icon.png");
        FixTextureImporter("Assets/Sprites/letreros/controls/jump_icon.png");
        FixTextureImporter("Assets/Sprites/letreros/controls/crouch_icon.png");
        FixTextureImporter("Assets/Sprites/letreros/controls/interact_icon.png");
        FixTextureImporter("Assets/Sprites/letreros/controls/inventory_icon.png");
        FixTextureImporter("Assets/Sprites/letreros/controls/run_icon.png");
        FixTextureImporter("Assets/Sprites/letreros/controls/mouse.png");
        FixTextureImporter("Assets/Sprites/letreros/warning.png");
        FixTextureImporter("Assets/Sprites/letreros/footer.png");

        // 2. Encontrar ControlsDialogContainer
        GameObject controlsContainer = FindGameObjectInScene("ControlsDialogContainer");

        if (controlsContainer == null)
        {
            Debug.LogError("[ImproveControlsPopup] ControlsDialogContainer not found in active scene.");
            return;
        }

        Undo.RecordObject(controlsContainer, "Improve Controls Layout");

        // 3. Ajustar el tamaño y posición del panel principal
        RectTransform containerRect = controlsContainer.GetComponent<RectTransform>();
        if (containerRect != null)
        {
            Undo.RecordObject(containerRect, "Adjust Container RectTransform");
            containerRect.anchorMin = new Vector2(0.5f, 0.5f);
            containerRect.anchorMax = new Vector2(0.5f, 0.5f);
            containerRect.pivot = new Vector2(0.5f, 0.5f);
            containerRect.anchoredPosition = Vector2.zero; // Centrado exacto
            containerRect.sizeDelta = new Vector2(1300, 800); // 1300x800 para fondo_popup.png
        }

        // Quitar Outline del contenedor principal
        Outline containerOutline = controlsContainer.GetComponent<Outline>();
        if (containerOutline != null)
        {
            Undo.RecordObject(containerOutline, "Disable Container Outline");
            containerOutline.enabled = false;
        }

        // 4. Cambiar el sprite de fondo al fondo_popup.png
        Image controlsBgImage = controlsContainer.GetComponent<Image>();
        Sprite popupBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/letreros/fondo_popup.png");
        if (controlsBgImage != null && popupBgSprite != null)
        {
            Undo.RecordObject(controlsBgImage, "Change Background Sprite");
            controlsBgImage.sprite = popupBgSprite;
            controlsBgImage.color = Color.white; 
            controlsBgImage.type = Image.Type.Simple; 
        }

        // 5. Cargar fuentes del proyecto
        TMP_FontAsset titleFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/horroroid/horroroidexpand SDF.asset");
        TMP_FontAsset bodyFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/ThunderWire Studio/UHFPS/Content/Fonts/NotoSerif/TMP/Normal/Normal/NotoSerif-Thin SDF.asset");

        if (titleFont == null) Debug.LogWarning("[ImproveControlsPopup] horroroidexpand SDF font not found!");
        if (bodyFont == null) Debug.LogWarning("[ImproveControlsPopup] NotoSerif-Thin SDF font not found!");

        // 6. Cabecera (Header)
        Transform headerTrans = controlsContainer.transform.Find("Header");
        if (headerTrans != null)
        {
            RectTransform headerRect = headerTrans.GetComponent<RectTransform>();
            if (headerRect != null)
            {
                Undo.RecordObject(headerRect, "Adjust Header Rect");
                headerRect.anchorMin = new Vector2(0f, 0.80f); 
                headerRect.anchorMax = new Vector2(1f, 1f);
                headerRect.offsetMin = Vector2.zero;
                headerRect.offsetMax = Vector2.zero;
            }

            TMP_Text titleTxt = headerTrans.Find("TitleText")?.GetComponent<TMP_Text>();
            if (titleTxt != null)
            {
                Undo.RecordObject(titleTxt, "Update Title Text Style");
                if (titleFont != null) titleTxt.font = titleFont;
                titleTxt.fontSize = 55; 
                titleTxt.color = new Color(0.85f, 0.2f, 0.2f, 1f); // Crimson horror red
                titleTxt.fontStyle = FontStyles.Bold;
                titleTxt.alignment = TextAlignmentOptions.Center;
            }

            TMP_Text subtitleTxt = headerTrans.Find("SubtitleText")?.GetComponent<TMP_Text>();
            if (subtitleTxt != null)
            {
                Undo.RecordObject(subtitleTxt, "Update Subtitle Text Style");
                if (bodyFont != null) subtitleTxt.font = bodyFont;
                subtitleTxt.fontSize = 22;
                subtitleTxt.color = new Color(0.8f, 0.8f, 0.75f, 0.9f); // Crema suave
                subtitleTxt.alignment = TextAlignmentOptions.Center;
            }

            Transform sepLine = headerTrans.Find("SeparatorLine");
            if (sepLine != null)
            {
                RectTransform sepRect = sepLine.GetComponent<RectTransform>();
                if (sepRect != null)
                {
                    Undo.RecordObject(sepRect, "Adjust Separator Rect");
                    sepRect.anchorMin = new Vector2(0.25f, 0.05f);
                    sepRect.anchorMax = new Vector2(0.75f, 0.08f);
                    sepRect.offsetMin = Vector2.zero;
                    sepRect.offsetMax = Vector2.zero;
                }
                Image sepImg = sepLine.GetComponent<Image>();
                if (sepImg != null)
                {
                    Undo.RecordObject(sepImg, "Adjust Separator Color");
                    sepImg.color = new Color(0.55f, 0.12f, 0.12f, 0.7f); // Rojo sangre traslúcido
                }
            }
        }

        // 7. Cuerpo del diálogo (Body)
        Transform bodyTrans = controlsContainer.transform.Find("Body");
        if (bodyTrans != null)
        {
            RectTransform bodyRect = bodyTrans.GetComponent<RectTransform>();
            if (bodyRect != null)
            {
                Undo.RecordObject(bodyRect, "Adjust Body Rect");
                bodyRect.anchorMin = new Vector2(0f, 0.22f); 
                bodyRect.anchorMax = new Vector2(1f, 0.78f);
                bodyRect.offsetMin = Vector2.zero;
                bodyRect.offsetMax = Vector2.zero;
            }

            // 7a. Columna Izquierda (Controles teclado)
            Transform leftColTrans = bodyTrans.Find("LeftColumn");
            if (leftColTrans != null)
            {
                RectTransform leftColRect = leftColTrans.GetComponent<RectTransform>();
                if (leftColRect != null)
                {
                    Undo.RecordObject(leftColRect, "Adjust LeftColumn Rect");
                    leftColRect.anchorMin = new Vector2(0.05f, 0f); 
                    leftColRect.anchorMax = new Vector2(0.60f, 1f);
                    leftColRect.offsetMin = Vector2.zero;
                    leftColRect.offsetMax = Vector2.zero;
                }

                VerticalLayoutGroup vlg = leftColTrans.GetComponent<VerticalLayoutGroup>();
                if (vlg != null)
                {
                    Undo.RecordObject(vlg, "Adjust Vertical Layout Group");
                    vlg.spacing = 10; // Espaciado perfecto
                    vlg.padding = new RectOffset(10, 10, 10, 10);
                }

                // Configurar cada fila de control con su sprite correspondiente y estructura WASD / 3D
                ConfigureControlRow(leftColTrans, "Row_Movement", "Assets/Sprites/letreros/controls/movement_icon.png", titleFont, bodyFont);
                ConfigureControlRow(leftColTrans, "Row_Jump", "Assets/Sprites/letreros/controls/jump_icon.png", titleFont, bodyFont);
                ConfigureControlRow(leftColTrans, "Row_Crouch", "Assets/Sprites/letreros/controls/crouch_icon.png", titleFont, bodyFont);
                ConfigureControlRow(leftColTrans, "Row_Interact", "Assets/Sprites/letreros/controls/interact_icon.png", titleFont, bodyFont);
                ConfigureControlRow(leftColTrans, "Row_Inventory", "Assets/Sprites/letreros/controls/inventory_icon.png", titleFont, bodyFont);
                ConfigureControlRow(leftColTrans, "Row_Run", "Assets/Sprites/letreros/controls/run_icon.png", titleFont, bodyFont);
            }

            // 7b. Columna Derecha (Mouse Girar)
            Transform rightColTrans = bodyTrans.Find("RightColumn");
            if (rightColTrans != null)
            {
                RectTransform rightColRect = rightColTrans.GetComponent<RectTransform>();
                if (rightColRect != null)
                {
                    Undo.RecordObject(rightColRect, "Adjust RightColumn Rect");
                    rightColRect.anchorMin = new Vector2(0.64f, 0f); 
                    rightColRect.anchorMax = new Vector2(0.95f, 1f);
                    rightColRect.offsetMin = Vector2.zero;
                    rightColRect.offsetMax = Vector2.zero;
                }

                TMP_Text mouseTitle = rightColTrans.Find("MouseTitle")?.GetComponent<TMP_Text>();
                if (mouseTitle != null)
                {
                    Undo.RecordObject(mouseTitle, "Update Mouse Title");
                    if (titleFont != null) mouseTitle.font = titleFont;
                    mouseTitle.fontSize = 28;
                    mouseTitle.color = new Color(0.85f, 0.2f, 0.2f, 1f); 
                    mouseTitle.fontStyle = FontStyles.Bold;
                    mouseTitle.alignment = TextAlignmentOptions.Center;
                }

                TMP_Text mouseSubtitle = rightColTrans.Find("MouseSubtitle")?.GetComponent<TMP_Text>();
                if (mouseSubtitle != null)
                {
                    Undo.RecordObject(mouseSubtitle, "Update Mouse Subtitle");
                    if (titleFont != null) mouseSubtitle.font = titleFont;
                    mouseSubtitle.fontSize = 22;
                    mouseSubtitle.color = Color.white;
                    mouseSubtitle.alignment = TextAlignmentOptions.Center;
                }

                TMP_Text mouseDesc = rightColTrans.Find("MouseDescription")?.GetComponent<TMP_Text>();
                if (mouseDesc != null)
                {
                    Undo.RecordObject(mouseDesc, "Update Mouse Description");
                    if (bodyFont != null) mouseDesc.font = bodyFont;
                    mouseDesc.fontSize = 18;
                    mouseDesc.color = new Color(0.85f, 0.85f, 0.8f, 0.9f);
                    mouseDesc.alignment = TextAlignmentOptions.Center;
                }

                Transform mouseIllustration = rightColTrans.Find("MouseIllustration");
                if (mouseIllustration != null)
                {
                    RectTransform mouseIllRect = mouseIllustration.GetComponent<RectTransform>();
                    if (mouseIllRect != null)
                    {
                        Undo.RecordObject(mouseIllRect, "Adjust Mouse Illustration Rect");
                        mouseIllRect.anchorMin = new Vector2(0.5f, 0.32f);
                        mouseIllRect.anchorMax = new Vector2(0.5f, 0.32f);
                        mouseIllRect.pivot = new Vector2(0.5f, 0.5f);
                        mouseIllRect.anchoredPosition = Vector2.zero;
                        mouseIllRect.sizeDelta = new Vector2(180, 180);
                    }

                    Image mouseIllImg = mouseIllustration.GetComponent<Image>();
                    if (mouseIllImg != null)
                    {
                        Undo.RecordObject(mouseIllImg, "Adjust Mouse Illustration Background");
                        mouseIllImg.color = new Color(0.55f, 0.12f, 0.12f, 0.15f); // Brillo rojo sangre inmersivo
                    }

                    Transform mouseIcon = mouseIllustration.Find("MouseIcon");
                    if (mouseIcon != null)
                    {
                        RectTransform mouseIconRect = mouseIcon.GetComponent<RectTransform>();
                        if (mouseIconRect != null)
                        {
                            Undo.RecordObject(mouseIconRect, "Adjust Mouse Icon Size");
                            mouseIconRect.anchorMin = new Vector2(0.5f, 0.5f);
                            mouseIconRect.anchorMax = new Vector2(0.5f, 0.5f);
                            mouseIconRect.pivot = new Vector2(0.5f, 0.5f);
                            mouseIconRect.anchoredPosition = Vector2.zero;
                            mouseIconRect.sizeDelta = new Vector2(100, 140);
                        }

                        Image mouseIconImg = mouseIcon.GetComponent<Image>();
                        Sprite mouseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/letreros/controls/mouse.png");
                        if (mouseIconImg != null && mouseSprite != null)
                        {
                            Undo.RecordObject(mouseIconImg, "Set Mouse Sprite");
                            mouseIconImg.sprite = mouseSprite;
                            mouseIconImg.color = Color.white;
                            mouseIconImg.preserveAspect = true;
                        }
                    }
                }
            }
        }

        // 8. Footer (Advertencia y Decoración)
        Transform footerTrans = controlsContainer.transform.Find("Footer");
        if (footerTrans != null)
        {
            RectTransform footerRect = footerTrans.GetComponent<RectTransform>();
            if (footerRect != null)
            {
                Undo.RecordObject(footerRect, "Adjust Footer Rect");
                footerRect.anchorMin = new Vector2(0.05f, 0.11f); 
                footerRect.anchorMax = new Vector2(0.95f, 0.21f);
                footerRect.offsetMin = Vector2.zero;
                footerRect.offsetMax = Vector2.zero;
            }

            Image footerBg = footerTrans.GetComponent<Image>();
            if (footerBg != null)
            {
                Undo.RecordObject(footerBg, "Adjust Footer Background Color");
                footerBg.color = new Color(0.04f, 0.04f, 0.04f, 0.85f); 
            }

            Outline footerOutline = footerTrans.GetComponent<Outline>();
            if (footerOutline != null)
            {
                Undo.RecordObject(footerOutline, "Disable Footer Outline");
                footerOutline.enabled = false; 
            }

            Transform warningIcon = footerTrans.Find("WarningIcon");
            if (warningIcon != null)
            {
                RectTransform warningIconRect = warningIcon.GetComponent<RectTransform>();
                if (warningIconRect != null)
                {
                    Undo.RecordObject(warningIconRect, "Adjust Warning Icon Rect");
                    warningIconRect.anchorMin = new Vector2(0.02f, 0.5f);
                    warningIconRect.anchorMax = new Vector2(0.02f, 0.5f);
                    warningIconRect.pivot = new Vector2(0f, 0.5f);
                    warningIconRect.anchoredPosition = Vector2.zero;
                    warningIconRect.sizeDelta = new Vector2(35, 32);
                }

                Image warningIconImg = warningIcon.GetComponent<Image>();
                Sprite warningSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/letreros/warning.png");
                if (warningIconImg != null && warningSprite != null)
                {
                    Undo.RecordObject(warningIconImg, "Set Warning Icon Sprite");
                    warningIconImg.sprite = warningSprite;
                    warningIconImg.color = new Color(0.85f, 0.2f, 0.2f, 1f); 
                    warningIconImg.preserveAspect = true;
                }
            }

            TMP_Text warningTxt = footerTrans.Find("WarningText")?.GetComponent<TMP_Text>();
            if (warningTxt != null)
            {
                Undo.RecordObject(warningTxt, "Update Warning Text");
                if (bodyFont != null) warningTxt.font = bodyFont;
                warningTxt.fontSize = 20;
                warningTxt.color = new Color(0.85f, 0.85f, 0.8f, 0.95f);
                warningTxt.text = "Conoce tus controles. La práctica es la clave para <color=#E63333>sobrevivir</color>.";
            }

            Transform footerDecorImage = footerTrans.Find("FooterDecorImage");
            if (footerDecorImage != null)
            {
                RectTransform footerDecorRect = footerDecorImage.GetComponent<RectTransform>();
                if (footerDecorRect != null)
                {
                    Undo.RecordObject(footerDecorRect, "Adjust Footer Decor Rect");
                    footerDecorRect.anchorMin = new Vector2(0.75f, 0f);
                    footerDecorRect.anchorMax = new Vector2(1f, 1f);
                    footerDecorRect.offsetMin = Vector2.zero;
                    footerDecorRect.offsetMax = Vector2.zero;
                }

                Image footerDecorImg = footerDecorImage.GetComponent<Image>();
                Sprite footerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/letreros/footer.png");
                if (footerDecorImg != null && footerSprite != null)
                {
                    Undo.RecordObject(footerDecorImg, "Set Footer Decor Sprite");
                    footerDecorImg.sprite = footerSprite;
                    footerDecorImg.color = Color.white;
                    footerDecorImg.preserveAspect = true;
                }
            }
        }

        // 9. Botón de Aceptar (ActionButton) con Colores Interactivos Premium
        Transform actionBtnTrans = controlsContainer.transform.Find("ActionButton");
        if (actionBtnTrans != null)
        {
            RectTransform btnRect = actionBtnTrans.GetComponent<RectTransform>();
            if (btnRect != null)
            {
                Undo.RecordObject(btnRect, "Adjust Action Button Rect");
                btnRect.anchorMin = new Vector2(0.5f, 0f);
                btnRect.anchorMax = new Vector2(0.5f, 0f);
                btnRect.pivot = new Vector2(0.5f, 0f);
                btnRect.anchoredPosition = new Vector2(0, 22);
                btnRect.sizeDelta = new Vector2(260, 50);
            }

            Outline btnOutline = actionBtnTrans.GetComponent<Outline>();
            if (btnOutline != null)
            {
                Undo.RecordObject(btnOutline, "Disable Button Outline");
                btnOutline.enabled = false;
            }

            Image btnImg = actionBtnTrans.GetComponent<Image>();
            if (btnImg != null)
            {
                Undo.RecordObject(btnImg, "Adjust Button Color");
                btnImg.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                Sprite btnSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                if (btnSprite != null) btnImg.sprite = btnSprite;

                // Añadir un Outline sutil rojo apagado al botón
                Outline outlineComponent = actionBtnTrans.GetComponent<Outline>();
                if (outlineComponent == null)
                {
                    outlineComponent = Undo.AddComponent<Outline>(actionBtnTrans.gameObject);
                }
                outlineComponent.enabled = true;
                outlineComponent.effectColor = new Color(0.55f, 0.12f, 0.12f, 0.45f); // Borde rojo oscuro
                outlineComponent.effectDistance = new Vector2(1, -1);
            }

            Button btnComp = actionBtnTrans.GetComponent<Button>();
            if (btnComp != null)
            {
                Undo.RecordObject(btnComp, "Configure Button States");
                ColorBlock cb = btnComp.colors;
                cb.normalColor = new Color(0.1f, 0.1f, 0.1f, 1f);
                cb.highlightedColor = new Color(0.28f, 0.1f, 0.1f, 1f); // Rojo oscuro interactivo
                cb.pressedColor = new Color(0.42f, 0.12f, 0.12f, 1f); // Brillo rojo al presionar
                cb.selectedColor = cb.normalColor;
                cb.disabledColor = new Color(0.05f, 0.05f, 0.05f, 0.5f);
                btnComp.colors = cb;
            }

            TMP_Text btnTxt = actionBtnTrans.Find("ActionButtonText")?.GetComponent<TMP_Text>();
            if (btnTxt != null)
            {
                Undo.RecordObject(btnTxt, "Update Button Text");
                if (titleFont != null) btnTxt.font = titleFont;
                btnTxt.fontSize = 24;
                btnTxt.color = Color.white;
                btnTxt.alignment = TextAlignmentOptions.Center;
            }
        }

        // 10. Forzar re-vinculación en el trigger del ControlsPopupTrigger y en ControlsPopupPanel
        GameObject popupPanelObj = FindGameObjectInScene("PopUpPanel");
        if (popupPanelObj != null)
        {
            ControlsPopupPanel panelScript = popupPanelObj.GetComponent<ControlsPopupPanel>();
            if (panelScript != null)
            {
                Undo.RecordObject(panelScript, "Re-bind Panel Script References");
                Transform leftColTrans = controlsContainer.transform.Find("Body/LeftColumn");
                if (leftColTrans != null)
                {
                    panelScript.MovementIconImg = leftColTrans.Find("Row_Movement/Icon")?.GetComponent<Image>();
                    panelScript.JumpIconImg = leftColTrans.Find("Row_Jump/Icon")?.GetComponent<Image>();
                    panelScript.CrouchIconImg = leftColTrans.Find("Row_Crouch/Icon")?.GetComponent<Image>();
                    panelScript.InteractIconImg = leftColTrans.Find("Row_Interact/Icon")?.GetComponent<Image>();
                    panelScript.InventoryIconImg = leftColTrans.Find("Row_Inventory/Icon")?.GetComponent<Image>();
                    panelScript.RunIconImg = leftColTrans.Find("Row_Run/Icon")?.GetComponent<Image>();
                }
                
                Transform mouseIconTrans = controlsContainer.transform.Find("Body/RightColumn/MouseIllustration/MouseIcon");
                if (mouseIconTrans != null)
                {
                    panelScript.MouseImage = mouseIconTrans.GetComponent<Image>();
                }

                Transform footerDecorTrans = controlsContainer.transform.Find("Footer/FooterDecorImage");
                if (footerDecorTrans != null)
                {
                    panelScript.FooterDecorImage = footerDecorTrans.GetComponent<Image>();
                }

                Transform warningIconTrans = controlsContainer.transform.Find("Footer/WarningIcon");
                if (warningIconTrans != null)
                {
                    panelScript.WarningIconImg = warningIconTrans.GetComponent<Image>();
                }

                EditorUtility.SetDirty(panelScript);
            }
        }

        var trigger = GameObject.FindObjectOfType<ControlsPopupTrigger>(true);
        if (trigger != null)
        {
            Undo.RecordObject(trigger, "Re-bind Trigger Sprites");
            Sprite mouseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/letreros/controls/mouse.png");
            if (mouseSprite != null) trigger.MouseSprite = mouseSprite;
            
            Sprite footerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/letreros/footer.png");
            if (footerSprite != null) trigger.FooterDecorSprite = footerSprite;

            EditorUtility.SetDirty(trigger);
        }

        // Forzar actualización de UI en escena
        Canvas.ForceUpdateCanvases();
        
        // Marcar la escena como modificada
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[ImproveControlsPopup] Completed! All UI positions, sprites, and margins have been polished.");
    }

    private static void FixTextureImporter(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning($"[ImproveControlsPopup] Importer not found for path: {path}");
            return;
        }

        bool dirty = false;
        if (importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            dirty = true;
        }

        if (importer.alphaSource != TextureImporterAlphaSource.FromGrayScale)
        {
            importer.alphaSource = TextureImporterAlphaSource.FromGrayScale;
            dirty = true;
        }

        if (!importer.alphaIsTransparency)
        {
            importer.alphaIsTransparency = true;
            dirty = true;
        }

        if (dirty)
        {
            Undo.RecordObject(importer, $"Fix Importer for {path}");
            importer.SaveAndReimport();
            Debug.Log($"[ImproveControlsPopup] Fixed texture settings & reimported: {path}");
        }
    }

    private static GameObject FindGameObjectInScene(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null) return obj;

        var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in allObjects)
        {
            if (go.name == name && go.scene.isLoaded)
            {
                return go;
            }
        }
        return null;
    }

    private static void ConfigureControlRow(Transform leftColTrans, string rowName, string spritePath, TMP_FontAsset titleFont, TMP_FontAsset bodyFont)
    {
        Transform row = leftColTrans.Find(rowName);
        if (row == null)
        {
            Debug.LogWarning($"[ImproveControlsPopup] Row not found: {rowName}");
            return;
        }

        RectTransform rowRect = row.GetComponent<RectTransform>();
        if (rowRect != null)
        {
            Undo.RecordObject(rowRect, $"Adjust {rowName} size");
            rowRect.sizeDelta = new Vector2(700, 75); 
        }

        HorizontalLayoutGroup hlg = row.GetComponent<HorizontalLayoutGroup>();
        if (hlg != null)
        {
            Undo.RecordObject(hlg, $"Adjust {rowName} layout");
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleLeft;
        }

        // 1. Icono
        Transform iconTrans = row.Find("Icon");
        if (iconTrans != null)
        {
            RectTransform iconRect = iconTrans.GetComponent<RectTransform>();
            if (iconRect != null)
            {
                Undo.RecordObject(iconRect, $"Adjust {rowName} Icon Rect");
                iconRect.sizeDelta = new Vector2(40, 40);
            }

            Image iconImg = iconTrans.GetComponent<Image>();
            Sprite rowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (iconImg != null)
            {
                Undo.RecordObject(iconImg, $"Set {rowName} Sprite");
                if (rowSprite != null) iconImg.sprite = rowSprite;
                iconImg.color = new Color(0.9f, 0.85f, 0.75f, 0.9f); // Oro envejecido de horror
                iconImg.preserveAspect = true;
            }
        }

        // 2. Teclas con Relieve 3D Mecánico
        Transform keysContainer = row.Find("KeysContainer");
        if (keysContainer != null)
        {
            RectTransform keysContRect = keysContainer.GetComponent<RectTransform>();
            
            // Destruir teclas anteriores para recrearlas limpias en 3D
            var childKeys = new System.Collections.Generic.List<Transform>();
            foreach (Transform child in keysContainer)
            {
                childKeys.Add(child);
            }
            foreach (var child in childKeys)
            {
                Undo.DestroyObjectImmediate(child.gameObject);
            }

            if (rowName == "Row_Movement")
            {
                HorizontalLayoutGroup keysHlg = keysContainer.GetComponent<HorizontalLayoutGroup>();
                if (keysHlg != null)
                {
                    Undo.DestroyObjectImmediate(keysHlg);
                }

                if (keysContRect != null)
                {
                    Undo.RecordObject(keysContRect, "Adjust Movement KeysContainer");
                    keysContRect.sizeDelta = new Vector2(116, 76);
                }

                // Cruz WASD Estilizada
                Create3DKey(keysContainer, "Key_W", "W", new Vector2(0, 19), new Vector2(34, 34), titleFont);
                Create3DKey(keysContainer, "Key_A", "A", new Vector2(-38, -19), new Vector2(34, 34), titleFont);
                Create3DKey(keysContainer, "Key_S", "S", new Vector2(0, -19), new Vector2(34, 34), titleFont);
                Create3DKey(keysContainer, "Key_D", "D", new Vector2(38, -19), new Vector2(34, 34), titleFont);
            }
            else
            {
                HorizontalLayoutGroup keysHlg = keysContainer.GetComponent<HorizontalLayoutGroup>();
                if (keysHlg == null)
                {
                    keysHlg = Undo.AddComponent<HorizontalLayoutGroup>(keysContainer.gameObject);
                }
                keysHlg.spacing = 8;
                keysHlg.childControlWidth = false;
                keysHlg.childControlHeight = false;
                keysHlg.childForceExpandWidth = false;
                keysHlg.childForceExpandHeight = false;
                keysHlg.childAlignment = TextAnchor.MiddleLeft;

                float keyWidth = 40;
                string keyName = "E";

                if (rowName == "Row_Jump") { keyWidth = 110; keyName = "SPACE"; }
                else if (rowName == "Row_Crouch") { keyWidth = 70; keyName = "CTRL"; }
                else if (rowName == "Row_Interact") { keyWidth = 40; keyName = "E"; }
                else if (rowName == "Row_Inventory") { keyWidth = 60; keyName = "TAB"; }
                else if (rowName == "Row_Run") { keyWidth = 70; keyName = "SHIFT"; }

                if (keysContRect != null)
                {
                    Undo.RecordObject(keysContRect, $"Adjust {rowName} KeysContainer");
                    keysContRect.sizeDelta = new Vector2(keyWidth, 40);
                }

                Create3DKey(keysContainer, "Key_" + keyName, keyName, Vector2.zero, new Vector2(keyWidth, 40), titleFont);
            }
        }

        // 3. Textos (TextBlock)
        Transform textBlock = row.Find("TextBlock");
        if (textBlock != null)
        {
            RectTransform txtBlockRect = textBlock.GetComponent<RectTransform>();
            if (txtBlockRect != null)
            {
                Undo.RecordObject(txtBlockRect, $"Adjust {rowName} TextBlock Rect");
                txtBlockRect.sizeDelta = new Vector2(400, 55);
            }

            TMP_Text rowTitle = textBlock.Find("Title")?.GetComponent<TMP_Text>();
            if (rowTitle != null)
            {
                Undo.RecordObject(rowTitle, $"Update {rowName} Title Style");
                if (titleFont != null) rowTitle.font = titleFont;
                rowTitle.fontSize = 24;
                rowTitle.color = new Color(0.85f, 0.2f, 0.2f, 1f); 
                rowTitle.fontStyle = FontStyles.Bold;
            }

            TMP_Text rowDesc = textBlock.Find("Description")?.GetComponent<TMP_Text>();
            if (rowDesc != null)
            {
                Undo.RecordObject(rowDesc, $"Update {rowName} Description Style");
                if (bodyFont != null) rowDesc.font = bodyFont;
                rowDesc.fontSize = 18;
                rowDesc.color = new Color(0.85f, 0.85f, 0.8f, 0.9f); 
            }
        }
    }

    private static void Create3DKey(Transform parent, string goName, string keyName, Vector2 position, Vector2 size, TMP_FontAsset titleFont)
    {
        // 1. Sombra / Base 3D
        GameObject keyBase = new GameObject(goName, typeof(RectTransform), typeof(Image));
        keyBase.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(keyBase, $"Create {goName}");

        RectTransform baseRect = keyBase.GetComponent<RectTransform>();
        baseRect.sizeDelta = size;
        baseRect.anchoredPosition = position;

        Image baseImg = keyBase.GetComponent<Image>();
        baseImg.color = new Color(0.02f, 0.02f, 0.02f, 0.85f); 
        Sprite keySprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/letreros/controls/crouch_icon.png"); // Usamos crouch_icon temporalmente como sprite si es apto, o el UISprite por defecto
        Sprite uisprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (uisprite != null) baseImg.sprite = uisprite;

        // 2. Capa Frontal Elevada (KeyCap)
        GameObject keyCap = new GameObject("KeyCap", typeof(RectTransform), typeof(Image));
        keyCap.transform.SetParent(keyBase.transform, false);
        Undo.RegisterCreatedObjectUndo(keyCap, $"Create {goName} KeyCap");

        RectTransform capRect = keyCap.GetComponent<RectTransform>();
        capRect.anchorMin = Vector2.zero;
        capRect.anchorMax = Vector2.one;
        capRect.offsetMin = new Vector2(0, 3); // Relieve 3 px
        capRect.offsetMax = new Vector2(0, 3);

        Image capImg = keyCap.GetComponent<Image>();
        capImg.color = new Color(0.13f, 0.13f, 0.13f, 0.98f); 
        if (uisprite != null) capImg.sprite = uisprite;

        Outline outline = keyCap.AddComponent<Outline>();
        outline.effectColor = new Color(0.5f, 0.45f, 0.4f, 0.35f); // Oro/Crema apagado
        outline.effectDistance = new Vector2(1, -1);

        // 3. Texto de la Tecla
        GameObject keyTextObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        keyTextObj.transform.SetParent(keyCap.transform, false);
        Undo.RegisterCreatedObjectUndo(keyTextObj, $"Create {goName} Text");

        RectTransform textRect = keyTextObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI textTMP = keyTextObj.GetComponent<TextMeshProUGUI>();
        textTMP.text = keyName;
        textTMP.fontSize = size.x > 40 ? 18 : 20;
        textTMP.fontStyle = FontStyles.Bold;
        textTMP.alignment = TextAlignmentOptions.Center;
        textTMP.color = new Color(0.9f, 0.85f, 0.8f, 1f); 
        if (titleFont != null) textTMP.font = titleFont;
    }
}
