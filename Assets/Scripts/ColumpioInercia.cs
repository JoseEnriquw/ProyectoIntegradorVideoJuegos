using UnityEngine;

public class ColumpioInercia : MonoBehaviour
{
    public enum EjeRotacion
    {
        X,
        Y,
        Z
    }

    public enum ModoImpulso
    {
        Automatico,
        ManualTeclado,
        Ninguno
    }

    public enum ModoAudio
    {
        [Tooltip("El audio se reproduce en bucle continuo y su volumen/pitch se modulan según la velocidad del columpio.")]
        LoopModuladoVelocidad,
        
        [Tooltip("El audio se dispara (Play) como un efecto de un solo golpe cuando el columpio llega a su punto más alto (máxima amplitud y velocidad cero).")]
        DispararEnPico,

        [Tooltip("El audio se dispara cuando el columpio pasa por el centro (ángulo cero, máxima velocidad).")]
        DispararEnCentro
    }

    [Header("Configuración de Rotación")]
    [Tooltip("El eje local alrededor del cual rotará el columpio. (Si tu modelo rota hacia adelante/atrás en Y, selecciona Y).")]
    public EjeRotacion ejeDeRotacion = EjeRotacion.Y;

    [Header("Físicas del Péndulo Real")]
    [Tooltip("Fuerza de la gravedad en m/s² (típicamente 9.81). Determina la aceleración de caída.")]
    public float gravedad = 9.81f;

    [Tooltip("Longitud física del columpio en metros (del pivote al asiento). Determina el ritmo de oscilación: más largo = más lento.")]
    [Range(0.5f, 10f)]
    public float longitudPendulo = 2.5f;

    [Tooltip("Peso/Masa del columpio en kg. Afecta la inercia: más pesado = tarda más en ganar velocidad y en frenarse por fricción. Más liviano = reacciona rápido y se frena rápido.")]
    [Range(0.1f, 100f)]
    public float masa = 5.0f;

    [Tooltip("Fricción o resistencia del aire. Valores más altos detienen el columpio más rápido.")]
    [Range(0f, 5f)]
    public float amortiguacion = 0.2f;

    [Tooltip("Límite de velocidad angular (en grados/seg) para evitar giros descontrolados de 360 grados.")]
    public float limiteVelocidad = 90f;

    [Header("Impulso / Fuerza")]
    [Tooltip("El modo en el que el columpio gana impulso: Automático (solo), Manual (teclas del jugador) o Ninguno (inercia pura).")]
    public ModoImpulso modoImpulso = ModoImpulso.Automatico;

    [Tooltip("Fuerza aplicada para ganar impulso en cada oscilación (en Newtons). Si la masa es baja, esta fuerza moverá el columpio con mayor rapidez.")]
    public float fuerzaImpulso = 3f;

    [Tooltip("Ángulo máximo en grados (amplitud) que alcanzará el columpio en modo automático.")]
    [Range(5f, 90f)]
    public float anguloMaximoAutomatico = 30f;

    [Header("Efecto Terror (Viento / Fantasma)")]
    [Tooltip("Activa una fuerza irregular y continua (ruido Perlin) que empuja el columpio para que parezca que se mueve solo (fuerzas paranormales o viento) de forma no uniforme.")]
    public bool efectoFantasma = true;

    [Tooltip("Intensidad de la fuerza del fantasma/viento.")]
    public float fuerzaFantasma = 0.8f;

    [Tooltip("Velocidad de cambio o turbulencia de la fuerza del fantasma.")]
    public float frecuenciaFantasma = 0.4f;

    [Header("Control Manual (Teclado)")]
    [Tooltip("¿Utilizar los ejes de entrada definidos en Unity (ej: Horizontal o Vertical)?")]
    public bool usarEjesInput = false;

    [Tooltip("Nombre del eje de Unity a utilizar (normalmente Vertical para W/S o flechas).")]
    public string nombreEjeInput = "Vertical";

    [Tooltip("Tecla para impulsar hacia adelante.")]
    public KeyCode teclaAdelante = KeyCode.W;

    [Tooltip("Tecla para impulsar hacia atrás.")]
    public KeyCode teclaAtras = KeyCode.S;

    [Header("Efectos de Sonido (Sincronización)")]
    [Tooltip("Componente AudioSource para reproducir el chirrido del columpio.")]
    public AudioSource fuenteAudioCreak;

    [Tooltip("Modo de reproducción y sincronización del sonido con el movimiento físico.")]
    public ModoAudio modoAudio = ModoAudio.LoopModuladoVelocidad;

