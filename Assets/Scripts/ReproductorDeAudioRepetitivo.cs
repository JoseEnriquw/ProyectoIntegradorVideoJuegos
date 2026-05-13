using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class ReproductorDeAudioRepetitivo : MonoBehaviour
{
    public enum MetodoInicio { OnStart, OnEnable, TriggerEnter, Manual }

    [Header("Configuración de Reproducción")]
    [Tooltip("El AudioSource específico que reproducirá el sonido. Si lo dejas vacío, buscará uno en este mismo objeto.")]
    public AudioSource audioSourceEspecifico;

    [Tooltip("El AudioClip que quieres reproducir (opcional si el AudioSource ya tiene uno).")]
    public AudioClip clipDeAudio;

    [Tooltip("Cuántos ciclos completos se van a ejecutar. Usa 0 si no quieres límite (ciclos infinitos).")]
    [Min(0)]
    public int ciclos = 3;

    [Tooltip("El tiempo en segundos que espera ANTES de comenzar cada ciclo (incluyendo el primero).")]
    [Min(0f)]
    public float delayEntreCiclos = 1f;

    [Tooltip("Cuántas veces se reproduce el audio dentro de un mismo ciclo (una detrás de la otra).")]
    [Min(1)]
    public int reproduccionesPorCiclo = 3;

    [Tooltip("Tiempo de espera extra entre cada reproducción dentro del mismo ciclo (usualmente 0).")]
    [Min(0f)]
    public float delayEntreReproducciones = 0f;

    [Header("Configuración de Disparo")]
    [Tooltip("Cómo quieres que comience a reproducirse.")]
    public MetodoInicio comoIniciar = MetodoInicio.Manual;

    [Tooltip("Si usas TriggerEnter, el tag que debe tener el objeto que colisiona (ej: 'Player'). Si está vacío, cualquier objeto lo dispara.")]
    public string tagParaTrigger = "Player";

    [Tooltip("Si es true, no permitirá que el trigger lo vuelva a activar una vez que ya comenzó (o ya terminó) su ciclo.")]
    public bool reproducirSoloUnaVezPorObjeto = true;

    [Header("Eventos Opcionales")]
    public UnityEvent AlTerminarTodasLasRepeticiones;

    private Coroutine rutinaActual;
    private bool yaSeReprodujo = false;

    private void Awake()
    {
        // Si no asignaste un AudioSource en el inspector, buscamos si hay uno en este objeto
        if (audioSourceEspecifico == null)
        {
            audioSourceEspecifico = GetComponent<AudioSource>();
        }

        // Si hay un AudioClip configurado, se lo asignamos al AudioSource
        if (audioSourceEspecifico != null && clipDeAudio != null)
        {
            audioSourceEspecifico.clip = clipDeAudio;
        }
    }

    private void Start()
    {
        if (comoIniciar == MetodoInicio.OnStart)
        {
            IniciarReproduccion();
        }
    }

    private void OnEnable()
    {
        if (comoIniciar == MetodoInicio.OnEnable)
        {
            IniciarReproduccion();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (comoIniciar != MetodoInicio.TriggerEnter) return;
        
        // Si hay tag y no coincide, ignoramos
        if (!string.IsNullOrEmpty(tagParaTrigger) && !other.CompareTag(tagParaTrigger)) return;

        // Si ya se reprodujo y está configurado para hacerlo solo una vez, ignoramos
        if (reproducirSoloUnaVezPorObjeto && yaSeReprodujo) return;

        IniciarReproduccion();
    }

    /// <summary>
    /// Llama a este método desde un botón, un UnityEvent, u otro script para empezar a reproducir.
    /// </summary>
    public void IniciarReproduccion()
    {
        if (audioSourceEspecifico == null)
        {
            Debug.LogWarning($"[ReproductorDeAudio] No hay un AudioSource asignado ni encontrado en {gameObject.name}.");
            return;
        }

        if (audioSourceEspecifico.clip == null && clipDeAudio == null)
        {
            Debug.LogWarning($"[ReproductorDeAudio] No hay un AudioClip asignado en {gameObject.name}.");
            return;
        }

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
        }

        yaSeReprodujo = true;
        rutinaActual = StartCoroutine(RutinaReproduccion());
    }

    /// <summary>
    /// Detiene la reproducción inmediatamente.
    /// </summary>
    public void DetenerReproduccion()
    {
        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
            rutinaActual = null;
        }
        
        if (audioSourceEspecifico != null)
        {
            audioSourceEspecifico.Stop();
        }
    }

    private IEnumerator RutinaReproduccion()
    {
        int ciclosCompletados = 0;
        bool ciclosInfinitos = ciclos <= 0;

        while (ciclosInfinitos || ciclosCompletados < ciclos)
        {
            // Esperamos el delay antes del ciclo
            if (delayEntreCiclos > 0f)
            {
                yield return new WaitForSeconds(delayEntreCiclos);
            }

            // Hacemos las repeticiones internas del ciclo
            for (int i = 0; i < reproduccionesPorCiclo; i++)
            {
                audioSourceEspecifico.Play();

                // Esperamos a que termine el clip de audio
                yield return new WaitForSeconds(audioSourceEspecifico.clip.length);

                // Si hay un delay extra entre las reproducciones internas (y no es la última), esperamos
                if (delayEntreReproducciones > 0f && i < reproduccionesPorCiclo - 1)
                {
                    yield return new WaitForSeconds(delayEntreReproducciones);
                }
            }

            ciclosCompletados++;
        }

        // Una vez terminadas las repeticiones (si no es infinito)
        rutinaActual = null;
        AlTerminarTodasLasRepeticiones?.Invoke();
    }

    // Dibujamos un Gizmo solo si estamos usando el modo Trigger
    private void OnDrawGizmos()
    {
        if (comoIniciar == MetodoInicio.TriggerEnter)
        {
            Collider col = GetComponent<Collider>();
            if (col != null && col.isTrigger)
            {
                Gizmos.color = new Color(0.2f, 0.4f, 0.8f, 0.4f); // Azulito translúcido
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, col.bounds.size / transform.lossyScale.x);
            }
        }
    }
}
