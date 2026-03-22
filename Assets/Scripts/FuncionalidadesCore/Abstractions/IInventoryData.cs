using System;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Abstrae la lógica de inventario sin depender de UI grid o slots visuales.
    /// </summary>
    public interface IInventoryData
    {
        /// <summary>Agregar un item al inventario.</summary>
        bool AddItem(string itemGUID, ushort quantity, ItemCustomData customData = null);

        /// <summary>Remover un item del inventario.</summary>
        bool RemoveItem(string itemGUID, ushort quantity = 1);

        /// <summary>Verificar si un item existe en el inventario.</summary>
        bool HasItem(string itemGUID);

        /// <summary>Obtener la cantidad de un item.</summary>
        int GetItemQuantity(string itemGUID);

        /// <summary>Combinar dos items.</summary>
        bool CombineItems(string item1GUID, string item2GUID);

        /// <summary>Usar un item.</summary>
        bool UseItem(string itemGUID);

        // --- Eventos ---
        event Action<string, ushort> OnItemAdded;
        event Action<string, ushort> OnItemRemoved;
        event Action<string> OnItemUsed;
        event Action<string, string> OnItemsCombined;
    }

    /// <summary>
    /// Datos personalizados asociados a un item de inventario.
    /// </summary>
    [Serializable]
    public class ItemCustomData : StorableCollection { }
}
