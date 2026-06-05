using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UHFPS.Runtime; // Requerido para integrarse con UHFPS (Ultimate Horror FPS Template)

/// <summary>
/// Secuencia premium para simular la ingesta de un suero, el colapso/desmayo del jugador,
/// una fase de inconsciencia con pantalla en negro, y su posterior recuperación
/// desbloqueando el poder de cambio de realidad.
/// </summary>
public class SecuenciaSueroRealidad : MonoBehaviour
{
    [Header("Referencias de Cámara")]
    [Tooltip("La cámara del jugador. Si se deja vacía, se buscará Camera.main automáticamente en Awake.")]
    public Transform camaraJugador;

    [Header("Configuración del Desmayo")]
    [Tooltip("Tiempo en segundos que tardará el jugador en caer al suelo.")]
    [Range(0.1f, 5f)]
    public float tiempoCaida = 1.5f;

    [Tooltip("Tiempo en segundos que permanecerá desmayado con la pantalla en negro.")]
    [Range(0.5f, 15f)]
    public float tiempoInconsciencia = 3.5f;

    [Tooltip("Tiempo en segundos que tardará el jugador en levantarse.")]
    [Range(0.1f, 5f)]
    public float tiempoLevantarse = 2.0f;

    [Tooltip("Velocidad del fundido a negro de la pantalla (GameManager.StartBackgroundFade).")]
    public float velocidadFade = 1.5f;

    [Header("Animación Programática de Caída")]
    [Tooltip("Desfase de altura al caer (negativo para bajar la cámara cerca del suelo).")]
    public float desfaseAlturaCaida = -1.3f;

    [Tooltip("Rotación en el eje Z al caer (simula caer de lado en el piso).")]
    public float rotacionZCaida = 75f;

    [Tooltip("Rotación en el eje X al caer (simula mirar ligeramente hacia abajo).")]
    public float rotacionXCaida = 15f;

    [Header("Poder / Integración")]
    [Tooltip("Referencia al controlador del poder de cambio de realidad.")]
    public ControladorCambioRealidad controladorPoder;

    [Tooltip("¿Desbloquear e iniciar el poder del cambio de realidad inmediatamente al levantarse?")]
    public bool desbloquearAlLevantarse = true;

    [Header("Efecto de Brazos al Levantarse")]
    [Tooltip("El objeto de los brazos (GameObject) que se debe activar al levantarse.")]
    public GameObject brazosObjeto;

    [Tooltip("Renderers de los brazos para aplicar el dissolve. Si está vacío, se buscarán automáticamente en los hijos de brazosObjeto.")]
    public Renderer[] brazosRenderers;

    [Tooltip("El nombre del parámetro float de dissolve en tu Shader Graph de los brazos.")]
    public string parametroDissolveBrazos = "_Dissolve";

    [Tooltip("Curva para controlar el dissolve de los brazos. (1 = invisible, 0 = visible)")]
    public AnimationCurve curvaDissolveBrazos = new AnimationCurve(
        new Keyframe(0f, 1f),       // Comienza disuelto/invisible
        new Keyframe(0.25f, 0f),    // Aparece rápido (sólido)
        new Keyframe(0.75f, 0f),    // Se mantiene visible
        new Keyframe(1f, 1f)        // Desaparece/se disuelve al finalizar
    );

    [Tooltip("Nombre del trigger del Animator de los brazos para reproducir su animación al activarse (opcional).")]
    public string triggerAnimacionBrazos = "Activar";

    [Tooltip("Duración en segundos del efecto de los brazos (aparición y desaparición por dissolve) después de levantarse.")]
    [Range(0.1f, 15f)]
    public float duracionEfectoBrazos = 4.0f;

    [Header("Depuración / Pruebas")]
    [Tooltip("¿Ejecutar la secuencia automáticamente al iniciar la escena para probarla rápido sin usar el inventario?")]
    public bool ejecutarAlIniciar = false;

    [Header("Eventos de la Secuencia")]
    public UnityEvent AlTomarSuero;
    public UnityEvent AlDesmayarse;
    public UnityEvent AlDespertar;

