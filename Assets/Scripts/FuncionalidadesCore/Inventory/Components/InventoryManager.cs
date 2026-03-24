using UnityEngine;

namespace FuncionalidadesCore.Inventory
{
    /// <summary>
    /// Componente que se coloca en la escena (Manager).
    /// Conecta tu base de datos ScriptableObject con la lógica pura del InventoryCore.
    /// </summary>
    [RequireComponent(typeof(InventoryCore))]
    public class InventoryManager : MonoBehaviour
    {
        [Header("Base de Datos")]
        [Tooltip("Asigna el ScriptableObject InventoryDatabase que creaste en tu carpeta de assets.")]
        public InventoryDatabase Database;

        /// <summary>Referencia al núcleo del inventario.</summary>
        public InventoryCore Core { get; private set; }

        private void Awake()
        {
            Core = GetComponent<InventoryCore>();
            
            if (Database != null)
            {
                // Inyectamos la lista del ScriptableObject al sistema matemático
                Core.SetItemDatabase(Database.Items);
            }
            else
            {
                Debug.LogError("[InventoryManager] No has asignado ninguna InventoryDatabase en el inspector.");
            }
        }
    }
}
