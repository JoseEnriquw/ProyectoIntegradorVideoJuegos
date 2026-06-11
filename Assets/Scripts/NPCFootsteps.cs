using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class NPCFootsteps : MonoBehaviour
{
    [Header("Configuracin de Pasos")]
    public AudioClip[] footstepSounds;
    [Tooltip("Distancia en unidades que debe recorrer para reproducir un paso")]
    public float stepDistance = 1.2f; 
    [Range(0f, 1f)]
    public float volume = 0.4f;

    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float distanceTraveled;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        
        // Ajustamos el audio source si es necesario para que sea 3D
        audioSource.spatialBlend = 1f; 
        audioSource.maxDistance = 20f;
        
        lastPosition = transform.position;
    }

    void Update()
    {
        if (footstepSounds == null || footstepSounds.Length == 0 || !agent.enabled) return;

        // Solo contamos distancia si realmente se est moviendo
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            float distanceThisFrame = Vector3.Distance(transform.position, lastPosition);
            distanceTraveled += distanceThisFrame;

            if (distanceTraveled >= stepDistance)
            {
                PlayFootstep();
                distanceTraveled = 0f;
            }
        }
        else
        {
            distanceTraveled = 0f; // Reiniciamos si se detiene
        }

        lastPosition = transform.position;
    }

    private void PlayFootstep()
    {
        AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
        audioSource.PlayOneShot(clip, volume);
    }
}