    private Vector3 posicionInicialLocal;
    private Quaternion rotacionInicialLocal;
    private bool secuenciaEnProgreso = false;

    private MaterialPropertyBlock propBlockBrazos;
    private int idPropDissolveBrazos;
    private List<Renderer> renderersBrazosCacheados = new List<Renderer>();
    private Animator animatorBrazos;

    private void Awake()
    {
        // Si no se asignó la cámara, buscarla de forma automática
        if (camaraJugador == null && Camera.main != null)
        {
            camaraJugador = Camera.main.transform;
        }

        if (camaraJugador != null)
        {
            // Almacenar el estado local de cabeza erguida inicial
            posicionInicialLocal = camaraJugador.localPosition;
            rotacionInicialLocal = camaraJugador.localRotation;
        }
        else
        {
            Debug.LogWarning("[SecuenciaSuero] No se pudo encontrar la cámara del jugador. Asígnala manualmente en el inspector.", this);
        }

        // Si tenemos el controlador asignado, asegurarse de que el poder esté bloqueado de inicio
        if (controladorPoder != null && desbloquearAlLevantarse)
        {
            controladorPoder.habilitarTeclaPrueba = false;
        }

        // Inicializar MaterialPropertyBlock y renderers para los brazos
        propBlockBrazos = new MaterialPropertyBlock();
        idPropDissolveBrazos = Shader.PropertyToID(parametroDissolveBrazos);

        if (brazosObjeto != null)
        {
            animatorBrazos = brazosObjeto.GetComponent<Animator>();

            if (brazosRenderers != null && brazosRenderers.Length > 0)
            {
                renderersBrazosCacheados.AddRange(brazosRenderers);
            }
            else
            {
                renderersBrazosCacheados.AddRange(brazosObjeto.GetComponentsInChildren<Renderer>(true));
            }

            // Inicializar brazos ocultos y disueltos (completamente invisibles)
            ActualizarDissolveBrazos(1f);
            brazosObjeto.SetActive(false);
        }
    }

    private void Start()
    {
        if (ejecutarAlIniciar)
        {
            // Esperar medio segundo para dar tiempo a que los managers e inputs de UHFPS estén listos
            Invoke(nameof(TomarSuero), 0.5f);
        }
    }

    /// <summary>
    /// Inicia la secuencia completa de ingesta del suero.
    /// Llama a esta función desde el evento 'On Use' de tu consumible en el inventario o desde un Trigger de UHFPS.
    /// </summary>
    [ContextMenu("Tomar Suero")]
    public void TomarSuero()
    {
        if (secuenciaEnProgreso) return;

        StartCoroutine(RutinaSecuenciaSuero());
    }

