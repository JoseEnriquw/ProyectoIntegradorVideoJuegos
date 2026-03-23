using UnityEngine;
using TMPro;
using System.Collections;

namespace FuncionalidadesCore.UI
{
    /// <summary>
    /// Gestor central de la Interfaz de Usuario Genérica (Retícula, Textos de Interacción, Subtítulos).
    /// Colócalo en la raíz de tu Canvas principal.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Retícula e Interacción")]
        public GameObject ReticleDot;
        [Tooltip("El texto que dice 'Pulsa E para interactuar'")]
        public TextMeshProUGUI InteractionPromptText;
        [Tooltip("Si usas un panel con fondo negro translúcido para que el texto resalte")]
        public GameObject InteractionPromptBackground;

        [Header("Mensajes y Subtítulos")]
        public TextMeshProUGUI HintHintText;
        public float DefaultHintDuration = 3f;

        private Coroutine hintCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Iniciar con todo oculto
            HideInteractionPrompt();
            if (HintHintText != null) HintHintText.text = "";
        }

        // ============================================
        // INTERACCIÓN (Conectar a InteractionDetector)
        // ============================================

        /// <summary>Muestra el texto de interacción (Ej: "Recoger Poción")</summary>
        public void ShowInteractionPrompt(string text)
        {
            if (InteractionPromptText != null)
            {
                InteractionPromptText.text = text;
                InteractionPromptText.gameObject.SetActive(true);
            }
            
            if (InteractionPromptBackground != null)
                InteractionPromptBackground.SetActive(true);

            // Opcional: Expandir o cambiar color de la retícula
            if (ReticleDot != null)
                ReticleDot.transform.localScale = Vector3.one * 1.5f;
        }

        /// <summary>Oculta el texto de interacción cuando dejas de mirar objeto</summary>
        public void HideInteractionPrompt()
        {
            if (InteractionPromptText != null)
                InteractionPromptText.gameObject.SetActive(false);
                
            if (InteractionPromptBackground != null)
                InteractionPromptBackground.SetActive(false);

            if (ReticleDot != null)
                ReticleDot.transform.localScale = Vector3.one;
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
