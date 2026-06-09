using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class ControlsPopupPanel : Singleton<ControlsPopupPanel>
    {
        [Header("Referencias de UI - General")]
        [Tooltip("El Canvas Group que controla la opacidad e interacción del panel de controles.")]
        public CanvasGroup PanelCanvasGroup;

        [Tooltip("El contenedor visual de este diálogo (ControlsDialogContainer).")]
        public GameObject DialogContainer;

        [Tooltip("Texto del título principal.")]
        public TMP_Text TitleText;

        [Tooltip("Texto del subtítulo principal.")]
        public TMP_Text SubtitleText;

        [Header("Referencias de UI - Controles")]
        public TMP_Text MovementTitle;
        public TMP_Text MovementDesc;
        
        public TMP_Text JumpTitle;
        public TMP_Text JumpDesc;
        
        public TMP_Text CrouchTitle;
        public TMP_Text CrouchDesc;
        
        public TMP_Text InteractTitle;
        public TMP_Text InteractDesc;
        
        public TMP_Text InventoryTitle;
        public TMP_Text InventoryDesc;
        
        public TMP_Text RunTitle;
        public TMP_Text RunDesc;

        [Header("Referencias de UI - Iconos de Teclado")]
        public Image MovementIconImg;
        public Image JumpIconImg;
        public Image CrouchIconImg;
        public Image InteractIconImg;
        public Image InventoryIconImg;
        public Image RunIconImg;

        [Header("Referencias de UI - Mouse")]
        public TMP_Text MouseTitle;
        public TMP_Text MouseSubtitle;
        public TMP_Text MouseDesc;
        
        [Tooltip("La imagen de ilustración del ratón.")]
        public Image MouseImage;

        [Header("Referencias de UI - Pie de Página")]
        public TMP_Text WarningText;
        public Image WarningIconImg;
        
        [Tooltip("La imagen horizontal decorativa del pie de página.")]
        public Image FooterDecorImage;

        [Header("Referencias de UI - Botón")]
        public Button ActionButton;
        public TMP_Text ActionButtonText;

        [Header("Ajustes de Fading")]
        [Tooltip("Velocidad de transición al mostrar u ocultar el panel.")]
        public float FadeSpeed = 5f;

        private Action onDismissCallback;
        
        /// <summary>
        /// Indica si el panel de controles está actualmente visible en pantalla.
        /// </summary>
        public bool IsShown { get; private set; }

        private void Awake()
        {
            if (PanelCanvasGroup != null)
            {
                PanelCanvasGroup.alpha = 0f;
                PanelCanvasGroup.interactable = false;
                PanelCanvasGroup.blocksRaycasts = false;
            }
            if (DialogContainer != null)
            {
                DialogContainer.SetActive(false);
            }
        }

        /// <summary>
        /// Muestra el panel de controles con los textos y sprites configurados dinámicamente.
        /// </summary>
        public void Show(
            string title,
            string subtitle,
            string movementTitleVal, string movementDescVal, Sprite movementIcon,
            string jumpTitleVal, string jumpDescVal, Sprite jumpIcon,
            string crouchTitleVal, string crouchDescVal, Sprite crouchIcon,
            string interactTitleVal, string interactDescVal, Sprite interactIcon,
            string inventoryTitleVal, string inventoryDescVal, Sprite inventoryIcon,
            string runTitleVal, string runDescVal, Sprite runIcon,
            string mouseTitleVal, string mouseSubVal, string mouseDescVal, Sprite mouseSprite,
            string warningVal, Sprite warningIcon,
            Sprite footerDecorSprite,
            string buttonVal,
            Action onDismiss)
        {
            StopAllCoroutines();

            // Asegurar activación del contenedor de controles y desactivación del de información
            if (DialogContainer != null)
            {
                DialogContainer.SetActive(true);
            }
            Transform mainContainer = transform.Find("MainDialogContainer");
            if (mainContainer != null)
            {
                mainContainer.gameObject.SetActive(false);
            }

            // Configurar textos dinámicos
            if (TitleText != null) TitleText.text = title;
            if (SubtitleText != null) SubtitleText.text = subtitle;

            if (MovementTitle != null) MovementTitle.text = movementTitleVal;
            if (MovementDesc != null) MovementDesc.text = movementDescVal;

            if (JumpTitle != null) JumpTitle.text = jumpTitleVal;
            if (JumpDesc != null) JumpDesc.text = jumpDescVal;

            if (CrouchTitle != null) CrouchTitle.text = crouchTitleVal;
            if (CrouchDesc != null) CrouchDesc.text = crouchDescVal;

            if (InteractTitle != null) InteractTitle.text = interactTitleVal;
            if (InteractDesc != null) InteractDesc.text = interactDescVal;

            if (InventoryTitle != null) InventoryTitle.text = inventoryTitleVal;
            if (InventoryDesc != null) InventoryDesc.text = inventoryDescVal;

            if (RunTitle != null) RunTitle.text = runTitleVal;
            if (RunDesc != null) RunDesc.text = runDescVal;

            if (MouseTitle != null) MouseTitle.text = mouseTitleVal;
            if (MouseSubtitle != null) MouseSubtitle.text = mouseSubVal;
            if (MouseDesc != null) MouseDesc.text = mouseDescVal;

            if (WarningText != null) WarningText.text = warningVal;
            if (ActionButtonText != null) ActionButtonText.text = !string.IsNullOrEmpty(buttonVal) ? buttonVal : "ACEPTAR";

            // Configurar imágenes dinámicas (Iconos)
            if (MovementIconImg != null && movementIcon != null) MovementIconImg.sprite = movementIcon;
            if (JumpIconImg != null && jumpIcon != null) JumpIconImg.sprite = jumpIcon;
            if (CrouchIconImg != null && crouchIcon != null) CrouchIconImg.sprite = crouchIcon;
            if (InteractIconImg != null && interactIcon != null) InteractIconImg.sprite = interactIcon;
            if (InventoryIconImg != null && inventoryIcon != null) InventoryIconImg.sprite = inventoryIcon;
            if (RunIconImg != null && runIcon != null) RunIconImg.sprite = runIcon;
            if (WarningIconImg != null && warningIcon != null) WarningIconImg.sprite = warningIcon;

            if (MouseImage != null)
            {
                if (mouseSprite != null)
                {
                    MouseImage.sprite = mouseSprite;
                    MouseImage.gameObject.SetActive(true);
                }
                else
                {
                    MouseImage.gameObject.SetActive(false);
                }
            }

            if (FooterDecorImage != null)
            {
                if (footerDecorSprite != null)
                {
                    FooterDecorImage.sprite = footerDecorSprite;
                    FooterDecorImage.gameObject.SetActive(true);
                }
                else
                {
                    FooterDecorImage.gameObject.SetActive(false);
                }
            }

            // Asignar callbacks
            if (ActionButton != null)
            {
                ActionButton.onClick.RemoveAllListeners();
                ActionButton.onClick.AddListener(Dismiss);
            }

            onDismissCallback = onDismiss;
            IsShown = true;

            // Pausar juego y liberar cursor
            if (GameManager.Instance != null)
            {
                GameManager.Instance.FreezePlayer(true, true);
            }
            Time.timeScale = 0f;

            if (PanelCanvasGroup != null)
            {
                PanelCanvasGroup.gameObject.SetActive(true);
                PanelCanvasGroup.interactable = true;
                PanelCanvasGroup.blocksRaycasts = true;
                StartCoroutine(CanvasGroupFader.StartFade(PanelCanvasGroup, true, FadeSpeed));
            }
        }

        /// <summary>
        /// Oculta el panel y reanuda el juego.
        /// </summary>
        public void Dismiss()
        {
            if (!IsShown) return;
            IsShown = false;

            if (PanelCanvasGroup != null)
            {
                PanelCanvasGroup.interactable = false;
                PanelCanvasGroup.blocksRaycasts = false;
                StartCoroutine(CanvasGroupFader.StartFade(PanelCanvasGroup, false, FadeSpeed, () =>
                {
                    if (DialogContainer != null)
                    {
                        DialogContainer.SetActive(false);
                    }
                    ResumeGame();
                }));
            }
            else
            {
                ResumeGame();
            }
        }

        private void ResumeGame()
        {
            // Reanudar tiempo
            Time.timeScale = 1f;

            // Reactivar controles y ocultar cursor
            if (GameManager.Instance != null)
            {
                GameManager.Instance.FreezePlayer(false, false);
            }

            onDismissCallback?.Invoke();
            onDismissCallback = null;
        }
    }
}
