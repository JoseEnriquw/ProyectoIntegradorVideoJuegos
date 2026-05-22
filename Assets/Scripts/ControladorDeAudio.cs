using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Un script premium, flexible y reutilizable para reproducir, detener, pausar y atenuar (fade)
/// clips de audio mediante triggers, eventos de Unity, o llamados desde otros scripts.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ControladorDeAudio : MonoBehaviour
{
    public enum ModoInicio { Manual, AlIniciar, AlHabilitar, AlEntrarTrigger }
    public enum ModoParada { Manual, AlDeshabilitar, AlSalirTrigger }

    [Header("Componente de Audio")]
    [Tooltip("El AudioSource que reproducirá el sonido. Si se deja vacío, se usará el del mismo objeto.")]
    public AudioSource audioSource;

    [Header("Clips de Audio")]
    [Tooltip("El clip de audio principal a reproducir.")]
    public AudioClip clipDeAudio;

    [Tooltip("Lista opcional de clips. Si agregas varios, el script puede seleccionar uno al azar para dar variedad.")]
    public List<AudioClip> clipsAleatorios = new List<AudioClip>();

    [Tooltip("¿Seleccionar un clip aleatorio de la lista al reproducir?")]
    public bool usarClipAleatorio = false;

    [Header("Configuración de Reproducción")]
    [Tooltip("Cómo se iniciará el audio.")]
    public ModoInicio iniciarCon = ModoInicio.Manual;

    [Tooltip("Cómo se detendrá el audio.")]
    public ModoParada detenerCon = ModoParada.Manual;

    [Range(0f, 1f)]
    [Tooltip("Volumen objetivo del audio.")]
    public float volumenMaximo = 1f;

    [Tooltip("¿El audio debe repetirse en bucle?")]
    public bool bucle = false;

    [Header("Transición de Volumen (Fade)")]
    [Tooltip("Si es mayor a 0, el volumen aumentará gradualmente (Fade In) al iniciar.")]
    [Min(0f)]
    public float duracionFadeIn = 0f;

    [Tooltip("Si es mayor a 0, el volumen disminuirá gradualmente (Fade Out) al detenerse.")]
    [Min(0f)]
    public float duracionFadeOut = 0f;

    [Header("Configuración de Trigger (Colisiones)")]
    [Tooltip("El tag del objeto que debe activar el trigger (generalmente 'Player').")]
    public string tagTrigger = "Player";

    [Tooltip("¿El trigger solo debe funcionar una única vez en todo el juego?")]
    public bool soloUnaVez = false;

    [Header("Eventos de Unity")]
    public UnityEvent AlIniciarReproduccion;
    public UnityEvent AlDetenerReproduccion;

    private Coroutine rutinaFade;
    private bool yaSeReprodujo = false;

    private void Awake()
    {
        // Si no se asignó un AudioSource, buscarlo en el mismo objeto
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // Si aún es nulo (por seguridad), agregar uno automáticamente
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Guardar la configuración por defecto
        audioSource.loop = bucle;
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (iniciarCon == ModoInicio.AlIniciar)
        {
            Reproducir();
        }
    }

    private void OnEnable()
    {
        if (iniciarCon == ModoInicio.AlHabilitar)
        {
            Reproducir();
        }
    }

    private void OnDisable()
    {
        if (detenerCon == ModoParada.AlDeshabilitar)
        {
            Detener();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (iniciarCon != ModoInicio.AlEntrarTrigger) return;
        if (soloUnaVez && yaSeReprodujo) return;

        if (string.IsNullOrEmpty(tagTrigger) || other.CompareTag(tagTrigger))
        {
            Reproducir();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (detenerCon != ModoParada.AlSalirTrigger) return;

        if (string.IsNullOrEmpty(tagTrigger) || other.CompareTag(tagTrigger))
        {
            Detener();
        }
    }

    /// <summary>
    /// Inicia la reproducción del audio respetando el Fade In si está configurado.
    /// </summary>
    public void Reproducir()
    {
        if (audioSource == null) return;
        if (soloUnaVez && yaSeReprodujo) return;

        yaSeReprodujo = true;

        // Seleccionar el clip
        AudioClip clipSeleccionado = clipDeAudio;
        if (usarClipAleatorio && clipsAleatorios.Count > 0)
        {
            int indiceRandom = Random.Range(0, clipsAleatorios.Count);
            clipSeleccionado = clipsAleatorios[indiceRandom];
        }

        if (clipSeleccionado != null)
        {
            audioSource.clip = clipSeleccionado;
        }

        if (audioSource.clip == null)
        {
            Debug.LogWarning($"[ControladorDeAudio] No hay clip de audio asignado en '{gameObject.name}'", this);
            return;
        }

        audioSource.loop = bucle;

        // Manejar el Fade In
        if (duracionFadeIn > 0f)
        {
            IniciarRutinaFade(0f, volumenMaximo, duracionFadeIn, null);
        }
        else
        {
            DetenerRutinaFade();
            audioSource.volume = volumenMaximo;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        AlIniciarReproduccion?.Invoke();
        Debug.Log($"[ControladorDeAudio] Reproduciendo clip '{audioSource.clip.name}' en '{gameObject.name}'.", this);
    }

    /// <summary>
    /// Detiene la reproducción de inmediato o con un Fade Out gradual si está configurado.
    /// </summary>
    public void Detener()
    {
        if (audioSource == null || !audioSource.isPlaying) return;

        if (duracionFadeOut > 0f)
        {
            IniciarRutinaFade(audioSource.volume, 0f, duracionFadeOut, () => {
                audioSource.Stop();
                AlDetenerReproduccion?.Invoke();
            });
        }
        else
        {
            DetenerRutinaFade();
            audioSource.Stop();
            AlDetenerReproduccion?.Invoke();
            Debug.Log($"[ControladorDeAudio] Audio detenido en '{gameObject.name}'.", this);
        }
    }

    /// <summary>
    /// Pausa la reproducción del audio actual.
    /// </summary>
    public void Pausar()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
            Debug.Log($"[ControladorDeAudio] Audio pausado en '{gameObject.name}'.", this);
        }
    }

    /// <summary>
    /// Reanuda el audio previamente pausado.
    /// </summary>
    public void Despausar()
    {
        if (audioSource != null && !audioSource.isPlaying && audioSource.time > 0f)
        {
            audioSource.UnPause();
            Debug.Log($"[ControladorDeAudio] Audio despausado en '{gameObject.name}'.", this);
        }
    }

    /// <summary>
    /// Reproduce un clip de audio específico una sola vez sin interrumpir el flujo principal (PlayOneShot).
    /// </summary>
    public void ReproducirUnicaVez(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volumenMaximo);
        }
    }

    /// <summary>
    /// Lógica de transición gradual del volumen (Fade).
    /// </summary>
    private void IniciarRutinaFade(float inicio, float fin, float duracion, System.Action callbackAlFinalizar)
    {
        DetenerRutinaFade();
        rutinaFade = StartCoroutine(RutinaFadeVolumen(inicio, fin, duracion, callbackAlFinalizar));
    }

    private void DetenerRutinaFade()
    {
        if (rutinaFade != null)
        {
            StopCoroutine(rutinaFade);
            rutinaFade = null;
        }
    }

    private IEnumerator RutinaFadeVolumen(float volumenInicio, float volumenFin, float duracion, System.Action callbackAlFinalizar)
    {
        float tiempoTranscurrido = 0f;
        audioSource.volume = volumenInicio;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(volumenInicio, volumenFin, tiempoTranscurrido / duracion);
            yield return null;
        }

        audioSource.volume = volumenFin;
        callbackAlFinalizar?.Invoke();
        rutinaFade = null;
    }

    private void OnDrawGizmos()
    {
        if (iniciarCon == ModoInicio.AlEntrarTrigger || detenerCon == ModoParada.AlSalirTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col != null && col.isTrigger)
            {
                Gizmos.color = new Color(0.9f, 0.4f, 0.2f, 0.3f); // Naranja translúcido
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, col.bounds.size / transform.lossyScale.x);
            }
        }
    }
}