    private IEnumerator RutinaSecuenciaSuero()
    {
        secuenciaEnProgreso = true;
        AlTomarSuero?.Invoke();
        Debug.Log("[SecuenciaSuero] Jugador consume el suero. Iniciando colapso...");

        Vector3 posicionInicialMundo = Vector3.zero;
        Vector2 rotacionLookInicial = Vector2.zero;
        CharacterController playerCollider = null;
        PlayerStateMachine stateMachine = null;

        // Guardar la posición y rotación exactas del jugador al inicio
        if (PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
        {
            posicionInicialMundo = PlayerPresenceManager.Instance.Player.transform.position;
            if (PlayerPresenceManager.Instance.LookController != null)
            {
                rotacionLookInicial = PlayerPresenceManager.Instance.LookController.LookRotation;
            }
        }

        // 1. Congelar los controles físicos del jugador
        if (PlayerPresenceManager.HasReference)
        {
            PlayerPresenceManager.Instance.PlayerIsUnlocked = false;
            PlayerPresenceManager.Instance.FreezePlayer(true);
            
            // Deshabilitar temporalmente el controlador de vista de mouse para que no sobreescriba la rotación de la cámara
            if (PlayerPresenceManager.Instance.LookController != null)
            {
                PlayerPresenceManager.Instance.LookController.enabled = false;
            }

            // Deshabilitar el PlayerStateMachine para evitar que los estados sobreescriban la posición de la cámara durante la secuencia
            if (PlayerPresenceManager.Instance.StateMachine != null)
            {
                stateMachine = PlayerPresenceManager.Instance.StateMachine;
                stateMachine.enabled = false;
            }

            // Deshabilitar el CharacterController para evitar desplazamientos por física/gravedad durante el desmayo
            if (PlayerPresenceManager.Instance.StateMachine != null)
            {
                playerCollider = PlayerPresenceManager.Instance.StateMachine.PlayerCollider;
                if (playerCollider != null)
                {
                    playerCollider.enabled = false;
                }
            }
        }

        // Determinar qué transform vamos a animar (preferir el CameraHolder para no entrar en conflicto con Cinemachine)
        Transform transformAAnimar = null;
        if (PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.PlayerManager != null)
        {
            transformAAnimar = PlayerPresenceManager.Instance.PlayerManager.CameraHolder;
        }
        
        if (transformAAnimar == null)
        {
            transformAAnimar = camaraJugador;
        }

        if (transformAAnimar != null)
        {
            posicionInicialLocal = transformAAnimar.localPosition;
            rotacionInicialLocal = transformAAnimar.localRotation;
        }

        // 2. Fundido a negro de la pantalla global (UHFPS GameManager)
        if (GameManager.HasReference)
        {
            // El fade a negro en UHFPS se activa pasando false
            StartCoroutine(GameManager.Instance.StartBackgroundFade(false, fadeSpeed: velocidadFade));
        }

        // 3. Simular la caída de la cámara al piso
        float tiempo = 0f;
        Vector3 posicionCaida = posicionInicialLocal + new Vector3(0, desfaseAlturaCaida, 0);
        Quaternion rotacionCaida = rotacionInicialLocal * Quaternion.Euler(rotacionXCaida, 0, rotacionZCaida);

        while (tiempo < tiempoCaida)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / tiempoCaida);
            float tSuave = Mathf.SmoothStep(0f, 1f, t);

            if (transformAAnimar != null)
            {
                transformAAnimar.localPosition = Vector3.Lerp(posicionInicialLocal, posicionCaida, tSuave);
                transformAAnimar.localRotation = Quaternion.Slerp(rotacionInicialLocal, rotacionCaida, tSuave);
            }

            yield return null;
        }

        // Forzar valores exactos de caída
        if (transformAAnimar != null)
        {
            transformAAnimar.localPosition = posicionCaida;
            transformAAnimar.localRotation = rotacionCaida;
        }

        AlDesmayarse?.Invoke();
        Debug.Log("[SecuenciaSuero] Jugador desmayado. Pantalla en negro...");

        // 4. Esperar el tiempo de unconsciousness del desmayo
        yield return new WaitForSeconds(tiempoInconsciencia);

        Debug.Log("[SecuenciaSuero] El jugador comienza a recuperar el conocimiento. Levantándose...");

        // 5. Fundido de vuelta a la luz
        if (GameManager.HasReference)
        {
            // El fade de vuelta a la visión normal se activa pasando true
            StartCoroutine(GameManager.Instance.StartBackgroundFade(true, fadeSpeed: velocidadFade));
        }

