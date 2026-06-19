using UnityEngine;
using UnityEngine.SceneManagement;

public class CinematicSceneLoader : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre de la siguiente escena a cargar")]
    public string nextSceneName;

    private void Start()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName.Equals("2 Bosque_intro", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log("[CinematicSceneLoader] Ocultando y bloqueando cursor en la escena 2 Bosque_intro.");
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si es el auto (o el jugador) el que entra al trigger
        if (other.CompareTag("Player"))
        {
            LoadNextScene();
        }
    }

    /// <summary>
    /// Ejecuta el cambio de escena con una demora de 1.5 segundos después de ocultar el botón.
    /// </summary>
    public void LoadNextScene()
    {
        // Ocultar y destruir el botón de omitir intro si existe para que desaparezca de inmediato
        SkipIntroController skipController = FindObjectOfType<SkipIntroController>();
        if (skipController != null)
        {
            skipController.HideButton();
        }

        // Stop all active AudioSources in the scene immediately
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

        StartCoroutine(LoadSceneWithDelay());
    }

    /// <summary>
    /// Ejecuta el cambio de escena directamente.
    /// </summary>
    public void LoadNextSceneDirect()
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

    private System.Collections.IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForSeconds(1.5f);
        LoadNextSceneDirect();
    }
}
