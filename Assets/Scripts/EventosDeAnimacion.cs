using UnityEngine;
using UnityEngine.Events;

public class EventosDeAnimacion : MonoBehaviour
{
    [Header("Eventos de la Animación")]
    [Tooltip("Arrastra aquí lo que quieras que pase (Ej: Reproductor de Audio y Cinemachine Impulse)")]
    public UnityEvent AlTerminarAnimacion;

    // Esta es la función que vas a ver en el desplegable de la ventana "Animation"
    public void DispararEvento()
    {
        AlTerminarAnimacion?.Invoke();
    }
}
