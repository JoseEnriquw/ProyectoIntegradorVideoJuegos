using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Item de inventario - puro data, sin UI.
    /// Contiene toda la configuración de un item (propiedades, stack, combinación, etc.).
    /// </summary>
    [Serializable]
    public sealed class Item
    {
        public string GUID;
        public string SectionGUID;
        public string Title;
        public string Description;
        public ushort Width;
        public ushort Height;
        public Sprite Icon;

        [Serializable]
        public struct ItemSettings
        {
            public bool isUsable;
            public bool isStackable;
            public bool isExaminable;
            public bool isCombinable;
            public bool isDroppable;
            public bool isDiscardable;
            public bool canBindShortcut;
            public bool alwaysShowQuantity;
        }
        public ItemSettings Settings;

        public enum UsableType { PlayerItem, HealthItem, CustomEvent }

        [Serializable]
        public struct ItemUsableSettings
        {
            public UsableType usableType;
            public int playerItemIndex;
            public uint healthPoints;
            public bool removeOnUse;
        }
        public ItemUsableSettings UsableSettings;

        [Serializable]
        public struct ItemProperties
        {
            public ushort maxStack;
        }
        public ItemProperties Properties;

        [Serializable]
        public struct ItemCombineSettings
        {
            public ushort requiredCurrentAmount;
            public ushort requiredSecondAmount;
            public ushort resultItemAmount;
            public string combineWithID;
            public string resultCombineID;
            public int playerItemIndex;
            public bool isCrafting;
            public bool keepAfterCombine;
            public bool removeSecondItem;
            public bool eventAfterCombine;
            public bool selectAfterCombine;
        }
        public ItemCombineSettings[] CombineSettings;

        /// <summary>Crear una copia profunda del item.</summary>
        public Item DeepCopy()
        {
            return new Item
            {
                GUID = GUID,
                SectionGUID = SectionGUID,
                Title = Title,
                Description = Description,
                Width = Width,
                Height = Height,
                Icon = Icon,
                Settings = Settings,
                UsableSettings = UsableSettings,
                Properties = Properties,
                CombineSettings = CombineSettings
            };
        }
    }

    /// <summary>
    /// Slot de inventario que contiene un item y su cantidad actual.
    /// </summary>
    public class InventorySlot
    {
        public Item Item;
        public ushort Quantity;
        public ItemCustomData CustomData;

        public InventorySlot(Item item, ushort quantity, ItemCustomData customData = null)
        {
            Item = item;
            Quantity = quantity;
            CustomData = customData ?? new ItemCustomData();
        }
    }

    /// <summary>
    /// Lógica de inventario desacoplada de UI grid.
    /// Gestiona items, stacking, combinación, y crafting sin depender de slots visuales.
    /// </summary>
    public class InventoryCore : MonoBehaviour, IInventoryData
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 20;

        private List<Item> itemDatabase = new();
        private readonly List<InventorySlot> slots = new();

        // --- Eventos ---
        public event Action<string, ushort> OnItemAdded;
        public event Action<string, ushort> OnItemRemoved;
        public event Action<string> OnItemUsed;
        public event Action<string, string> OnItemsCombined;

        /// <summary>Todos los slots del inventario.</summary>
        public IReadOnlyList<InventorySlot> Slots => slots;

        /// <summary>Establecer la base de datos de items disponibles.</summary>
        public void SetItemDatabase(List<Item> database)
        {
            itemDatabase = database;
        }

        /// <summary>Agregar un item al inventario.</summary>
        public bool AddItem(string itemGUID, ushort quantity, ItemCustomData customData = null)
        {
            var itemDef = itemDatabase.FirstOrDefault(i => i.GUID == itemGUID);
            if (itemDef == null)
            {
                Debug.LogError($"[Inventory] Item '{itemGUID}' not found in database.");
                return false;
            }

            // Si es stackable, buscar slot existente
            if (itemDef.Settings.isStackable)
            {
                var existingSlot = slots.FirstOrDefault(s => s.Item.GUID == itemGUID);
                if (existingSlot != null)
                {
                    ushort newQty = (ushort)Mathf.Min(existingSlot.Quantity + quantity, itemDef.Properties.maxStack);
                    ushort added = (ushort)(newQty - existingSlot.Quantity);
                    existingSlot.Quantity = newQty;
                    OnItemAdded?.Invoke(itemGUID, added);
                    return true;
                }
            }

            // Crear nuevo slot
            if (slots.Count >= maxSlots)
            {
                Debug.LogWarning("[Inventory] Inventory is full.");
                return false;
            }

            var item = itemDef.DeepCopy();
            slots.Add(new InventorySlot(item, quantity, customData));
            OnItemAdded?.Invoke(itemGUID, quantity);
            return true;
        }

        /// <summary>Remover un item del inventario.</summary>
        public bool RemoveItem(string itemGUID, ushort quantity = 1)
        {
            var slot = slots.FirstOrDefault(s => s.Item.GUID == itemGUID);
            if (slot == null) return false;

            if (slot.Quantity <= quantity)
            {
                slots.Remove(slot);
                OnItemRemoved?.Invoke(itemGUID, slot.Quantity);
            }
            else
            {
                slot.Quantity -= quantity;
                OnItemRemoved?.Invoke(itemGUID, quantity);
            }

            return true;
        }

        /// <summary>Verificar si un item existe.</summary>
        public bool HasItem(string itemGUID)
        {
            return slots.Any(s => s.Item.GUID == itemGUID);
        }

        /// <summary>Obtener cantidad de un item.</summary>
        public int GetItemQuantity(string itemGUID)
        {
            var slot = slots.FirstOrDefault(s => s.Item.GUID == itemGUID);
            return slot?.Quantity ?? 0;
        }

        /// <summary>Usar un item.</summary>
        public bool UseItem(string itemGUID)
        {
            var slot = slots.FirstOrDefault(s => s.Item.GUID == itemGUID);
            if (slot == null || !slot.Item.Settings.isUsable) return false;

            OnItemUsed?.Invoke(itemGUID);

            if (slot.Item.UsableSettings.removeOnUse)
                RemoveItem(itemGUID, 1);

            return true;
        }

        /// <summary>Combinar dos items.</summary>
        public bool CombineItems(string item1GUID, string item2GUID)
        {
            var slot1 = slots.FirstOrDefault(s => s.Item.GUID == item1GUID);
            var slot2 = slots.FirstOrDefault(s => s.Item.GUID == item2GUID);

            if (slot1 == null || slot2 == null) return false;
            if (!slot1.Item.Settings.isCombinable) return false;

            var combineSettings = slot1.Item.CombineSettings?
                .FirstOrDefault(cs => cs.combineWithID == item2GUID);

            if (combineSettings == null) return false;
            var cs = combineSettings.Value;

            // Verificar cantidades requeridas
            if (slot1.Quantity < cs.requiredCurrentAmount || slot2.Quantity < cs.requiredSecondAmount)
                return false;

            // Aplicar combinación
            if (cs.isCrafting)
            {
                slot1.Quantity -= cs.requiredCurrentAmount;
                slot2.Quantity -= cs.requiredSecondAmount;

                if (slot1.Quantity <= 0) slots.Remove(slot1);
                if (slot2.Quantity <= 0) slots.Remove(slot2);
            }
            else
            {
                if (!cs.keepAfterCombine) slots.Remove(slot1);
                if (cs.removeSecondItem) slots.Remove(slot2);
            }

            // Agregar item resultante
            if (!string.IsNullOrEmpty(cs.resultCombineID))
            {
                AddItem(cs.resultCombineID, cs.resultItemAmount);
            }

            OnItemsCombined?.Invoke(item1GUID, item2GUID);
            return true;
        }

        // --- Save/Load ---
        public StorableCollection OnSave()
        {
            var data = new StorableCollection();
            var slotData = slots.Select(s => new StorableCollection
            {
                { "guid", s.Item.GUID },
                { "quantity", s.Quantity }
            }).ToList();

            data.Add("slots", slotData);
            return data;
        }
    }
}
