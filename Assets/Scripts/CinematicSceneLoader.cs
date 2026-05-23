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
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log("[CinematicSceneLoader] Cambiando a la escena: " + nextSceneName);
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("[CinematicSceneLoader] El nombre de la escena está vacío.");
            }
        }
    }
}
