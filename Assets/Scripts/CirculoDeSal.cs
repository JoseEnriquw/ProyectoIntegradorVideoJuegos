using UnityEngine;

public class CirculoDeSal : MonoBehaviour
{
    // Variable estática para saber globalmente si el jugador está protegido
    public static bool jugadorProtegido = false;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica que el que entra sea el jugador (Asegúrate de que tu player tenga el tag "Player")
        if (other.CompareTag("Player"))
        {
            jugadorProtegido = true;
            Debug.Log("Jugador entró al círculo de sal. Está protegido.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorProtegido = false;
            Debug.Log("Jugador salió del círculo de sal. Ya NO está protegido.");
        }
    }
}