    [Tooltip("Punto de inicio en segundos dentro del clip de audio para reproducir (útil para extraer un chirrido específico de un archivo largo como columpio.wav).")]
    public float puntoInicioAudio = 0.5f;

    [Tooltip("Duración en segundos del chirrido a reproducir antes de silenciarlo gradualmente (sólo para modos Disparar).")]
    public float duracionAudio = 1.3f;

    [Tooltip("¿Modular el volumen del disparo según la amplitud/altura del columpio? (Más alto = chirrido más fuerte; casi quieto = silencio).")]
    public bool modularVolumenPorAmplitud = true;

    [Tooltip("Ángulo mínimo (en grados) necesario para que se dispare el sonido. Evita chirridos molestos cuando el columpio casi no se mueve.")]
    public float umbralAnguloAudio = 2.0f;

    [Tooltip("Volumen máximo que alcanzará el audio en su velocidad/amplitud máxima.")]
    [Range(0f, 1f)]
    public float volumenMaximo = 0.8f;

    [Tooltip("Velocidad de referencia (en grados/seg) para modular el sonido en modo Loop. A esta velocidad el sonido estará al volumen máximo.")]
    public float velocidadReferenciaAudio = 20f;

    [Tooltip("Pitch mínimo del sonido cuando se mueve despacio.")]
    [Range(0.5f, 1.5f)]
    public float pitchMinimo = 0.8f;

    [Tooltip("Pitch máximo del sonido cuando se mueve rápido.")]
    [Range(0.5f, 1.5f)]
    public float pitchMaximo = 1.2f;

    // Variables internas (simuladas en radianes para precisión física)
    private float anguloActual = 0f; // Radianes
    private float velocidadAngular = 0f; // Radianes/seg
    private Quaternion rotacionInicial;

    // Variables para detección de eventos físicos
    private float anguloAnterior = 0f;
    private float velocidadAnterior = 0f;
    private float tiempoUltimoDisparo = -99f;
    private bool reproduciendoSegmento = false;

    void Start()
    {
        // Guardamos la rotación inicial del transform para no perder la orientación dada en la escena
        rotacionInicial = transform.localRotation;

        // Si hay una fuente de audio configurada y usamos modo Loop, la iniciamos
        if (fuenteAudioCreak != null)
        {
            if (modoAudio == ModoAudio.LoopModuladoVelocidad)
            {
                fuenteAudioCreak.loop = true;
                if (!fuenteAudioCreak.isPlaying)
                {
                    fuenteAudioCreak.Play();
                }
                fuenteAudioCreak.volume = 0f;
            }
            else
            {
                // En modo de disparo, no queremos que el AudioSource cicle por su cuenta de forma infinita
                fuenteAudioCreak.loop = false;
            }
        }
    }

    void Update()
    {
        // Limitamos el deltaTime para evitar saltos bruscos si bajan los FPS
        float dt = Mathf.Min(Time.deltaTime, 0.1f);

        // Guardamos los valores del frame anterior para detectar cruces y picos
        anguloAnterior = anguloActual;
        velocidadAnterior = velocidadAngular;

        // 1. Simular la física de restauración del péndulo real (Gravedad)
        float aceleracionRestauradora = -(gravedad / longitudPendulo) * Mathf.Sin(anguloActual);
        velocidadAngular += aceleracionRestauradora * dt;

        // 2. Aplicar fricción / resistencia del aire modulada por la masa
        float amortiguacionEfectiva = amortiguacion / masa;
        velocidadAngular -= velocidadAngular * amortiguacionEfectiva * dt;

        // 3. Aplicar efecto de viento / fantasma (fuerza paranormal irregular)
        if (efectoFantasma)
        {
            float ruido = Mathf.PerlinNoise(Time.time * frecuenciaFantasma, 99.99f) * 2f - 1f; // [-1, 1]
            float aceleracionFantasma = (ruido * fuerzaFantasma) / (masa * longitudPendulo);
            velocidadAngular += aceleracionFantasma * dt;
        }

        // 4. Aplicar impulso de acuerdo al modo seleccionado
        switch (modoImpulso)
        {
            case ModoImpulso.Automatico:
                ActualizarImpulsoAutomatico(dt);
                break;
            case ModoImpulso.ManualTeclado:
                ActualizarImpulsoManual(dt);
                break;
            case ModoImpulso.Ninguno:
                break;
        }

        // 5. Limitar la velocidad angular en radianes
        float limiteVelocidadRad = limiteVelocidad * Mathf.Deg2Rad;
        velocidadAngular = Mathf.Clamp(velocidadAngular, -limiteVelocidadRad, limiteVelocidadRad);

        // 6. Aplicar la velocidad angular al ángulo actual
        anguloActual += velocidadAngular * dt;

        // 7. Convertir ángulo actual a grados para la rotación de Unity
        float anguloGrados = anguloActual * Mathf.Rad2Deg;

        // Aplicar la rotación final respetando la rotación inicial de la escena
        Quaternion rotacionOffset = Quaternion.identity;
        switch (ejeDeRotacion)
        {
            case EjeRotacion.X:
                rotacionOffset = Quaternion.Euler(anguloGrados, 0, 0);
                break;
            case EjeRotacion.Y:
                rotacionOffset = Quaternion.Euler(0, anguloGrados, 0);
                break;
            case EjeRotacion.Z:
                rotacionOffset = Quaternion.Euler(0, 0, anguloGrados);
                break;
        }
        transform.localRotation = rotacionInicial * rotacionOffset;

        // 8. Controlar la reproducción de audio (Detección de timings físicos)
        ControlarSistemaAudio(dt);
    }

