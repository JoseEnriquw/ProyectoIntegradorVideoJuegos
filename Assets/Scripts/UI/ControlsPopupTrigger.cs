using System;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Tools;

namespace UHFPS.Runtime
{
    [RequireComponent(typeof(Collider))]
    public class ControlsPopupTrigger : MonoBehaviour, IInteractStart
    {
        public enum TriggerTypeEnum { CollisionTrigger, DirectInteraction, CallOnly }

        [Header("Tipo de Activación")]
        [Tooltip("CollisionTrigger: Al entrar en el collider.\nDirectInteraction: Al interactuar con el objeto usando raycast.\nCallOnly: Solo se abre llamando a TriggerPopup() por código.")]
        public TriggerTypeEnum TriggerType = TriggerTypeEnum.CollisionTrigger;

        [Tooltip("Indica si el panel de controles solo debe mostrarse una vez.")]
        public bool TriggerOnlyOnce = true;

        [Header("Imágenes del Panel de Controles")]
        [Tooltip("El sprite que ilustrará al mouse de la derecha.")]
        public Sprite MouseSprite;

        [Tooltip("El sprite horizontal que se mostrará en el pie de página.")]
        public Sprite FooterDecorSprite;

        [Header("Iconos de Teclado y Advertencia")]
        public Sprite MovementIcon;
        public Sprite JumpIcon;
        public Sprite CrouchIcon;
        public Sprite InteractIcon;
        public Sprite InventoryIcon;
        public Sprite RunIcon;
        public Sprite WarningIcon;

        [Header("Configuración de Localización (Claves Gloc)")]
        public GString TitleGloc = new GString("popups.controls_title");
        public GString SubtitleGloc = new GString("popups.controls_subtitle");
        
        [Header("Gloc - Teclado")]
        public GString MovementTitleGloc = new GString("popups.controls_movement_title");
        public GString MovementDescGloc = new GString("popups.controls_movement_desc");
        
        public GString JumpTitleGloc = new GString("popups.controls_jump_title");
        public GString JumpDescGloc = new GString("popups.controls_jump_desc");
        
        public GString CrouchTitleGloc = new GString("popups.controls_crouch_title");
        public GString CrouchDescGloc = new GString("popups.controls_crouch_desc");
        
        public GString InteractTitleGloc = new GString("popups.controls_interact_title");
        public GString InteractDescGloc = new GString("popups.controls_interact_desc");
        
        public GString InventoryTitleGloc = new GString("popups.controls_inventory_title");
        public GString InventoryDescGloc = new GString("popups.controls_inventory_desc");
        
        public GString RunTitleGloc = new GString("popups.controls_run_title");
        public GString RunDescGloc = new GString("popups.controls_run_desc");
        
        [Header("Gloc - Mouse")]
        public GString MouseTitleGloc = new GString("popups.controls_mouse_title");
        public GString MouseSubtitleGloc = new GString("popups.controls_mouse_subtitle");
        public GString MouseDescGloc = new GString("popups.controls_mouse_desc");
        
        [Header("Gloc - Inferiores")]
        public GString WarningGloc = new GString("popups.controls_warning");
        public GString ButtonGloc = new GString("popups.controls_button");

        [Header("Eventos")]
        [Tooltip("Se ejecuta cuando el jugador presiona Aceptar y cierra el panel de controles.")]
        public UnityEvent OnPopupDismissed;

        private bool hasTriggered = false;

        private void Start()
        {
            // Suscribir los textos a la base de datos de localización al inicio
            TitleGloc.SubscribeGloc();
            SubtitleGloc.SubscribeGloc();
            
            MovementTitleGloc.SubscribeGloc();
            MovementDescGloc.SubscribeGloc();
            
            JumpTitleGloc.SubscribeGloc();
            JumpDescGloc.SubscribeGloc();
            
            CrouchTitleGloc.SubscribeGloc();
            CrouchDescGloc.SubscribeGloc();
            
            InteractTitleGloc.SubscribeGloc();
            InteractDescGloc.SubscribeGloc();
            
            InventoryTitleGloc.SubscribeGloc();
            InventoryDescGloc.SubscribeGloc();
            
            RunTitleGloc.SubscribeGloc();
            RunDescGloc.SubscribeGloc();
            
            MouseTitleGloc.SubscribeGloc();
            MouseSubtitleGloc.SubscribeGloc();
            MouseDescGloc.SubscribeGloc();
            
            WarningGloc.SubscribeGloc();
            ButtonGloc.SubscribeGloc();

            // Configurar el collider para que sea trigger si se usa colisión
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                if (TriggerType == TriggerTypeEnum.CollisionTrigger)
                {
                    col.isTrigger = true;
                }
            }
            else
            {
                Debug.LogWarning($"[ControlsPopupTrigger] No se encontró un Collider en {gameObject.name}. El modo CollisionTrigger requiere un collider.");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (TriggerType != TriggerTypeEnum.CollisionTrigger) return;
            if (hasTriggered && TriggerOnlyOnce) return;

            if (other.CompareTag("Player"))
            {
                TriggerPopup();
            }
        }

        // Sistema de interacción de UHFPS
        public void InteractStart()
        {
            if (TriggerType != TriggerTypeEnum.DirectInteraction) return;
            if (hasTriggered && TriggerOnlyOnce) return;

            TriggerPopup();
        }

        /// <summary>
        /// Muestra el panel de controles pasándole dinámicamente toda la configuración de textos e imágenes.
        /// </summary>
        public void TriggerPopup()
        {
            if (ControlsPopupPanel.Instance == null)
            {
                Debug.LogWarning("[ControlsPopupTrigger] No se encontró ControlsPopupPanel.Instance en la escena activa.");
                return;
            }

            hasTriggered = true;

            ControlsPopupPanel.Instance.Show(
                TitleGloc.Value,
                SubtitleGloc.Value,
                MovementTitleGloc.Value, MovementDescGloc.Value, MovementIcon,
                JumpTitleGloc.Value, JumpDescGloc.Value, JumpIcon,
                CrouchTitleGloc.Value, CrouchDescGloc.Value, CrouchIcon,
                InteractTitleGloc.Value, InteractDescGloc.Value, InteractIcon,
                InventoryTitleGloc.Value, InventoryDescGloc.Value, InventoryIcon,
                RunTitleGloc.Value, RunDescGloc.Value, RunIcon,
                MouseTitleGloc.Value, MouseSubtitleGloc.Value, MouseDescGloc.Value, MouseSprite,
                WarningGloc.Value, WarningIcon,
                FooterDecorSprite,
                ButtonGloc.Value,
                OnDismissed
            );
        }

        private void OnDismissed()
        {
            OnPopupDismissed?.Invoke();
        }

        /// <summary>
        /// Permite reactivar el trigger para que pueda volver a mostrarse en el juego.
        /// </summary>
        public void ResetTriggerState()
        {
            hasTriggered = false;
        }
    }
}
