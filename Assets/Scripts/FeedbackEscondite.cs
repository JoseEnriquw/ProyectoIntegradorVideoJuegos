using UnityEngine;
using UnityEngine.UI;
using UHFPS.Runtime;

public class FeedbackEscondite : MonoBehaviour
{
    [Header("Configuración de Distancia")]
    [Tooltip("A qué distancia del NPC se empieza a sentir tensión en el escondite.")]
    public float distanciaAlertaMax = 12f;
    [Tooltip("Distancia a la que la tensión es máxima (NPC pegado a la puerta).")]
    public float distanciaAlertaCritica = 3f;

    [Header("Componentes de Audio")]
    [Tooltip("AudioSource dedicado para el bucle del latido.")]
    public AudioSource audioSourceLatidos;
    [Tooltip("Sonido de latido del corazón (en bucle).")]
    public AudioClip clipLatidos;
    [Range(0f, 1f)] public float volumenMaxLatidos = 0.8f;
    
    [Tooltip("AudioSource para efectos puntuales (respiración/suspiro).")]
    public AudioSource audioSourceEfectos;
    [Tooltip("Sonido de suspiro cuando el peligro se aleja.")]
    public AudioClip clipSuspiroAlivio;

    [Header("Componentes Visuales")]
    [Tooltip("Imagen UI de una viñeta roja que ocupará toda la pantalla.")]
    public Image vignetteImage;
    [Range(0f, 1f)] public float maxVignetteAlpha = 0.5f;
    public float velocidadPulsoBase = 2f;

    private PlayerStateMachine playerMachine;
    private bool peligroDetectadoAnteriormente = false;
    private float currentVignetteAlpha = 0f;

    void Start()
    {
        // Enlazar con la máquina de estados del jugador (UHFPS)
        if (PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
        {
            playerMachine = PlayerPresenceManager.Instance.Player.GetComponent<PlayerStateMachine>();
        }

        // Si son el mismo AudioSource o falta el de efectos, creamos uno nuevo en runtime
        // para evitar que el FadeOut de volumen de los latidos silencie el suspiro de alivio.
        if (audioSourceEfectos == null || audioSourceEfectos == audioSourceLatidos)
        {
            AudioSource nuevoSource = gameObject.AddComponent<AudioSource>();
            nuevoSource.playOnAwake = false;
            nuevoSource.loop = false;
            audioSourceEfectos = nuevoSource;
            Debug.Log("[FeedbackEscondite] Se creó un AudioSource separado para los efectos de sonido automáticamente para evitar solapamientos.");
        }

        if (audioSourceLatidos != null && clipLatidos != null)
        {
            audioSourceLatidos.clip = clipLatidos;
            audioSourceLatidos.loop = true;
            audioSourceLatidos.volume = 0f;
        }

        if (vignetteImage != null)
        {
            vignetteImage.color = new Color(0.5f, 0f, 0f, 0f);
            vignetteImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerMachine == null)
        {
            if (PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
                playerMachine = PlayerPresenceManager.Instance.Player.GetComponent<PlayerStateMachine>();
            return;
        }

        // Comprobamos si el jugador está actualmente escondido usando el estado nativo de UHFPS
        bool estaEscondido = playerMachine.IsCurrent(PlayerStateMachine.HIDING_STATE);

        if (!estaEscondido)
        {
            DesactivarEfectosTension();
            peligroDetectadoAnteriormente = false;
            return;
        }

        // Si está escondido, buscamos la distancia al NPC más cercano
        float distanciaNPC = ObtenerDistanciaAlNPCMasCercano();

        if (distanciaNPC < distanciaAlertaMax)
        {
            ProcesarTension(distanciaNPC);
            peligroDetectadoAnteriormente = true;
        }
        else
        {
            // Si antes había peligro y ahora ya no está dentro del rango
            if (peligroDetectadoAnteriormente)
            {
                ReproducirAlivio();
                peligroDetectadoAnteriormente = false;
            }
            DesactivarEfectosTension();
        }
    }

    private float ObtenerDistanciaAlNPCMasCercano()
    {
        NPCStateMachine[] npcs = FindObjectsOfType<NPCStateMachine>();
        if (npcs.Length == 0) return float.MaxValue;

        Vector3 posJugador = playerMachine.transform.position;
        float minDistance = float.MaxValue;

        foreach (var npc in npcs)
        {
            if (npc == null || !npc.gameObject.activeInHierarchy) continue;

            float dist = Vector3.Distance(posJugador, npc.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
            }
        }
        return minDistance;
    }

    private void ProcesarTension(float distancia)
    {
        // Normalizamos la distancia (0 = lejos, 1 = crítico/al lado)
        float factorTension = Mathf.InverseLerp(distanciaAlertaMax, distanciaAlertaCritica, distancia);

        // --- 1. AUDIO DE LATIDOS ---
        if (audioSourceLatidos != null)
        {
            if (!audioSourceLatidos.isPlaying) audioSourceLatidos.Play();

            // Aumenta volumen y velocidad de reproducción (pitch) según cercanía
            audioSourceLatidos.volume = Mathf.Lerp(0.1f, volumenMaxLatidos, factorTension);
            audioSourceLatidos.pitch = Mathf.Lerp(1.0f, 1.7f, factorTension);
        }

        // --- 2. VIÑETA ROJA PULSANTE ---
        if (vignetteImage != null)
        {
            vignetteImage.gameObject.SetActive(true);

            // La velocidad del parpadeo del corazón de la viñeta acelera con el peligro
            float freqPulso = Mathf.Lerp(velocidadPulsoBase, velocidadPulsoBase * 3f, factorTension);
            float oscilacion = (Mathf.Sin(Time.time * freqPulso) + 1f) / 2f; // Valores de 0 a 1

            float targetAlpha = Mathf.Lerp(0.05f, maxVignetteAlpha, factorTension) * oscilacion;
            currentVignetteAlpha = Mathf.Lerp(currentVignetteAlpha, targetAlpha, Time.deltaTime * 5f);

            vignetteImage.color = new Color(0.4f, 0f, 0f, currentVignetteAlpha);
        }
    }

    private void DesactivarEfectosTension()
    {
        // Atenuar latidos gradualmente
        if (audioSourceLatidos != null && audioSourceLatidos.isPlaying)
        {
            audioSourceLatidos.volume = Mathf.Lerp(audioSourceLatidos.volume, 0f, Time.deltaTime * 3f);
            if (audioSourceLatidos.volume < 0.01f) audioSourceLatidos.Stop();
        }

        // Desvanecer viñeta roja
        if (vignetteImage != null && vignetteImage.gameObject.activeSelf)
        {
            currentVignetteAlpha = Mathf.Lerp(currentVignetteAlpha, 0f, Time.deltaTime * 4f);
            vignetteImage.color = new Color(0.4f, 0f, 0f, currentVignetteAlpha);

            if (currentVignetteAlpha < 0.01f) vignetteImage.gameObject.SetActive(false);
        }
    }

    private void ReproducirAlivio()
    {
        if (audioSourceEfectos != null && clipSuspiroAlivio != null)
        {
            audioSourceEfectos.PlayOneShot(clipSuspiroAlivio);
        }
    }
}
