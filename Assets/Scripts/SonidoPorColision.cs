using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Un script premium, flexible y optimizado para reproducir efectos de sonido
/// al activarse el objeto (OnEnable) y/o cuando colisiona físicamente con otra superficie (OnCollisionEnter).
/// Permite modular el volumen según la fuerza del impacto y evitar repeticiones molestas (cooldown).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SonidoPorColision : MonoBehaviour
{
    [Header("Componentes de Audio")]
    [Tooltip("El AudioSource que reproducirá el sonido. Se buscará en el mismo objeto si se deja vacío.")]
    public AudioSource audioSource;

    [Header("Clips de Audio")]
    [Tooltip("El clip principal a reproducir.")]
    public AudioClip clipDeAudio;

    [Tooltip("Lista opcional de clips para elegir al azar y añadir variedad.")]
    public List<AudioClip> clipsAleatorios = new List<AudioClip>();

    [Tooltip("¿Usar clips de la lista al azar?")]
    public bool usarClipAleatorio = false;

    [Header("Reproducción al Activar (OnEnable)")]
    [Tooltip("¿Reproducir un sonido inmediatamente cuando el objeto se activa (SetActive(true))?")]
    public bool reproducirAlActivar = false;

    [Header("Configuración de Colisión (OnCollisionEnter)")]
    [Tooltip("¿Reproducir sonido al chocar físicamente con otro objeto?")]
    public bool reproducirAlColisionar = true;

    [Tooltip("Velocidad mínima de impacto requerida para reproducir el sonido. Evita que suene al arrastrarse ligeramente.")]
    [Range(0.01f, 5f)]
    public float velocidadMinimaImpacto = 0.1f;

    [Tooltip("¿Variar el volumen del sonido según la fuerza del impacto?")]
    public bool volumenDinamicoPorFuerza = true;

    [Tooltip("Velocidad de impacto con la que el sonido alcanzará el volumen máximo configurado.")]
    [Range(0.5f, 15f)]
    public float velocidadMaximoVolumen = 5f;

    [Tooltip("Volumen máximo que puede tener el sonido.")]
    [Range(0f, 1f)]
    public float volumenMaximo = 1f;

    [Header("Filtros y Cooldown")]
    [Tooltip("Tiempo mínimo en segundos entre sonidos para evitar molestas ráfagas si rebota muy rápido.")]
    [Min(0f)]
    public float cooldownSonido = 0.2f;

    [Tooltip("Si se especifica, solo colisiones con objetos que tengan este tag activarán el sonido. Vacío = cualquier objeto.")]
    public string tagFiltro = "";

    private float ultimoTiempoSonido = -99f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Configuración segura por defecto
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        if (reproducirAlActivar)
        {
            ReproducirSonido(volumenMaximo);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!reproducirAlColisionar) return;

        // Comprobar cooldown
        if (Time.time - ultimoTiempoSonido < cooldownSonido) return;

        // Comprobar filtro de tag
        if (!string.IsNullOrEmpty(tagFiltro) && !collision.gameObject.CompareTag(tagFiltro)) return;

        // Comprobar fuerza del impacto usando la velocidad relativa
        float velocidadImpacto = collision.relativeVelocity.magnitude;
        if (velocidadImpacto < velocidadMinimaImpacto) return;

        // Calcular volumen del sonido
        float volumenFinal = volumenMaximo;
        if (volumenDinamicoPorFuerza)
        {
            // Mapea la velocidad entre la mínima y la máxima para dar un volumen proporcional
            float t = Mathf.InverseLerp(velocidadMinimaImpacto, velocidadMaximoVolumen, velocidadImpacto);
            volumenFinal = Mathf.Lerp(0.1f, volumenMaximo, t);
        }

        // Registrar tiempo y reproducir
        ultimoTiempoSonido = Time.time;
        ReproducirSonido(volumenFinal);
    }

    /// <summary>
    /// Selecciona y reproduce un clip de sonido con el volumen indicado.
    /// </summary>
    public void ReproducirSonido(float volumen)
    {
        if (audioSource == null) return;

        AudioClip clipAReproducir = clipDeAudio;

        if (usarClipAleatorio && clipsAleatorios != null && clipsAleatorios.Count > 0)
        {
            int indice = Random.Range(0, clipsAleatorios.Count);
            clipAReproducir = clipsAleatorios[indice];
        }

        if (clipAReproducir != null)
        {
            audioSource.PlayOneShot(clipAReproducir, volumen);
            Debug.Log($"[SonidoPorColision] Reproducido '{clipAReproducir.name}' en '{gameObject.name}' con volumen {volumen:F2}.", this);
        }
        else
        {
            Debug.LogWarning($"[SonidoPorColision] No hay clips de audio asignados en '{gameObject.name}'.", this);
        }
    }
}
