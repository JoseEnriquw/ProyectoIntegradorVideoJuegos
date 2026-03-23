using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace FuncionalidadesCore.Inventory.UI
{
    /// <summary>
    /// Reemplazo avanzado de la UI lineal. Este script crea un sistema de inventario estilo 
    /// "Resident Evil" / "Tarkov" / "UHFPS", donde los objetos ocupan NxM espacios en una cuadrícula.
    /// Reemplaza al InventoryUIPanel lineal.
    /// </summary>
    public class InventoryGridUI : MonoBehaviour
    {
        [Header("Referencias")]
        public InventoryManager Manager;
        public GameObject InventoryWindow; // La ventana entera a activar/desactivar
        public RectTransform GridPane; // Paneles donde irán apareciendo (sin LayoutGroup)
        public GameObject GridItemPrefab; // El item en sí
        public GameObject EmptySlotPrefab; // NUEVO: El cuadradito vacío de fondo

        [Header("Configuración de la Cuadrícula")]
        public KeyCode ToggleKey = KeyCode.Tab;
        public int Columns = 8;
        public int Rows = 5;
        public float CellSize = 64f; // Tamaño en píxeles de 1 casilla
        public Vector2 Spacing = new Vector2(2f, 2f); // Espaciado real entre casillas

        // Array de [x, y] para saber qué celdas están ocupadas visualmente
        private bool[,] gridOccupied;
        
        // Diccionario para vincular un itemGUID matemático con su representación 2D visual
        private readonly Dictionary<string, InventoryGridItem> activeItems = new();

        private void Start()
        {
            if (Manager == null) Manager = FindObjectOfType<InventoryManager>();
            if (Manager == null) return;

            gridOccupied = new bool[Columns, Rows];

            // Tamaño fijo visual del Grid Pane para que entre justo
            GridPane.sizeDelta = new Vector2(Columns * CellSize, Rows * CellSize);

            // Escuchar al core invisible
            Manager.Core.OnItemAdded += HandleItemAdded;
            Manager.Core.OnItemRemoved += HandleItemRemoved;
            
            // Dibuja instantáneamente todo el fondo cuadriculado vacío
            DrawBackgroundGrid();
            
            // Asegurarnos de arrancar con el menú cerrado
            if (InventoryWindow) InventoryWindow.SetActive(false);
        }

        private void DrawBackgroundGrid()
        {
            if (EmptySlotPrefab == null) return;

            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    GameObject bgObj = Instantiate(EmptySlotPrefab, GridPane);
                    RectTransform rt = bgObj.GetComponent<RectTransform>();
                    
                    // Forzar anclajes a Top-Left por código (a prueba de fallos)
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(0, 1);
                    rt.pivot = new Vector2(0, 1);
                    
                    // Forzar que mida 1x1 CellSize (sin incluir espaciado en su propia imagen)
                    rt.sizeDelta = new Vector2(CellSize, CellSize);
                    
                    // Posicionarlo usando CellSize + Spaciado
                    float posX = x * (CellSize + Spacing.x);
                    float posY = y * (CellSize + Spacing.y);
                    rt.anchoredPosition = new Vector2(posX, -posY);
                    
                    bgObj.transform.SetAsFirstSibling();
                }
            }
        }

        private void Update()
        {
            // Abrir y cerrar inventario
            if (Input.GetKeyDown(ToggleKey))
            {
                if (InventoryWindow != null)
                {
                    bool isOpen = !InventoryWindow.activeSelf;
                    InventoryWindow.SetActive(isOpen);

                    // Congelar al jugador y mostrar el mouse usando el GameManager
                    if (GameManagerCore.Instance != null && !GameManagerCore.Instance.PlayerDied)
                    {
                        GameManagerCore.Instance.FreezePlayer(isOpen, isOpen);
                    }
                }
            }
        }

        // ==============================================
        // LÓGICA DE DIBUJO Y POSICIONAMIENTO NxM
        // ==============================================

        private void HandleItemAdded(string guid, ushort quantity)
        {
            var itemDef = Manager.Database.Items.FirstOrDefault(i => i.GUID == guid);
            if (itemDef == null) return;

            // 1. Si ya existe visualmente, solo actualizamos el número interno (si es stackable)
            if (activeItems.TryGetValue(guid, out var gridItemUI))
            {
                int total = Manager.Core.GetItemQuantity(guid);
                gridItemUI.UpdateQuantity(total);
                return;
            }

            // 2. Si NO existe visualmente, debemos encontrarle un hueco donde quepa NxM
            int w = itemDef.Width > 0 ? itemDef.Width : 1;
            int h = itemDef.Height > 0 ? itemDef.Height : 1;

            if (FindEmptySpace(w, h, out int startX, out int startY))
            {
                // Marcamos el espacio como ocupado en nuestro array 2D
                MarkSpace(startX, startY, w, h, true);

                // Instanciamos el prefab visual
                GameObject obj = Instantiate(GridItemPrefab, GridPane);
                RectTransform rt = obj.GetComponent<RectTransform>();
                
                // Forzar anclajes a Top-Left por código
                rt.anchorMin = new Vector2(0, 1);
                rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                
                // Configurar tamaño exacto sumando los tamaños de celda PLUS los espaciados intermedios
                float finalWidth = (w * CellSize) + ((w - 1) * Spacing.x);
                float finalHeight = (h * CellSize) + ((h - 1) * Spacing.y);
                rt.sizeDelta = new Vector2(finalWidth, finalHeight);
                
                // Posicionar respetando el grid y el spacing
                float posX = startX * (CellSize + Spacing.x);
                float posY = startY * (CellSize + Spacing.y);
                rt.anchoredPosition = new Vector2(posX, -posY);

                InventoryGridItem newItem = obj.GetComponent<InventoryGridItem>();
                if (newItem != null)
                {
                    newItem.Setup(guid, itemDef.Icon, Manager.Core.GetItemQuantity(guid), this);
                    activeItems.Add(guid, newItem);
                }
            }
            else
            {
                Debug.LogWarning($"[Grid UI] ¡No hay espacio físico {w}x{h} para colocar {itemDef.Title}!");
                // Opcional: podrías devolver el item o tirarlo al piso, aquí al menos la UI rechaza pintarlo
            }
        }

        private void HandleItemRemoved(string guid, ushort quantityRemoved)
        {
            if (!activeItems.TryGetValue(guid, out var gridItemUI)) return;

            int total = Manager.Core.GetItemQuantity(guid);

            if (total <= 0)
            {
                // Calculamos de vuelta dónde estaba posicionado para liberar la matemática
                var rect = gridItemUI.GetComponent<RectTransform>();
                int startX = Mathf.RoundToInt(rect.anchoredPosition.x / (CellSize + Spacing.x));
                int startY = Mathf.RoundToInt(Mathf.Abs(rect.anchoredPosition.y) / (CellSize + Spacing.y));
                
                var itemDef = Manager.Database.Items.FirstOrDefault(i => i.GUID == guid);
                if (itemDef != null)
                {
                    int w = itemDef.Width > 0 ? itemDef.Width : 1;
                    int h = itemDef.Height > 0 ? itemDef.Height : 1;
                    MarkSpace(startX, startY, w, h, false);
                }

                // Destruirlo visualmente
                Destroy(gridItemUI.gameObject);
                activeItems.Remove(guid);
            }
            else
            {
                // Aún quedan, solo bajar la cantidad mostrada
                gridItemUI.UpdateQuantity(total);
            }
        }

        public void RequestUseItem(string guid)
        {
            Manager.Core.UseItem(guid);
        }

        // ==============================================
        // MÉTODOS MATEMÁTICOS DE LA CUADRÍCULA 2D
        // ==============================================

        private bool FindEmptySpace(int width, int height, out int resultX, out int resultY)
        {
            resultX = -1;
            resultY = -1;

            // Evitar objetos más grandes que el inventario entero
            if (width > Columns || height > Rows) return false;

            for (int y = 0; y <= Rows - height; y++)
            {
                for (int x = 0; x <= Columns - width; x++)
                {
                    if (CheckSpace(x, y, width, height))
                    {
                        resultX = x;
                        resultY = y;
                        return true; // Encontramos el primer hueco
                    }
                }
            }
            return false;
        }

        private bool CheckSpace(int startX, int startY, int width, int height)
        {
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    if (gridOccupied[x, y]) return false; // si un solo cuadrito está ocupado, falla
                }
            }
            return true;
        }

        private void MarkSpace(int startX, int startY, int width, int height, bool isOccupied)
        {
            for (int y = startY; y < startY + height; y++)
            {
                for (int x = startX; x < startX + width; x++)
                {
                    gridOccupied[x, y] = isOccupied;
                }
            }
        }
    }
}
