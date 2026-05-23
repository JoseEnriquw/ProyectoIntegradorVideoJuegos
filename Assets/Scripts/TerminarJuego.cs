using UnityEngine;
using UHFPS.Runtime;

public class TerminarJuego : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Si el jugador atraviesa este trigger
        if (other.CompareTag("Player"))
        {
            // 1. Congelamos al jugador y liberamos el cursor (Exactamente lo mismo que hace la pausa)
            GameManager.Instance.FreezePlayer(true, true);
            
            // 2. Ejecutamos la función oficial de UHFPS para volver al Menú Principal con fundido a negro
            GameManager.Instance.MainMenu();
        }
    }
}
