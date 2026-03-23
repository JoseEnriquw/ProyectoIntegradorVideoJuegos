using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FuncionalidadesCore.Inventory.UI
{
    /// <summary>
    /// Componente que va en el PREFAB de tu Cuadro (Slot) del inventario.
    /// Contiene las referencias a los elementos visuales individuales.
    /// </summary>
    public class InventoryUISlot : MonoBehaviour
    {
        public Image IconImage;
        public TextMeshProUGUI QuantityText;
        public Button ActionButton;

        private string itemGUID;
        private InventoryUIPanel parentPanel;

        /// <summary>Inicializa visualmente el cuadrito.</summary>
        public void Setup(string guid, Sprite icon, int quantity, InventoryUIPanel panel)
        {
            itemGUID = guid;
            parentPanel = panel;
            
            // Configurar imagen
            if (IconImage != null && icon != null)
            {
                IconImage.sprite = icon;
                IconImage.enabled = true;
            }
            
            // Configurar texto de cantidad
            UpdateQuantity(quantity);

            // Acción del botón (ej: al hacer click, usar el item)
            if (ActionButton != null)
            {
                ActionButton.onClick.RemoveAllListeners();
                ActionButton.onClick.AddListener(OnSlotClicked);
            }
        }

        public void UpdateQuantity(int quantity)
        {
            if (QuantityText != null)
            {
                QuantityText.text = quantity > 1 ? quantity.ToString() : "";
            }
        }

        private void OnSlotClicked()
        {
            // Le avisamos al Panel Padre que queremos usar este item
            parentPanel.RequestUseItem(itemGUID);
        }
    }
}
