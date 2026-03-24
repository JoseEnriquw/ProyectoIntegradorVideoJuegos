using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FuncionalidadesCore.Inventory.UI
{
    /// <summary>
    /// El panel principal del inventario en tu Canvas.
    /// Escucha al cerebro (InventoryCore) y crea/destruye los prefabs visuales.
    /// </summary>
    public class InventoryUIPanel : MonoBehaviour
    {
        [Header("Referencias de Lógica")]
        public InventoryManager Manager;

        [Header("Referencias de UI")]
        public Transform SlotsContainer; // El objeto con el GridLayoutGroup
        public GameObject SlotPrefab;    // El prefab de tu cuadrito

        [Header("Visibilidad")]
        public GameObject InventoryWindow; // La ventana entera
        public KeyCode ToggleKey = KeyCode.Tab;

        // Diccionario para saber qué cuadrito visual pertenece a qué Item
        private readonly Dictionary<string, InventoryUISlot> activeSlots = new();

        private void Start()
        {
            if (Manager == null) Manager = FindObjectOfType<InventoryManager>();
            if (Manager == null) return;

            // 1. Suscribirse a los avisos del cerebro (Core)
            Manager.Core.OnItemAdded += HandleItemAdded;
            Manager.Core.OnItemRemoved += HandleItemRemoved;
            Manager.Core.OnItemUsed += HandleItemUsed;

            // Asegurarnos de arrancar con el menú cerrado
            if (InventoryWindow) InventoryWindow.SetActive(false);
        }

        private void Update()
        {
            // Abrir y cerrar inventario
            if (Input.GetKeyDown(ToggleKey))
            {
                bool isOpen = !InventoryWindow.activeSelf;
                InventoryWindow.SetActive(isOpen);

                // Opcional: Pausar el juego usando GameManagerCore
                if (GameManagerCore.Instance != null && !GameManagerCore.Instance.PlayerDied)
                {
                    GameManagerCore.Instance.FreezePlayer(isOpen, isOpen); // Congelar al jugador y mostrar cursor
                }
            }
        }

        // ===============================================
        // ESCUCHAS DEL CORE INVISIBLE
        // ===============================================

        private void HandleItemAdded(string guid, ushort quantityAdded)
        {
            int total = Manager.Core.GetItemQuantity(guid);

            // Si el prefab de este item YA existe en la UI, solo actualizamos el número
            if (activeSlots.TryGetValue(guid, out var slotUI))
            {
                slotUI.UpdateQuantity(total);
            }
            else // Si no existe, creamos el cuadrito visual desde cero
            {
                var itemDef = Manager.Database.Items.FirstOrDefault(i => i.GUID == guid);
                if (itemDef == null) return;

                // Instanciar Prefab
                GameObject newObj = Instantiate(SlotPrefab, SlotsContainer);
                var newSlotUI = newObj.GetComponent<InventoryUISlot>();
                
                if (newSlotUI != null)
                {
                    newSlotUI.Setup(guid, itemDef.Icon, total, this);
                    activeSlots.Add(guid, newSlotUI);
                }
            }
        }

        private void HandleItemRemoved(string guid, ushort quantityRemoved)
        {
            if (!activeSlots.TryGetValue(guid, out var slotUI)) return;

            int total = Manager.Core.GetItemQuantity(guid);

            if (total <= 0)
            {
                // Si nos quedamos sin items de este tipo, destruimos el cuadrito
                Destroy(slotUI.gameObject);
                activeSlots.Remove(guid);
            }
            else
            {
                // Si aún nos quedan, solo bajamos el número
                slotUI.UpdateQuantity(total);
            }
        }

        private void HandleItemUsed(string guid)
        {
            // Efectos de sonido o cierre de ventana
            // (La suma/resta matemática se hace sola antes de llegar aquí)
        }

        // ===============================================
        // ACCIONES DESDE LA UI HACIA EL CORE
        // ===============================================

        /// <summary>Llamado por el click del botón dentro del Prefab del Slot</summary>
        public void RequestUseItem(string guid)
        {
            // Le ordenamos al Cerebro que intente usarlo. 
            // Si el item se gasta (removeOnUse), el Cerebro disparará OnItemRemoved por su cuenta.
            Manager.Core.UseItem(guid);
        }
    }
}
