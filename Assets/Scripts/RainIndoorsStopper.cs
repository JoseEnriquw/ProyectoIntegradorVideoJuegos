using UnityEngine;

public class RainIndoorsStopper : MonoBehaviour
{
    [Tooltip("El Particle System de lluvia que está en el jugador.")]
    public ParticleSystem rainParticleSystem;

    [Tooltip("El AudioSource del sonido de la lluvia (opcional).")]
    public AudioSource rainAudioSource;

    private float originalVolume;

    private int playerCollidersInside = 0;

    private void Start()
    {
        if (rainAudioSource != null)
        {
            originalVolume = rainAudioSource.volume;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Objeto entró al granero: " + other.gameObject.name);

        if (other.CompareTag("Player") || other.gameObject.name.ToLower().Contains("player") || other.GetComponentInChildren<Camera>() != null)
        {
            playerCollidersInside++;
            
            // Solo apagamos la lluvia si es el PRIMER collider del jugador que entra
            if (playerCollidersInside == 1)
            {
                Debug.Log("¡Jugador detectado! Apagando lluvia...");
                if (rainParticleSystem != null)
                {
                    var emission = rainParticleSystem.emission;
                    emission.enabled = false;
                }

                if (rainAudioSource != null)
                {
                    rainAudioSource.volume = originalVolume * 0.2f;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.name.ToLower().Contains("player") || other.GetComponentInChildren<Camera>() != null)
        {
            playerCollidersInside--;
            
            // Por seguridad, si baja de 0 lo dejamos en 0
            if (playerCollidersInside < 0) playerCollidersInside = 0;

            // Solo encendemos la lluvia si YA NO QUEDAN colliders del jugador adentro
            if (playerCollidersInside == 0)
            {
                Debug.Log("¡Jugador salió! Encendiendo lluvia...");
                if (rainParticleSystem != null)
                {
                    var emission = rainParticleSystem.emission;
                    emission.enabled = true;
                }

                if (rainAudioSource != null)
                {
                    rainAudioSource.volume = originalVolume;
                }
            }
        }
    }
}
