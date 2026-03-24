using UnityEngine;

namespace FuncionalidadesCore.Inventory
{
    /// <summary>
    /// Componente listo para usar en objetos 3D. 
    /// Permite recoger el item y mandarlo al inventario usando el sistema de interacción.
    /// </summary>
    public class InventoryItemPickup : MonoBehaviour, IInteractStart, IInteractInfo
    {
        [Header("Configuración del Item")]
        [Tooltip("El GUID exacto que definiste en el InventoryDatabase")]
        public string ItemGUID;
        public ushort Quantity = 1;

        [Header("HUD")]
        public string Title = "Recoger Item";

        // Referencia estática o búsqueda al vuelo del inventario
        private InventoryManager inventoryManager;

        private void Start()
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }

        // --- IInteractInfo ---
        public string InteractTitle => Title;

        // --- IInteractStart ---
        public bool CanInteract() => true;

        public void InteractStart()
        {
            if (inventoryManager == null)
            {
                Debug.LogError("[InventoryItemPickup] No se encontró InventoryManager en la escena.");
                return;
            }

            // El núcleo matemático intenta agregar el item
            bool success = inventoryManager.Core.AddItem(ItemGUID, Quantity);

            if (success)
            {
                // Si hubo espacio y el item existe, destruimos el modelo 3D del piso
                Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"[InventoryItemPickup] No se pudo recoger el item {ItemGUID}. (Inventario lleno o no existe en DB).");
            }
        }
    }
}
