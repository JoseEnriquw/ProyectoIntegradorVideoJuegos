using UnityEngine;
using System.Collections;
using UHFPS.Runtime;
using UHFPS.Tools;
using UHFPS.Scriptable;

public class EquiparItemAlInicio : MonoBehaviour
{
    [Tooltip("El nombre exacto del ítem tal como aparece en el Player Items Manager (Ej: 'Flashlight').")]
    public string itemName = "Flashlight";

    [Tooltip("Nombre exacto de la escena donde quieres que esto ocurra (ej: 'Nivel2'). Déjalo vacío si quieres que funcione en cualquier escena donde esté este script.")]
    public string escenaEspecifica = "";

    [Header("Auto-desequipar Linterna")]
    [Tooltip("Si está activo, desequipa la linterna automáticamente cuando la batería llegue a 0.")]
    public bool desequiparAlAcabarBateria = true;

    [Header("Diálogos (Opcional)")]
    [Tooltip("El AudioSource específico donde se escuchará el diálogo. (Ej: arrastrá tu HEROPLAYER aquí)")]
    public AudioSource audioSourceEspecifico;

    [Tooltip("Diálogo (Dialogue Asset) que se lanza al equipar la linterna.")]
    public DialogueAsset dialogoAlEquipar;

    [Tooltip("Diálogo (Dialogue Asset) que se lanza cuando se agota la batería.")]
    public DialogueAsset dialogoBateriaAgotada;

    // Referencias cacheadas tras el equip inicial
    private PlayerItemsManager playerItems;
    private FlashlightItem linterna;
    private bool monitorearBateria = false;

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
        playerItems = GameManager.Instance.PlayerPresence.PlayerManager.PlayerItems;

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
                    
                    // Ejecutamos diálogo de equipamiento si existe
                    if (dialogoAlEquipar != null) ReproducirDialogo(dialogoAlEquipar);
                }

                // Si es una linterna y queremos monitorear la batería, la cacheamos
                if (desequiparAlAcabarBateria && itemToEquip is FlashlightItem fl)
                {
                    linterna = fl;
                    monitorearBateria = true;
                }
            }
            else
            {
                Debug.LogWarning($"[EquiparItemAlInicio] No se encontró un ítem llamado '{itemName}' en el PlayerItemsManager.");
            }
        }
    }

    private void Update()
    {
        // Solo monitoreamos si la feature está activa, tenemos referencia y la batería aún no llegó a 0
        if (!monitorearBateria || linterna == null || playerItems == null)
            return;

        if (linterna.batteryEnergy <= 0f)
        {
            monitorearBateria = false; // Evitamos llamarlo más de una vez
            playerItems.DeselectCurrent();

            // Ejecutamos diálogo de batería agotada si existe
            if (dialogoBateriaAgotada != null) ReproducirDialogo(dialogoBateriaAgotada);

            Debug.Log("[EquiparItemAlInicio] Batería agotada — linterna desequipada.");
        }
    }

    // --- Lógica para reproducir DialogueAssets directamente ---
    private void ReproducirDialogo(DialogueAsset asset)
    {
        StartCoroutine(RutinaReproducirDialogo(asset));
    }

    private IEnumerator RutinaReproducirDialogo(DialogueAsset asset)
    {
        DialogueTrigger dt = gameObject.AddComponent<DialogueTrigger>();
        dt.Dialogue = asset;
        dt.TriggerType = DialogueTrigger.TriggerTypeEnum.Event;
        
        if (audioSourceEspecifico != null)
        {
            dt.DialogueType = DialogueTrigger.DialogueTypeEnum.Local;
            dt.DialogueAudio = audioSourceEspecifico;
        }
        else
        {
            dt.DialogueType = DialogueTrigger.DialogueTypeEnum.Global;
        }

        yield return null;

        dt.TriggerDialogue();
    }
}