        // 6. Simular levantarse del suelo
        tiempo = 0f;
        while (tiempo < tiempoLevantarse)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / tiempoLevantarse);
            float tSuave = Mathf.SmoothStep(0f, 1f, t);

            if (transformAAnimar != null)
            {
                transformAAnimar.localPosition = Vector3.Lerp(posicionCaida, posicionInicialLocal, tSuave);
                transformAAnimar.localRotation = Quaternion.Slerp(rotacionCaida, rotacionInicialLocal, tSuave);
            }

            yield return null;
        }

        // Asegurar la restauración perfecta de la cabeza del jugador
        if (transformAAnimar != null)
        {
            transformAAnimar.localPosition = posicionInicialLocal;
            transformAAnimar.localRotation = rotacionInicialLocal;
        }

        // 6.5. Efecto de brazos cuando ya está levantado
        if (brazosObjeto != null)
        {
            float dissolveInicial = curvaDissolveBrazos != null && curvaDissolveBrazos.length > 0 
                ? curvaDissolveBrazos.Evaluate(0f) 
                : 1f;
            ActualizarDissolveBrazos(dissolveInicial);
            brazosObjeto.SetActive(true);
            Debug.Log("[SecuenciaSuero] Jugador levantado. Iniciando efecto de brazos...");

            if (animatorBrazos != null && !string.IsNullOrEmpty(triggerAnimacionBrazos))
            {
                animatorBrazos.SetTrigger(triggerAnimacionBrazos);
                Debug.Log($"[SecuenciaSuero] Disparando trigger de animación de brazos: {triggerAnimacionBrazos}");
            }

            // Bucle del efecto de los brazos
            tiempo = 0f;
            while (tiempo < duracionEfectoBrazos)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracionEfectoBrazos);

                if (curvaDissolveBrazos != null && curvaDissolveBrazos.length > 0)
                {
                    float valorDissolve = curvaDissolveBrazos.Evaluate(t);
                    ActualizarDissolveBrazos(valorDissolve);
                }

                yield return null;
            }

            // Ocultar brazos al finalizar y asegurar que queden disueltos/invisibles
            float dissolveFinal = curvaDissolveBrazos != null && curvaDissolveBrazos.length > 0 
                ? curvaDissolveBrazos.Evaluate(1f) 
                : 1f;
            ActualizarDissolveBrazos(dissolveFinal);
            brazosObjeto.SetActive(false);
            Debug.Log("[SecuenciaSuero] Efecto de brazos finalizado. Brazos desactivados y ocultados.");
        }

        // 7. Habilitar y desbloquear el poder del Cambio de Realidad
        if (controladorPoder != null && desbloquearAlLevantarse)
        {
            controladorPoder.habilitarTeclaPrueba = true;
            Debug.Log("[SecuenciaSuero] ¡Poder de cambio de realidad desbloqueado!");
        }

        // 8. Reactivar controles de UHFPS
        if (PlayerPresenceManager.HasReference)
        {
            // Teletransportar al jugador de vuelta a la posición inicial guardada para corregir cualquier desplazamiento por físicas
            PlayerPresenceManager.Instance.SetPlayerPositionAndLook(posicionInicialMundo, rotacionLookInicial);

            // Reactivar el CharacterController antes de desbloquear al jugador
            if (playerCollider != null)
            {
                playerCollider.enabled = true;
                Physics.SyncTransforms(); // Sincronizar la posición física en Unity
            }

            // Reactivar el PlayerStateMachine
            if (stateMachine != null)
            {
                stateMachine.enabled = true;
            }

            if (PlayerPresenceManager.Instance.LookController != null)
            {
                PlayerPresenceManager.Instance.LookController.enabled = true;
            }
            
            PlayerPresenceManager.Instance.UnlockPlayer();

            // Esperar a que la rutina asíncrona de desbloqueo finalice para evitar condiciones de carrera con el cursor
            while (!PlayerPresenceManager.Instance.PlayerIsUnlocked)
            {
                yield return null;
            }
        }

        AlDespertar?.Invoke();
        secuenciaEnProgreso = false;
        Debug.Log("[SecuenciaSuero] Secuencia finalizada. Control devuelto al jugador.");
    }

    private void ActualizarDissolveBrazos(float valorDissolve)
    {
        if (renderersBrazosCacheados == null) return;
        for (int i = 0; i < renderersBrazosCacheados.Count; i++)
        {
            Renderer r = renderersBrazosCacheados[i];
            if (r != null)
            {
                r.GetPropertyBlock(propBlockBrazos);
                propBlockBrazos.SetFloat(idPropDissolveBrazos, valorDissolve);
                r.SetPropertyBlock(propBlockBrazos);
            }
        }
    }
}
