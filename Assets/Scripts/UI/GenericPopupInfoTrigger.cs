using System;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Runtime;

namespace UHFPS.Runtime
{
    public class GenericPopupInfoTrigger : MonoBehaviour, IInteractStart
    {
        public enum TriggerTypeEnum { CollisionTrigger, DirectInteraction, CallOnly }

        [Header("Tipo de Activación")]
        [Tooltip("CollisionTrigger: Se activa cuando el jugador entra al área del collider.\nDirectInteraction: Se activa al mirar el objeto e interactuar (sistema UHFPS).\nCallOnly: Solo se activa llamando a TriggerPopup() por código o eventos.")]
        public TriggerTypeEnum TriggerType = TriggerTypeEnum.CollisionTrigger;

        [Tooltip("Indica si el popup solo debe mostrarse una única vez en el juego.")]
        public bool TriggerOnlyOnce = true;

        [Header("Contenido del Popup")]
        [Tooltip("El título localizado que se mostrará arriba (ej. GUARDAR PARTIDA).")]
        public GString PopupTitle;

        [Tooltip("Icono temático/circular decorativo para la columna izquierda.")]
        public Sprite TopicIcon;

        [Tooltip("Texto principal de descripción o instrucciones localizado.")]
        public GString DescriptionText;

        [Tooltip("Instrucción del atajo/tecla (ej. 'para esconderse / salir').")]
        public GString KeyPrompt;

        [Tooltip("La letra/tecla del atajo (ej. 'E'). Si se deja vacía, se puede extraer del prompt si se usan corchetes como [E].")]
        public string PopupKeyLetter = "E";

        [Tooltip("Texto de advertencia localizado para la esquina inferior izquierda.")]
        public GString WarningText;

        [Tooltip("Sprite de vista previa del objeto/mecánica para la columna derecha.")]
        public Sprite PreviewSprite;

        [Tooltip("El texto del botón de aceptar (localizado).")]
        public GString ButtonLabel = new GString("ACEPTAR");

        [Header("Eventos")]
        [Tooltip("Eventos que se ejecutarán cuando el jugador cierre el popup.")]
        public UnityEvent OnPopupDismissed;

        private bool hasTriggered = false;

        private void Start()
        {
            // Suscribir los textos a la base de datos de localización
            if (PopupTitle != null) PopupTitle.SubscribeGloc();
            if (DescriptionText != null) DescriptionText.SubscribeGloc();
            if (KeyPrompt != null) KeyPrompt.SubscribeGloc();
            if (WarningText != null) WarningText.SubscribeGloc();
            if (ButtonLabel != null) ButtonLabel.SubscribeGloc();

            // Configurar el collider para que sea trigger si se usa colisión
            Collider col = GetComponent<Collider>();
            if (TriggerType == TriggerTypeEnum.CollisionTrigger)
            {
                if (col != null)
                {
                    col.isTrigger = true;
                }
                else
                {
                    Debug.LogWarning($"[GenericPopupInfoTrigger] No se encontró un Collider en {gameObject.name}. El modo CollisionTrigger requiere un collider.");
                }
            }
        }

        // Activación por colisión (OnTriggerEnter)
        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType != TriggerTypeEnum.CollisionTrigger) return;
            if (hasTriggered && TriggerOnlyOnce) return;

            if (other.CompareTag("Player"))
            {
                TriggerPopup();
            }
        }

        // Activación por interacción (Sistema de Raycast de UHFPS)
        public void InteractStart()
        {
            if (TriggerType != TriggerTypeEnum.DirectInteraction) return;
            if (hasTriggered && TriggerOnlyOnce) return;

            TriggerPopup();
        }

        /// <summary>
        /// Muestra el popup con los textos y sprites configurados.
        /// </summary>
        public void TriggerPopup()
        {
            if (PopupInfoPanel.Instance == null)
            {
                Debug.LogWarning("[PopupInfoTrigger] No se encontró PopupInfoPanel.Instance en la escena activa.");
                return;
            }

            hasTriggered = true;

            string title = PopupTitle != null ? PopupTitle.Value : string.Empty;
            string desc = DescriptionText != null ? DescriptionText.Value : string.Empty;
            string keyLetter = PopupKeyLetter;
            string keyPromptVal = KeyPrompt != null ? KeyPrompt.Value : string.Empty;
            string warning = WarningText != null ? WarningText.Value : string.Empty;
            string btn = ButtonLabel != null ? ButtonLabel.Value : "ACEPTAR";

            PopupInfoPanel.Instance.Show(title, TopicIcon, desc, keyLetter, keyPromptVal, warning, PreviewSprite, btn, OnDismissed);
        }

        private void OnDismissed()
        {
            OnPopupDismissed?.Invoke();
        }

        // Método público para resetear el estado y permitir activarlo otra vez
        public void ResetTriggerState()
        {
            hasTriggered = false;
        }
    }
}
