using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // Necesario para detectar el mouse encima

namespace FuncionalidadesCore.Inventory.UI
{
    /// <summary>
    /// Componente que va en el PREFAB visual para el inventario de estilo Grid NxM.
    /// Funciona en conjunto con InventoryGridUI.
    /// </summary>
    public class InventoryGridItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Image BackgroundImage;
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
            HideHoverDetails(); // Limpiamos texto al usar el ítem (por si desaparece)
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (parentGrid != null) parentGrid.ShowItemDetails(itemGUID);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideHoverDetails();
        }

        private void HideHoverDetails()
        {
            if (parentGrid != null) parentGrid.HideItemDetails();
        }
    }
}
