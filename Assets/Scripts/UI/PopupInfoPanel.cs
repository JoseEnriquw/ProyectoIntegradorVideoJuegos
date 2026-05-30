using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    public class PopupInfoPanel : Singleton<PopupInfoPanel>
    {
        [Header("Referencias de UI")]
        [Tooltip("El Canvas Group que controla la opacidad y la interactividad de todo el panel.")]
        public CanvasGroup PanelCanvasGroup;

        [Tooltip("Texto para el título del cartel (ej. GUARDAR PARTIDA).")]
        public TMP_Text TitleText;

        [Tooltip("Icono circular decorativo/temático en la columna izquierda.")]
        public Image TopicIcon;

        [Tooltip("Texto principal de descripción o instrucciones.")]
        public TMP_Text DescriptionText;

        [Header("Contenedor de Atajo/Tecla (Opcional)")]
        [Tooltip("Contenedor que agrupa la visualización de la tecla.")]
        public GameObject KeyPromptContainer;

        [Tooltip("Texto del atajo de teclado (ej. '[E] para guardar.').")]
        public TMP_Text KeyPromptText;

        [Tooltip("La imagen que simula el fondo de la tecla (ej. el cuadrado para la E).")]
        public Image KeyIconImage;

        [Tooltip("El texto dentro del icono de la tecla (ej. 'E').")]
        public TMP_Text KeyIconText;

        [Header("Contenedor de Advertencia (Opcional)")]
        [Tooltip("Contenedor para la advertencia/consejo en la parte inferior.")]
        public GameObject WarningContainer;

        [Tooltip("Texto de la advertencia (ej. 'Guarda con frecuencia...').")]
        public TMP_Text WarningText;

        [Header("Ilustración de Vista Previa")]
        [Tooltip("La imagen de vista previa en la columna derecha.")]
        public Image PreviewImage;

        [Header("Botón de Acción")]
        [Tooltip("El botón para continuar / cerrar el panel.")]
        public Button ActionButton;

        [Tooltip("Texto dentro del botón de continuar.")]
        public TMP_Text ActionButtonText;

        [Header("Ajustes de Fading")]
        [Tooltip("Velocidad de transición al mostrar / ocultar el panel.")]
        public float FadeSpeed = 5f;

        private Action onDismissCallback;
        
        /// <summary>
        /// Indica si el panel está actualmente visible en pantalla.
        /// </summary>
        public bool IsShown { get; private set; }

        private void Awake()
        {
            if (PanelCanvasGroup != null)
            {
                PanelCanvasGroup.alpha = 0f;
                PanelCanvasGroup.interactable = false;
                PanelCanvasGroup.blocksRaycasts = false;
                if (!IsShown)
                {
                    PanelCanvasGroup.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Muestra el panel con los datos configurados.
        /// </summary>
        public void Show(
            string title, 
            Sprite icon, 
            string description, 
            string keyName,     // Nueva sobrecarga: Permite pasar la tecla directamente (ej. "E")
            string keyPrompt, 
            string warning, 
            Sprite preview, 
            string buttonLabel, 
            Action onDismiss)
        {
            StopAllCoroutines();

            // Configurar Título
            if (TitleText != null)
            {
                TitleText.text = title;
                TitleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
            }

            // Configurar Icono de Tema
            if (TopicIcon != null)
            {
                if (icon != null)
                {
                    TopicIcon.sprite = icon;
                    TopicIcon.gameObject.SetActive(true);
                }
                else
                {
                    TopicIcon.gameObject.SetActive(false);
                }
            }

            // Configurar Descripción
            if (DescriptionText != null)
            {
                DescriptionText.text = description;
                DescriptionText.gameObject.SetActive(!string.IsNullOrEmpty(description));
            }

            // Configurar Atajo de Tecla
            if (KeyPromptContainer != null)
            {
                if (!string.IsNullOrEmpty(keyPrompt) || !string.IsNullOrEmpty(keyName))
                {
                    string finalKeyName = keyName;
                    string finalKeyAction = keyPrompt;

                    // Si no se pasó una tecla explícita, intentar extraerla de la descripción (si tiene corchetes)
                    if (string.IsNullOrEmpty(finalKeyName) && keyPrompt.StartsWith("[") && keyPrompt.Contains("]"))
                    {
                        int closeBracketIndex = keyPrompt.IndexOf("]");
                        finalKeyName = keyPrompt.Substring(1, closeBracketIndex - 1);
                        finalKeyAction = keyPrompt.Substring(closeBracketIndex + 1).Trim();
                    }

                    // Configurar el icono de la tecla si se detectó una tecla válida
                    if (!string.IsNullOrEmpty(finalKeyName) && KeyIconText != null)
                    {
                        KeyIconText.text = finalKeyName;
                        if (KeyIconImage != null) KeyIconImage.gameObject.SetActive(true);
                    }
                    else
                    {
                        if (KeyIconImage != null) KeyIconImage.gameObject.SetActive(false);
                    }

                    // Configurar el texto descriptivo de la acción
                    if (KeyPromptText != null)
                    {
                        KeyPromptText.text = finalKeyAction;
                    }

                    KeyPromptContainer.SetActive(true);
                }
                else
                {
                    KeyPromptContainer.SetActive(false);
                }
            }

            // Configurar Advertencia
            if (WarningContainer != null)
            {
                if (!string.IsNullOrEmpty(warning) && WarningText != null)
                {
                    WarningText.text = warning;
                    WarningContainer.SetActive(true);
                }
                else
                {
                    WarningContainer.SetActive(false);
                }
            }

            // Configurar Imagen de Vista Previa (Columna Derecha)
            if (PreviewImage != null)
            {
                if (preview != null)
                {
                    PreviewImage.sprite = preview;
                    PreviewImage.gameObject.SetActive(true);
                }
                else
                {
                    PreviewImage.gameObject.SetActive(false);
                }
            }

            // Configurar Botón de Acción
            if (ActionButton != null)
            {
                ActionButton.onClick.RemoveAllListeners();
                ActionButton.onClick.AddListener(Dismiss);

                if (ActionButtonText != null)
                {
                    ActionButtonText.text = !string.IsNullOrEmpty(buttonLabel) ? buttonLabel : "ACEPTAR";
                }
            }

            onDismissCallback = onDismiss;
            IsShown = true;

            // Congelar juego y habilitar cursor
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
        /// Oculta el panel y ejecuta el callback de finalización.
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
                    PanelCanvasGroup.gameObject.SetActive(false);
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
            // Despausar tiempo
            Time.timeScale = 1f;

            // Descongelar jugador y ocultar cursor
            if (GameManager.Instance != null)
            {
                GameManager.Instance.FreezePlayer(false, false);
            }

            // Invocar callback
            onDismissCallback?.Invoke();
            onDismissCallback = null;
        }
    }
}