    private void ActualizarImpulsoAutomatico(float dt)
    {
        float amplitudActual = Mathf.Abs(anguloActual);
        float anguloMaximoRad = anguloMaximoAutomatico * Mathf.Deg2Rad;

        if (Mathf.Abs(velocidadAngular) < 0.001f && amplitudActual < 0.01f)
        {
            velocidadAngular = 0.05f;
            return;
        }

        if (amplitudActual < anguloMaximoRad)
        {
            float factorAmplitud = Mathf.Clamp01(1f - (amplitudActual / anguloMaximoRad));
            float factorDeTiming = Mathf.Clamp01(Mathf.Cos(anguloActual));
            float aceleracionImpulso = fuerzaImpulso / (masa * longitudPendulo);

            float direccionImpulso = Mathf.Sign(velocidadAngular);
            velocidadAngular += direccionImpulso * aceleracionImpulso * factorAmplitud * factorDeTiming * dt;
        }
    }

    private void ActualizarImpulsoManual(float dt)
    {
        float entrada = 0f;

        if (usarEjesInput)
        {
            entrada = Input.GetAxis(nombreEjeInput);
        }
        else
        {
            if (Input.GetKey(teclaAdelante)) entrada = 1f;
            else if (Input.GetKey(teclaAtras)) entrada = -1f;
        }

        if (Mathf.Abs(entrada) > 0.01f)
        {
            float amplitudActual = Mathf.Abs(anguloActual);
            if (Mathf.Abs(velocidadAngular) < 0.001f && amplitudActual < 0.01f)
            {
                velocidadAngular = entrada * 0.05f;
            }
            else
            {
                float aceleracionImpulso = (entrada * fuerzaImpulso) / (masa * longitudPendulo);
                velocidadAngular += aceleracionImpulso * dt;
            }
        }
    }

    private void ControlarSistemaAudio(float dt)
    {
        if (fuenteAudioCreak == null || fuenteAudioCreak.clip == null) return;

        if (modoAudio == ModoAudio.LoopModuladoVelocidad)
        {
            ActualizarAudioCreak();
        }
        else
        {
            // Procesamiento para modos de disparo por eventos físicos
            float anguloGrados = Mathf.Abs(anguloActual * Mathf.Rad2Deg);
            
            // Detectar Pico (La velocidad cruza por cero al cambiar de dirección)
            if (modoAudio == ModoAudio.DispararEnPico)
            {
                // Si la velocidad angular cambia de signo
                if (Mathf.Sign(velocidadAngular) != Mathf.Sign(velocidadAnterior) && Mathf.Abs(velocidadAngular) < 0.5f)
                {
                    if (anguloGrados >= umbralAnguloAudio)
                    {
                        DispararChirrido(anguloGrados);
                    }
                }
            }
            // Detectar Centro (El ángulo cruza por cero)
            else if (modoAudio == ModoAudio.DispararEnCentro)
            {
                // Si el ángulo cambia de signo
                if (Mathf.Sign(anguloActual) != Mathf.Sign(anguloAnterior))
                {
                    if (anguloGrados >= umbralAnguloAudio || Mathf.Abs(velocidadAngular * Mathf.Rad2Deg) > 5f)
                    {
                        DispararChirrido(anguloGrados);
                    }
                }
            }

            // Gestionar el fin de la reproducción del segmento de audio (Fading)
            if (reproduciendoSegmento)
            {
                float tiempoTranscurrido = Time.time - tiempoUltimoDisparo;
                if (tiempoTranscurrido > duracionAudio)
                {
                    // Desvanecimiento suave al final del tiempo del segmento para evitar clics de audio
                    fuenteAudioCreak.volume = Mathf.Lerp(fuenteAudioCreak.volume, 0f, dt * 12f);
                    if (fuenteAudioCreak.volume < 0.01f)
                    {
                        fuenteAudioCreak.Stop();
                        reproduciendoSegmento = false;
                    }
                }
            }
        }
    }

