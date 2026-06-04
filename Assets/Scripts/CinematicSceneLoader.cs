using UnityEngine;
using UnityEngine.SceneManagement;

public class CinematicSceneLoader : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre de la siguiente escena a cargar")]
    public string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si es el auto (o el jugador) el que entra al trigger
        if (other.CompareTag("Player"))
        {
            LoadNextScene();
        }
    }

    /// <summary>
    /// Ejecuta el cambio de escena directamente.
    /// </summary>
    public void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log("[CinematicSceneLoader] Iniciando cambio automático a escena: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[CinematicSceneLoader] No se puede cargar la escena porque el nombre está vacío.");
        }
    }
}
