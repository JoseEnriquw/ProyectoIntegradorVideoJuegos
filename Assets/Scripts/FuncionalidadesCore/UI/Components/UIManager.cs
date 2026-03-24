using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

namespace FuncionalidadesCore.UI
{
    /// <summary>
    /// Gestor central de la Interfaz de Usuario Genérica (Retícula, Textos de Interacción, Subtítulos).
    /// Colócalo en la raíz de tu Canvas principal.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Contenedor Principal (Ocultable)")]
        [Tooltip("Mete tu ReticleImage y textos dentro de un Panel transparente y arrástralo aquí")]
        public GameObject HUDPanel;

        [Header("Retícula e Interacción")]
        public Image ReticleImage; // Importante: Arrastrar el componente Image en vez del GameObject
        public Sprite NormalReticle;
        public Sprite InteractReticle;
        public float NormalSize = 1f;
        public float InteractSize = 1.5f;
        public float ScaleSpeed = 15f;

        [Tooltip("El texto que dice 'Pulsa E para interactuar'")]
        public TextMeshProUGUI InteractionPromptText;
        [Tooltip("Si usas un panel con fondo negro translúcido para que el texto resalte")]
        public GameObject InteractionPromptBackground;

        [Header("Mensajes y Subtítulos")]
        public TextMeshProUGUI HintHintText;
        public float DefaultHintDuration = 3f;

        private Coroutine hintCoroutine;
        private float currentTargetScale;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Iniciar con todo oculto
            currentTargetScale = NormalSize;
            HideInteractionPrompt();
            if (HintHintText != null) HintHintText.text = "";
        }

        private void Update()
        {
            if (ReticleImage != null)
            {
                // Interpolar suavemente el tamaño (Lerp) para el efecto elástico
                float currentScale = ReticleImage.transform.localScale.x;
                float newScale = Mathf.Lerp(currentScale, currentTargetScale, Time.deltaTime * ScaleSpeed);
                ReticleImage.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }

        // ============================================
        // INTERACCIÓN (Conectar a InteractionDetector)
        // ============================================

        public void ShowInteractionPrompt(string text)
        {
            if (InteractionPromptText != null)
            {
                InteractionPromptText.text = text;
                InteractionPromptText.gameObject.SetActive(true);
            }
            
            if (InteractionPromptBackground != null)
                InteractionPromptBackground.SetActive(true);

            // Cambiar imagen a "Interactuable" y decirle que crezca suavemente
            currentTargetScale = InteractSize;
            if (ReticleImage != null && InteractReticle != null)
                ReticleImage.sprite = InteractReticle;
        }

        /// <summary>Oculta el texto de interacción cuando dejas de mirar objeto</summary>
        public void HideInteractionPrompt()
        {
            if (InteractionPromptText != null)
                InteractionPromptText.gameObject.SetActive(false);
                
            if (InteractionPromptBackground != null)
                InteractionPromptBackground.SetActive(false);

            // Cambiar imagen a "Normal" y decirle que encoja suavemente
            currentTargetScale = NormalSize;
            if (ReticleImage != null && NormalReticle != null)
                ReticleImage.sprite = NormalReticle;
        }

        public void ToggleHUD(bool show)
        {
            if (HUDPanel != null)
            {
                HUDPanel.SetActive(show);
            }
        }

        // ============================================
        // MENSAJES DEL SISTEMA
        // ============================================

        /// <summary>Muestra un mensaje en pantalla por X segundos</summary>
        public void ShowHintMessage(string message, float duration = 0f)
        {
            if (HintHintText == null) return;

            if (duration <= 0f) duration = DefaultHintDuration;

            if (hintCoroutine != null)
                StopCoroutine(hintCoroutine);

            hintCoroutine = StartCoroutine(HintCoroutine(message, duration));
        }

        private IEnumerator HintCoroutine(string message, float duration)
        {
            HintHintText.text = message;
            HintHintText.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(duration);
            
            HintHintText.gameObject.SetActive(false);
            HintHintText.text = "";
            hintCoroutine = null;
        }
    }
}
