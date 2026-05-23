using UnityEngine;
using UHFPS.Runtime;

[RequireComponent(typeof(DynamicObject))]
public class PuertaTrampa : MonoBehaviour
{
    [Header("Configuración de la Trampa")]
    [Tooltip("El componente AudioSource en la escena que reproducirá la voz del dueño (Ej: 3D_FarmerVoice).")]
    public AudioSource fuenteVozDueño;

    [Tooltip("Tiempo en segundos que tarda la puerta en cerrarse. Ajusta esto para que el portazo suene en el momento exacto.")]
    public float tiempoDeCierre = 0.2f;

    private DynamicObject dynamicObject;
    private bool trampaActivada = false;

    void Start()
    {
        dynamicObject = GetComponent<DynamicObject>();

        // Suscribimos nuestra función al evento "lockedEvent" del DynamicObject.
        dynamicObject.lockedEvent.AddListener(ReproducirAudioBloqueado);
    }

    void Update()
    {
        // Si la trampa ya se activó, no hacemos más comprobaciones en el Update
        if (trampaActivada) return;

        // IsHolding se vuelve true en el instante en que el jugador hace clic para interactuar con la puerta
        if (dynamicObject.IsHolding)
        {
            ActivarTrampa();
        }
    }

    private void ActivarTrampa()
    {
        trampaActivada = true;

        // 1. Soltamos la puerta de la mano del jugador
        dynamicObject.InteractStop();

        // 2. Impedimos temporalmente que el jugador la vuelva a agarrar mientras se está cerrando
        dynamicObject.isInteractLocked = true;

        // 3. Obligamos a la puerta a cerrarse automáticamente
        dynamicObject.SetCloseState();

        // 4. Iniciamos una cuenta regresiva corta para bloquearla permanentemente y hacer el ruido
        StartCoroutine(EsperarYBloquear());
    }

    private System.Collections.IEnumerator EsperarYBloquear()
    {
        // Esperamos el tiempo exacto que tarda en cerrarse (0.2s por defecto) para que no haya delay en el audio
        yield return new WaitForSeconds(tiempoDeCierre);

        // Forzamos la reproducción del sonido de "puerta cerrándose" exactamente al cerrarse
        dynamicObject.PlaySound(DynamicSoundType.Close);

        // Devolvemos la capacidad de interactuar al jugador (para que pueda "intentar" abrirla)
        dynamicObject.isInteractLocked = false;

        // Bloqueamos la puerta definitivamente
        dynamicObject.SetLockedStatus(true);
    }

    private void ReproducirAudioBloqueado()
    {
        // Verificamos que la trampa esté activa y haya un AudioSource asignado
        if (trampaActivada && fuenteVozDueño != null)
        {
            // Verificamos si no está sonando ya, para que no se superpongan los audios
            if (!fuenteVozDueño.isPlaying)
            {
                fuenteVozDueño.Play();
            }
        }
    }
}
