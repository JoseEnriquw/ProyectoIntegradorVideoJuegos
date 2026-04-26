using UnityEngine;
using UHFPS.Runtime;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre exacto de la escena a cargar (debe estar en Build Settings).")]
    public string NextSceneName;
    [Tooltip("Si es falso, el trigger no hará nada hasta que se llame a SetActiveTrue().")]
    public bool IsActive = false;

    private void OnTriggerEnter(Collider other)
    {
        // Solo actuar si el sistema está activado y es el jugador quien entra
        if (IsActive && other.CompareTag("Player"))
        {
            Transition();
        }
    }

    /// <summary>
    /// Ejecuta el cambio de escena.
    /// </summary>
    public void Transition()
    {
        if (string.IsNullOrEmpty(NextSceneName))
        {
            Debug.LogWarning("[SceneTransition] ¡No has configurado el nombre de la escena de destino!");
            return;
        }

        Debug.Log("[SceneTransition] Iniciando transición a: " + NextSceneName);

        // Usamos el GameManager de UHFPS para una transición suave
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadNextLevel(NextSceneName);
        }
        else
        {
            // Fallback estándar de Unity
            UnityEngine.SceneManagement.SceneManager.LoadScene(NextSceneName);
        }
    }

    /// <summary>
    /// Llama a esta función desde el evento de 'Lectura de Carta' para habilitar el paso.
    /// </summary>
    public void SetActiveTrue()
    {
        IsActive = true;
        Debug.Log("[SceneTransition] El camino hacia la siguiente escena ahora está habilitado.");
    }
}
