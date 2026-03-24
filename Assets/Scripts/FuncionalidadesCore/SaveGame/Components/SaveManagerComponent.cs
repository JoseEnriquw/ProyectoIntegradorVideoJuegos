using UnityEngine;

namespace FuncionalidadesCore.SaveGame
{
    /// <summary>
    /// Wrapper de MonoBehaviour para inicializar la lógica matemática de SaveGameCore en Unity.
    /// Pon esto en un objeto "GameManager" y te servirá para llamar a Guardar/Cargar desde botones de UI.
    /// </summary>
    public class SaveManagerComponent : MonoBehaviour
    {
        [Header("Configuración del Guardado")]
        public string SaveFileName = "partida1.json";
        public bool UseEncryption = false;
        [Tooltip("Llave de 16 caracteres para la encriptación AES")]
        public string EncryptionKey = "1234567890123456"; 

        // Referencia a la lógica pura
        private SaveGameCore saveCore;

        private void Awake()
        {
            // Inicializar la matemática indicándole dónde guardar (la carpeta segura persistente de juego en PC/Móvil)
            string savePath = Application.persistentDataPath + "/MySavedGames";
            saveCore = new SaveGameCore(savePath, EncryptionKey, UseEncryption);
            
            Debug.Log($"[SaveManager Component] Directorio de guardados configurado en: {savePath}");
        }

        /// <summary>
        /// Recolecta todos los objetos ISaveable de la escena y guarda el archivo físico.
        /// (Sirve para ponerlo en el OnClick de un botón "Guardar").
        /// </summary>
        public async void SaveCurrentGame()
        {
            var dataToSave = new StorableCollection();
            
            // Busca todos los objetos en escena que tengan interfaces ISaveable
            var saveables = FindObjectsOfType<MonoBehaviour>(); // Equivalente a un barrido pesado. Podría ser mediante un Registry.
            foreach (var mono in saveables)
            {
                if (mono is ISaveable saveable && mono is Component comp)
                {
                    dataToSave.Add(comp.gameObject.name, saveable.OnSave());
                }
            }

            // Realiza el guardado asíncrono
            await saveCore.SaveAsync(SaveFileName, dataToSave);
            Debug.Log($"[SaveManager Component] ¡Partida guardada exitosamente bajo el nombre {SaveFileName}!");
        }

        /// <summary>
        /// Lee el archivo JSON físico e inyecta la memoria a todos los objetos ISaveable compatibles.
        /// (Llamar desde botón "Cargar Partida").
        /// </summary>
        public async void LoadCurrentGame()
        {
            if (!saveCore.SaveExists(SaveFileName))
            {
                Debug.LogWarning($"[SaveManager Component] No existe la partida {SaveFileName}.");
                return;
            }

            var loadedData = await saveCore.LoadAsync<StorableCollection>(SaveFileName);
            if (loadedData == null) return;

            var saveables = FindObjectsOfType<MonoBehaviour>();
            foreach (var mono in saveables)
            {
                if (mono is ISaveable saveable && mono is Component comp)
                {
                    // Si encontramos los datos de este objeto por su nombre
                    if (loadedData.TryGetValue<StorableCollection>(comp.gameObject.name, out var objData))
                    {
                        saveable.OnLoad(objData);
                    }
                }
            }

            Debug.Log("[SaveManager Component] ¡Partida cargada exitosamente!");
        }
    }
}
