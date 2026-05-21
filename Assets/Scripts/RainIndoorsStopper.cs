using UnityEngine;

public class RainIndoorsStopper : MonoBehaviour
{
    [Tooltip("El GameObject de la lluvia que quieres apagar entero al entrar.")]
    public GameObject rainObject;

    [Tooltip("Si ya tenías el ParticleSystem aquí, lo apagaremos entero (no hace falta ponerlo arriba si ya está aquí).")]
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
        if (other.CompareTag("Player") || other.gameObject.name.ToLower().Contains("player") || other.GetComponentInChildren<Camera>() != null)
        {
            playerCollidersInside++;
            
            if (playerCollidersInside == 1)
            {
                Debug.Log("¡Jugador entró! Apagando GameObject de lluvia...");
                
                if (rainObject != null)
                {
                    rainObject.SetActive(false);
                }
                else if (rainParticleSystem != null)
                {
                    rainParticleSystem.gameObject.SetActive(false);
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
            
            if (playerCollidersInside < 0) playerCollidersInside = 0;

            if (playerCollidersInside == 0)
            {
                Debug.Log("¡Jugador salió! Encendiendo GameObject de lluvia...");
                
                if (rainObject != null)
                {
                    rainObject.SetActive(true);
                }
                else if (rainParticleSystem != null)
                {
                    rainParticleSystem.gameObject.SetActive(true);
                }

                if (rainAudioSource != null)
                {
                    rainAudioSource.volume = originalVolume;
                }
            }
        }
    }
}
