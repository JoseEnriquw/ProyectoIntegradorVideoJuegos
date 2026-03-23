using System.Collections.Generic;
using UnityEngine;

namespace FuncionalidadesCore.Inventory
{
    /// <summary>
    /// Base de datos de items en formato ScriptableObject (persiste entre escenas).
    /// </summary>
    [CreateAssetMenu(fileName = "New Inventory Database", menuName = "FuncionalidadesCore/Inventory/Database")]
    public class InventoryDatabase : ScriptableObject
    {
        [Header("Items de tu Juego")]
        [Tooltip("Agrega aquí todos los items que existirán en tu juego. El InventoryCore los leerá al arrancar.")]
        public List<Item> Items = new List<Item>();
    }
}
