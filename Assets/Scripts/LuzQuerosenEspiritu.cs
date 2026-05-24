using UnityEngine;

[RequireComponent(typeof(Light))]
public class LuzQuerosenEspiritu : MonoBehaviour
{
    public enum EstadoLuz
    {
        Normal,
        TransicionAtenuar,
        Atenuado,
        TransicionRecuperar,
        ApagadoPermanente
    }

    [Header("Flicker Normal (Querosen)")]
    [Tooltip("Intensidad base de la lámpara en estado normal.")]
    public float intensidadNormal = 1.5f;

    [Tooltip("Velocidad del parpadeo cálido en estado normal.")]
    public float velocidadFlickerNormal = 1.5f;

    [Tooltip("Rango de fluctuación de la intensidad en estado normal.")]
    public float rangoFlickerNormal = 0.15f;

    [Header("Efecto Espíritu (Atenuación)")]
    [Tooltip("Intensidad base a la que baja la luz cuando hay presencia espiritual (puede ser 0 para apagado total).")]
    public float intensidadAtenuada = 0.05f;

    [Tooltip("Tiempo en segundos que tarda la luz en atenuarse por completo.")]
    public float duracionAtenuacion = 3.0f;

    [Tooltip("Tiempo en segundos que la luz permanece atenuada antes de recuperarse (si 'Recuperar Al Final' está activo).")]
    public float duracionPermanencia = 4.0f;

    [Tooltip("¿La luz vuelve a encenderse sola tras pasar el espíritu?")]
    public bool recuperarAlFinal = true;

    [Tooltip("Tiempo en segundos que tarda la luz en volver a su estado normal.")]
    public float duracionRecuperacion = 2.0f;

    [Header("Inestabilidad Paranormal")]
    [Tooltip("Multiplicador de la velocidad de parpadeo durante el efecto (flicker rápido y violento).")]
    public float inestabilidadFlicker = 8.0f;

    [Tooltip("Fuerza o rango del parpadeo caótico cuando el espíritu está presente.")]
    public float fuerzaFlickerEspiritu = 0.6f;

    [Tooltip("Probabilidad (de 0 a 1) por segundo de que ocurran micro-apagones instantáneos (sputtering/parpadeo seco).")]
    [Range(0f, 1f)]
    public float probabilidadApagonTemporal = 0.3f;

    [Header("Cambio de Color Paranormal")]
    [Tooltip("¿Cambiar el color de la luz a un tono frío durante la presencia espiritual?")]
    public bool cambiarColorEspiritu = false;

    [Tooltip("Color frío o fantasmal al que cambiará la luz.")]
    public Color colorEspiritu = new Color(0.5f, 0.8f, 1f);

    [Header("Disparador por Proximidad (Opcional)")]
    [Tooltip("¿Disparar el efecto automáticamente cuando un objetivo (ej: el jugador) se acerque?")]
    public bool dispararPorProximidad = false;

    [Tooltip("Tag del objeto que activa el efecto (normalmente 'Player').")]
    public string tagObjetivo = "Player";

    [Tooltip("Distancia mínima al objetivo para activar el efecto de atenuación.")]
    public float distanciaActivar = 5.0f;

    [Header("Efectos de Sonido (Opcional)")]
    [Tooltip("Componente AudioSource para reproducir los sonidos.")]
    public AudioSource fuenteAudio;

    [Tooltip("Audio que suena mientras la llama lucha por no apagarse (bucle).")]
    public AudioClip sonidoSoploEspiritu;

    [Tooltip("Audio de golpe o soplido seco al apagarse del todo.")]
    public AudioClip sonidoApagado;

    // Variables de estado
    private Light targetLight;
    private EstadoLuz estadoActual = EstadoLuz.Normal;
    private float randomOffset;
    private float tiempoTransicionActual = 0f;
    private float tiempoEstadoActual = 0f;
    
    // Respaldos de valores iniciales
    private Color colorNormal;
    private Transform transformObjetivo;

    void Start()
    {
        targetLight = GetComponent<Light>();
        colorNormal = targetLight.color;
        randomOffset = Random.Range(0.0f, 1000.0f);

        // Si se activa por proximidad, buscamos al jugador
        if (dispararPorProximidad)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(tagObjetivo);
            if (obj != null)
            {
                transformObjetivo = obj.transform;
            }
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Si se dispara por proximidad, monitoreamos la distancia
        if (dispararPorProximidad && estadoActual == EstadoLuz.Normal && transformObjetivo != null)
        {
            float distancia = Vector3.Distance(transform.position, transformObjetivo.position);
            if (distancia <= distanciaActivar)
            {
                ActivarEfectoEspiritu();
            }
        }

        // Simulación de estados
        switch (estadoActual)
        {
            case EstadoLuz.Normal:
                SimularFlickerNormal();
                break;

            case EstadoLuz.TransicionAtenuar:
                ActualizarTransicionAtenuar(dt);
                break;

            case EstadoLuz.Atenuado:
                ActualizarAtenuado(dt);
                break;

            case EstadoLuz.TransicionRecuperar:
                ActualizarTransicionRecuperar(dt);
                break;

            case EstadoLuz.ApagadoPermanente:
                targetLight.intensity = 0f;
                break;
        }
    }

    private void SimularFlickerNormal()
    {
        // Flicker lento y orgánico usando Perlin Noise
        float noise = Mathf.PerlinNoise(randomOffset, Time.time * velocidadFlickerNormal);
        targetLight.intensity = intensidadNormal + (noise * 2f - 1f) * rangoFlickerNormal;
        targetLight.color = colorNormal;
    }

