using UnityEngine;
using System.Collections;
using UHFPS.Runtime;

public class EquiparItemAlInicio : MonoBehaviour
{
    [Tooltip("El nombre exacto del ítem tal como aparece en el Player Items Manager (Ej: 'Flashlight').")]
    public string itemName = "Flashlight";

    [Tooltip("Nombre exacto de la escena donde quieres que esto ocurra (ej: 'Nivel2'). Déjalo vacío si quieres que funcione en cualquier escena donde esté este script.")]
    public string escenaEspecifica = "";

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Si hay una escena específica configurada y no estamos en ella, nos cancelamos.
        if (!string.IsNullOrEmpty(escenaEspecifica) && scene.name != escenaEspecifica)
        {
            return; // No hacemos nada
        }

        // Usamos una corrutina para esperar un instante y asegurar que el inventario cargó todo
        StartCoroutine(EquipItemDelay());
    }

    private IEnumerator EquipItemDelay()
    {
        // Esperamos medio segundo para que UHFPS termine de cargar el nivel y los ítems iniciales
        yield return new WaitForSeconds(0.5f);

        // Obtenemos el manager de ítems del jugador usando el GameManager
        var playerItems = GameManager.Instance.PlayerPresence.PlayerManager.PlayerItems;
        
        if (playerItems != null)
        {
            // Buscamos el ítem por su nombre
            var itemToEquip = playerItems.GetItemByName(itemName);
            
            if (itemToEquip != null)
            {
                // Obtenemos su índice real en la lista y lo equipamos
                int index = playerItems.PlayerItems.IndexOf(itemToEquip);
                if (index != -1)
                {
                    playerItems.SwitchPlayerItem(index);
                }
            }
            else
            {
                Debug.LogWarning($"[EquiparItemAlInicio] No se encontró un ítem llamado '{itemName}' en el PlayerItemsManager.");
            }
        }
    }
}
