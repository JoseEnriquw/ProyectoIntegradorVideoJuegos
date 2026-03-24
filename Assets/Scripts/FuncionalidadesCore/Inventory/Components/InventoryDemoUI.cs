using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace FuncionalidadesCore.Inventory
{
    /// <summary>
    /// Plantilla base para dibujar el inventario en un Canvas.
    /// Responde automáticamente a los eventos del InventoryCore para crear y destruir slots visuales.
    /// Requiere TextMeshPro para funcionar si se descomenta el código visual.
    /// </summary>
    public class InventoryDemoUI : MonoBehaviour
    {
        [Header("Referencias Opcionales")]
        [Tooltip("Puntero al inventario. Si se deja vacío, buscará en la escena.")]
        public InventoryManager InventoryMgr;

        private void Start()
        {
            if (InventoryMgr == null)
                InventoryMgr = FindObjectOfType<InventoryManager>();

            if (InventoryMgr == null) return;

            // Nos suscribimos a los eventos puros del cerebro
            InventoryMgr.Core.OnItemAdded += HandleItemAdded;
            InventoryMgr.Core.OnItemRemoved += HandleItemRemoved;
            InventoryMgr.Core.OnItemUsed += HandleItemUsed;
        }

        private void OnDestroy()
        {
            if (InventoryMgr == null) return;
            
            InventoryMgr.Core.OnItemAdded -= HandleItemAdded;
            InventoryMgr.Core.OnItemRemoved -= HandleItemRemoved;
            InventoryMgr.Core.OnItemUsed -= HandleItemUsed;
        }

        private void HandleItemAdded(string guid, ushort quantityAdded)
        {
            int total = InventoryMgr.Core.GetItemQuantity(guid);
            Debug.Log($"[DemoUI] UI Actualizada: Recibiste {quantityAdded}x {guid}. Tienes un total de: {total}");

            // Ejemplo real para Unity UI:
            // 1. Verificar si ya existe un slot instanciado (GameObject) para este 'guid'.
            // 2. Si existe, buscar su TextMeshProUGUI interno y actualizarlo con 'total.ToString()'.
            // 3. Si no existe, Instantiate(tuPrefabSlot, panelPadre), asignarle el ícono desde InventoryDB.Items.First(x=>x.GUID == guid).Icon
        }

        private void HandleItemRemoved(string guid, ushort quantityRemoved)
        {
            int total = InventoryMgr.Core.GetItemQuantity(guid);
            Debug.Log($"[DemoUI] UI Actualizada: Perdiste {quantityRemoved}x {guid}. Tienes un total de: {total}");

            // Ejemplo real para Unity UI:
            // 1. Si 'total' es 0, buscar el slot instanciado de este 'guid' y aplicarle Destroy(gameObject).
            // 2. Si > 0, actualizar su TextMeshProUGUI interno con 'total.ToString()'.
        }

        private void HandleItemUsed(string guid)
        {
            Debug.Log($"[DemoUI] El jugador ha presionado 'Usar' en {guid}.");
            // Aquí puedes reproducir un sonido, curar al jugador, etc.
        }
    }
}
