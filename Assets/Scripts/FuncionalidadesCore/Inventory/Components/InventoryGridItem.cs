using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FuncionalidadesCore.Inventory.UI
{
    /// <summary>
    /// Componente que va en el PREFAB visual para el inventario de estilo Grid NxM.
    /// Funciona en conjunto con InventoryGridUI.
    /// </summary>
    public class InventoryGridItem : MonoBehaviour
    {
        public Image BackgroundImage; // Opcional, para pintar un cuadrito al item (ej: color gris translúcido)
        public Image IconImage;
        public TextMeshProUGUI QuantityText;
        public Button ActionButton; // Botón para clickear usar

        private string itemGUID;
        private InventoryGridUI parentGrid;

        public void Setup(string guid, Sprite icon, int quantity, InventoryGridUI grid)
        {
            itemGUID = guid;
            parentGrid = grid;
            
            if (IconImage != null && icon != null)
            {
                IconImage.sprite = icon;
                IconImage.preserveAspect = true; // IMPORTANTÍSIMO: Asegura que la linterna no se deforme
                IconImage.enabled = true;
            }
            
            UpdateQuantity(quantity);

            if (ActionButton != null)
            {
                ActionButton.onClick.RemoveAllListeners();
                ActionButton.onClick.AddListener(OnItemClicked);
            }
        }

        public void UpdateQuantity(int quantity)
        {
            if (QuantityText != null)
            {
                QuantityText.text = quantity > 1 ? quantity.ToString() : "";
            }
        }

        private void OnItemClicked()
        {
            parentGrid.RequestUseItem(itemGUID);
            // Aquí en un futuro puedes agregar click derecho, arrastrar, examinar, etc.
        }
    }
}