    private void ActualizarTransicionAtenuar(float dt)
    {
        tiempoTransicionActual += dt;
        float t = Mathf.Clamp01(tiempoTransicionActual / duracionAtenuacion);

        // Interpolamos intensidad base y color
        float baseIntensity = Mathf.Lerp(intensidadNormal, intensidadAtenuada, t);
        if (cambiarColorEspiritu)
        {
            targetLight.color = Color.Lerp(colorNormal, colorEspiritu, t);
        }

        // Calculamos parpadeo caótico (más rápido y violento según avanza t)
        float flickerSpeed = Mathf.Lerp(velocidadFlickerNormal, velocidadFlickerNormal * inestabilidadFlicker, t);
        float noise = Mathf.PerlinNoise(randomOffset, Time.time * flickerSpeed);
        float rangoFlicker = Mathf.Lerp(rangoFlickerNormal, fuerzaFlickerEspiritu, t);
        
        float finalIntensity = baseIntensity + (noise * 2f - 1f) * rangoFlicker;

        // Simular micro-apagones aleatorios (la llama parpadea secamente)
        if (t > 0.2f && Random.value < probabilidadApagonTemporal * t * dt * 5f)
        {
            finalIntensity = 0f;
        }

        targetLight.intensity = Mathf.Max(0f, finalIntensity);

        if (t >= 1f)
        {
            estadoActual = EstadoLuz.Atenuado;
            tiempoEstadoActual = Time.time;
        }
    }

    private void ActualizarAtenuado(float dt)
    {
        // Se mantiene muy tenue y parpadeando débilmente
        float noise = Mathf.PerlinNoise(randomOffset, Time.time * (velocidadFlickerNormal * inestabilidadFlicker * 0.5f));
        float finalIntensity = intensidadAtenuada + (noise * 2f - 1f) * (fuerzaFlickerEspiritu * 0.3f);
        
        // Pequeños amagos de apagón total
        if (Random.value < probabilidadApagonTemporal * 0.5f * dt * 5f)
        {
            finalIntensity = 0f;
        }
        targetLight.intensity = Mathf.Max(0f, finalIntensity);

        if (recuperarAlFinal)
        {
            if (Time.time - tiempoEstadoActual > duracionPermanencia)
            {
                estadoActual = EstadoLuz.TransicionRecuperar;
                tiempoTransicionActual = 0f;
            }
        }
        else
        {
            // Apagado total definitivo
            targetLight.intensity = 0f;
            estadoActual = EstadoLuz.ApagadoPermanente;
            
            if (fuenteAudio != null)
            {
                fuenteAudio.Stop();
                if (sonidoApagado != null)
                {
                    fuenteAudio.PlayOneShot(sonidoApagado);
                }
            }
        }
    }

    private void ActualizarTransicionRecuperar(float dt)
    {
        tiempoTransicionActual += dt;
        float t = Mathf.Clamp01(tiempoTransicionActual / duracionRecuperacion);

        // Retornamos a la intensidad normal y color original
        float baseIntensity = Mathf.Lerp(intensidadAtenuada, intensidadNormal, t);
        if (cambiarColorEspiritu)
        {
            targetLight.color = Color.Lerp(colorEspiritu, colorNormal, t);
        }

        // Parpadea erráticamente mientras se estabiliza
        float flickerSpeed = Mathf.Lerp(velocidadFlickerNormal * inestabilidadFlicker * 0.8f, velocidadFlickerNormal, t);
        float noise = Mathf.PerlinNoise(randomOffset, Time.time * flickerSpeed);
        float rangoFlicker = Mathf.Lerp(fuerzaFlickerEspiritu * 0.8f, rangoFlickerNormal, t);

        targetLight.intensity = Mathf.Max(0f, baseIntensity + (noise * 2f - 1f) * rangoFlicker);

        if (t >= 1f)
        {
            estadoActual = EstadoLuz.Normal;
            if (fuenteAudio != null && fuenteAudio.isPlaying)
            {
                // Apagar el sonido del espíritu gradualmente
                StartCoroutine(FadeOutAudio(1f));
            }
        }
    }

    private System.Collections.IEnumerator FadeOutAudio(float duration)
    {
        float startVolume = fuenteAudio.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fuenteAudio.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        fuenteAudio.Stop();
        fuenteAudio.volume = startVolume;
    }

    /// <summary>
    /// Activa externamente la atenuación espiritual de la luz.
    /// Útil para dispararlo desde triggers en UHFPS o colliders.
    /// </summary>
    public void ActivarEfectoEspiritu()
    {
        if (estadoActual != EstadoLuz.Normal) return;

        estadoActual = EstadoLuz.TransicionAtenuar;
        tiempoTransicionActual = 0f;

        // Reproducir sonido de soplido/hum espiritual
        if (fuenteAudio != null && sonidoSoploEspiritu != null)
        {
            fuenteAudio.clip = sonidoSoploEspiritu;
            fuenteAudio.loop = true;
            fuenteAudio.Play();
        }
    }

    /// <summary>
    /// Resetea la luz inmediatamente a su estado normal.
    /// </summary>
    public void ResetearLuz()
    {
        estadoActual = EstadoLuz.Normal;
        targetLight.intensity = intensidadNormal;
        targetLight.color = colorNormal;
        if (fuenteAudio != null)
        {
            fuenteAudio.Stop();
        }
    }

    /// <summary>
    /// Obtiene el estado actual de la simulación de la luz.
    /// </summary>
    public EstadoLuz ObtenerEstadoActual()
    {
        return estadoActual;
    }
}
