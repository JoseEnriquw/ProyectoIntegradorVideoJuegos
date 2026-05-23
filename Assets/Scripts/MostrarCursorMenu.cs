using UnityEngine;

public class MostrarCursorMenu : MonoBehaviour
{
    private void Start()
    {
        // Desbloquea el cursor para que pueda moverse libremente por la pantalla
        Cursor.lockState = CursorLockMode.None;
        
        // Hace que el cursor sea visible
        Cursor.visible = true;
    }
}