    private void DispararChirrido(float amplitudGrados)
    {
        if (fuenteAudioCreak == null) return;

        // Evitar solapamientos muy seguidos (tiempo mínimo de re-disparo de 0.4s)
        if (Time.time - tiempoUltimoDisparo < 0.4f) return;

        tiempoUltimoDisparo = Time.time;
        reproduciendoSegmento = true;

        // Calcular volumen en función de la altura actual de oscilación
        float factorVolumen = 1f;
        if (modularVolumenPorAmplitud)
        {
            // Mapeamos el ángulo de oscilación actual respecto al ángulo máximo automático
            float refMax = modoImpulso == ModoImpulso.Automatico ? anguloMaximoAutomatico : 30f;
            factorVolumen = Mathf.Clamp01(amplitudGrados / refMax);
        }

        fuenteAudioCreak.volume = factorVolumen * volumenMaximo;
        
        // Modulamos ligeramente el pitch para evitar monotonía
        float velocidadGrados = Mathf.Abs(velocidadAngular * Mathf.Rad2Deg);
        float factorVelocidad = Mathf.Clamp01(velocidadGrados / velocidadReferenciaAudio);
        fuenteAudioCreak.pitch = Mathf.Lerp(pitchMinimo, pitchMaximo, factorVelocidad) + Random.Range(-0.05f, 0.05f);

        // Ajustamos la aguja de reproducción al punto de inicio deseado
        fuenteAudioCreak.time = Mathf.Clamp(puntoInicioAudio, 0f, fuenteAudioCreak.clip.length - 0.1f);
        fuenteAudioCreak.Play();
    }

    private void ActualizarAudioCreak()
    {
        float velocidadAbsolutaGrados = Mathf.Abs(velocidadAngular) * Mathf.Rad2Deg;

        if (velocidadAbsolutaGrados > 0.1f)
        {
            if (!fuenteAudioCreak.isPlaying)
            {
                fuenteAudioCreak.Play();
            }

            float factorVelocidad = Mathf.Clamp01(velocidadAbsolutaGrados / velocidadReferenciaAudio);

            fuenteAudioCreak.volume = Mathf.Lerp(0f, volumenMaximo, factorVelocidad);
            fuenteAudioCreak.pitch = Mathf.Lerp(pitchMinimo, pitchMaximo, factorVelocidad);
        }
        else
        {
            fuenteAudioCreak.volume = Mathf.Lerp(fuenteAudioCreak.volume, 0f, Time.deltaTime * 5f);
            if (fuenteAudioCreak.volume < 0.01f && fuenteAudioCreak.isPlaying)
            {
                fuenteAudioCreak.Stop();
            }
        }
    }

    /// <summary>
    /// Permite a otros scripts empujar el columpio agregando velocidad de golpe (fuerza en grados/seg).
    /// </summary>
    public void AgregarImpulso(float fuerza)
    {
        velocidadAngular += fuerza * Mathf.Deg2Rad;
    }

    /// <summary>
    /// Resetea el columpio a su posición de reposo original.
    /// </summary>
    public void DetenerColumpio()
    {
        anguloActual = 0f;
        velocidadAngular = 0f;
        transform.localRotation = rotacionInicial;
        if (fuenteAudioCreak != null)
        {
            fuenteAudioCreak.volume = 0f;
            fuenteAudioCreak.Stop();
        }
    }

    /// <summary>
    /// Retorna el ángulo de oscilación actual del columpio en grados.
    /// </summary>
    public float ObtenerAnguloActual()
    {
        return anguloActual * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Retorna la velocidad angular actual del columpio en grados por segundo.
    /// </summary>
    public float ObtenerVelocidadAngular()
    {
        return velocidadAngular * Mathf.Rad2Deg;
    }
}
