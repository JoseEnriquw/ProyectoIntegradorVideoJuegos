using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering; // Requerido para manipular Volumes de Post-Processing de URP
using UnityEngine.Rendering.Universal; // Requerido para URP Decal Projector

/// <summary>
/// Controlador premium para cambiar de realidad con efecto de dissolve sincronizado, Post-Processing dinámico y SFX.
/// Maneja la activación, desactivación y transición de materiales sin clonar assets en memoria
/// utilizando MaterialPropertyBlocks, e interpola perfiles de Post-Processing y reproduce sonidos para un impacto AAA.
/// </summary>
public class ControladorCambioRealidad : MonoBehaviour
{
    public static ControladorCambioRealidad Instancia { get; private set; }

    [Header("Objetos de cada Realidad")]
    [Tooltip("Objetos de la realidad 'linda' (normal).")]
    public List<GameObject> objetosLindos = new List<GameObject>();

    [Tooltip("Objetos de la realidad 'podrida' (decrépita).")]
    public List<GameObject> objetosPodridos = new List<GameObject>();

    [Header("Configuración del Efecto de Dissolve")]
    [Tooltip("Duración en segundos de la transición de dissolve.")]
    [Range(0.1f, 15f)]
    public float tiempoTransicion = 1.5f;

    [Tooltip("El nombre del parámetro float en tu Shader Graph de Dissolve.")]
    public string parametroDissolve = "_Dissolve";

    [Header("Post-Processing (URP Volumes)")]
    [Tooltip("Volume con el perfil de color cálido/saturado para la realidad Linda.")]
    public Volume volumenLindo;

    [Tooltip("Volume con el perfil de color verdoso/frío/oscuro para la realidad Podrida.")]
    public Volume volumenPodrido;

    [Tooltip("Volume con efectos fuertes de impacto (Chromatic Aberration, Lens Distortion, Vignette en valores máximos).")]
    public Volume volumenTransicion;

    [Tooltip("Curva para definir la intensidad de la distorsión del volumen de transición. El pico representa el centro del cambio.")]
    public AnimationCurve curvaTransicion = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 4f),
        new Keyframe(0.5f, 1f, 0f, 0f),
        new Keyframe(1f, 0f, -4f, 0f)
    );

    [Header("Efectos de Sonido (SFX)")]
    [Tooltip("El AudioSource que reproducirá el sonido. Si se deja vacío, se buscará o añadirá uno en este objeto.")]
    public AudioSource audioSource;

    [Tooltip("Efecto de sonido que se reproduce al cambiar al mundo Podrido.")]
    public AudioClip sfxMundoPodrido;

    [Tooltip("Efecto de sonido que se reproduce al cambiar al mundo Lindo.")]
    public AudioClip sfxMundoLindo;

    [Range(0f, 1f)]
    [Tooltip("Volumen general de reproducción de los efectos de sonido.")]
    public float volumenSFX = 1f;

    [Header("Pruebas / Input")]
    [Tooltip("¿Habilitar tecla rápida de prueba?")]
    public bool habilitarTeclaPrueba = true;
    
    [Tooltip("Tecla para alternar la realidad durante el gameplay.")]
    public KeyCode teclaPrueba = KeyCode.G;

    [Header("Estado Inicial")]
    [Tooltip("¿Comenzar en la realidad podrida?")]
    public bool iniciarEnPodrido = false;

    private bool esMundoPodrido = false;
    public bool EsMundoPodrido => esMundoPodrido;
    
    private Coroutine rutinaTransicion;
    private MaterialPropertyBlock propBlock;
    private int idPropDissolve;

    // Listas internas de renderers para no hacer GetComponent en tiempo de ejecución
    private List<Renderer> renderersLindos = new List<Renderer>();
    private List<Renderer> renderersPodridos = new List<Renderer>();

    // Listas internas de decales para no hacer GetComponent en tiempo de ejecución
    private List<DecalProjector> decalsLindos = new List<DecalProjector>();
    private List<DecalProjector> decalsPodridos = new List<DecalProjector>();

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
        }

        propBlock = new MaterialPropertyBlock();
        idPropDissolve = Shader.PropertyToID(parametroDissolve);

        // Pre-cachear los renderers para optimizar rendimiento
        CachearRenderers(objetosLindos, renderersLindos);
        CachearRenderers(objetosPodridos, renderersPodridos);

        // Pre-cachear los decales para optimizar rendimiento
        CachearDecals(objetosLindos, decalsLindos);
        CachearDecals(objetosPodridos, decalsPodridos);

        // Asegurar la existencia de un AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Sonido 2D por defecto para impacto estéreo parejo
    }

    private void Start()
    {
        esMundoPodrido = iniciarEnPodrido;
        EstablecerEstadoInicial();
    }

    private void Update()
    {
        if (habilitarTeclaPrueba && Input.GetKeyDown(teclaPrueba))
        {
            AlternarRealidad();
        }
    }

    private void CachearRenderers(List<GameObject> objetos, List<Renderer> listaDestino)
    {
        foreach (var obj in objetos)
        {
            if (obj != null)
            {
                // Buscar renderers en el objeto y en todos sus hijos (activos o inactivos)
                listaDestino.AddRange(obj.GetComponentsInChildren<Renderer>(true));
            }
        }
    }

    private void EstablecerEstadoInicial()
    {
        // Activar la realidad inicial y desactivar la otra
        foreach (var obj in objetosLindos)
        {
            if (obj != null) obj.SetActive(!esMundoPodrido);
        }
        
        foreach (var obj in objetosPodridos)
        {
            if (obj != null) obj.SetActive(esMundoPodrido);
        }

        // Aplicar los valores de dissolve iniciales
        ActualizarMaterialesInstantaneo(renderersLindos, esMundoPodrido ? 1f : 0f);
        ActualizarMaterialesInstantaneo(renderersPodridos, esMundoPodrido ? 0f : 1f);

        // Aplicar la opacidad (fade) inicial de los decales
        AplicarFadeDecals(decalsLindos, esMundoPodrido ? 0f : 1f);
        AplicarFadeDecals(decalsPodridos, esMundoPodrido ? 1f : 0f);

        // Establecer pesos de post-procesado iniciales
        if (volumenLindo != null) volumenLindo.weight = esMundoPodrido ? 0f : 1f;
        if (volumenPodrido != null) volumenPodrido.weight = esMundoPodrido ? 1f : 0f;
        if (volumenTransicion != null) volumenTransicion.weight = 0f;
    }

    /// <summary>
    /// Alterna entre la realidad linda y la podrida de forma fluida.
    /// </summary>
    [ContextMenu("Alternar Realidad")]
    public void AlternarRealidad()
    {
        CambiarRealidad(!esMundoPodrido);
    }

    /// <summary>
    /// Cambia a la realidad especificada con un efecto de dissolve, post-procesado y sonido.
    /// </summary>
    public void CambiarRealidad(bool aPodrida)
    {
        if (esMundoPodrido == aPodrida && rutinaTransicion == null) return;
        
        esMundoPodrido = aPodrida;

        if (rutinaTransicion != null)
        {
            StopCoroutine(rutinaTransicion);
        }

        rutinaTransicion = StartCoroutine(RutinaTransicionRealidad(esMundoPodrido));
    }

    private IEnumerator RutinaTransicionRealidad(bool aPodrida)
    {
        // 1. Identificar colecciones
        List<GameObject> aActivar = aPodrida ? objetosPodridos : objetosLindos;
        List<Renderer> renderersAEntrar = aPodrida ? renderersPodridos : renderersLindos;
        List<Renderer> renderersASalir = aPodrida ? renderersLindos : renderersPodridos;

        List<DecalProjector> decalsAEntrar = aPodrida ? decalsPodridos : decalsLindos;
        List<DecalProjector> decalsASalir = aPodrida ? decalsLindos : decalsPodridos;

        // 2. Activar físicamente los objetos que van a entrar ANTES de que empiece el dissolve
        // para que puedan renderizarse a medida que se materializan.
        foreach (var obj in aActivar)
        {
            if (obj != null) obj.SetActive(true);
        }

        // Inicializar estados iniciales de la transición
        AplicarDissolve(renderersAEntrar, 1f); // Empiezan invisibles
        AplicarDissolve(renderersASalir, 0f);  // Empiezan visibles

        AplicarFadeDecals(decalsAEntrar, 0f);  // Empiezan invisibles (opacity = 0)
        AplicarFadeDecals(decalsASalir, 1f);   // Empiezan visibles (opacity = 1)

        // 3. Reproducir el efecto de sonido de la transición
        AudioClip sfxSeleccionado = aPodrida ? sfxMundoPodrido : sfxMundoLindo;
        if (audioSource != null && sfxSeleccionado != null)
        {
            audioSource.PlayOneShot(sfxSeleccionado, volumenSFX);
        }

        float tiempoTranscurrido = 0f;

        // 4. Interpolar dissolve, colores y distorsiones simultáneamente
        while (tiempoTranscurrido < tiempoTransicion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float t = Mathf.Clamp01(tiempoTranscurrido / tiempoTransicion);

            // Curva suave de transición
            float tSuave = Mathf.SmoothStep(0f, 1f, t);

            // Los que salen: se disuelven (0 -> 1) y se les baja la opacidad (1 -> 0)
            AplicarDissolve(renderersASalir, tSuave);
            AplicarFadeDecals(decalsASalir, 1f - tSuave);

            // Los que entran: se materializan (1 -> 0) y se les sube la opacidad (0 -> 1)
            AplicarDissolve(renderersAEntrar, 1f - tSuave);
            AplicarFadeDecals(decalsAEntrar, tSuave);

            // Interpolación de Volúmenes Lindo <-> Podrido
            if (aPodrida)
            {
                if (volumenLindo != null) volumenLindo.weight = 1f - tSuave;
                if (volumenPodrido != null) volumenPodrido.weight = tSuave;
            }
            else
            {
                if (volumenLindo != null) volumenLindo.weight = tSuave;
                if (volumenPodrido != null) volumenPodrido.weight = 1f - tSuave;
            }

            // Efecto "Pulse" del Volumen de Transición (Lens Distortion, Chromatic Aberration, Vignette)
            if (volumenTransicion != null)
            {
                float pesoSpike = curvaTransicion != null ? curvaTransicion.Evaluate(t) : Mathf.Sin(t * Mathf.PI);
                volumenTransicion.weight = pesoSpike;
            }

            yield return null;
        }

        // Garantizar valores exactos finales
        AplicarDissolve(renderersASalir, 1f);
        AplicarDissolve(renderersAEntrar, 0f);

        AplicarFadeDecals(decalsASalir, 0f);
        AplicarFadeDecals(decalsAEntrar, 1f);

        if (volumenLindo != null) volumenLindo.weight = aPodrida ? 0f : 1f;
        if (volumenPodrido != null) volumenPodrido.weight = aPodrida ? 1f : 0f;
        if (volumenTransicion != null) volumenTransicion.weight = 0f;

        // 5. Desactivar físicamente los objetos de la realidad vieja para liberar recursos
        List<GameObject> aDesactivar = aPodrida ? objetosLindos : objetosPodridos;
        foreach (var obj in aDesactivar)
        {
            if (obj != null) obj.SetActive(false);
        }

        rutinaTransicion = null;
    }

    private void AplicarDissolve(List<Renderer> renderers, float valorDissolve)
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            Renderer r = renderers[i];
            if (r != null)
            {
                // Obtenemos el property block del renderer
                r.GetPropertyBlock(propBlock);
                // Seteamos la propiedad del shader de forma segura sin clonar el material
                propBlock.SetFloat(idPropDissolve, valorDissolve);
                // Le devolvemos el property block modificado
                r.SetPropertyBlock(propBlock);
            }
        }
    }

    private void ActualizarMaterialesInstantaneo(List<Renderer> renderers, float valorDissolve)
    {
        AplicarDissolve(renderers, valorDissolve);
    }

    private void CachearDecals(List<GameObject> objetos, List<DecalProjector> listaDestino)
    {
        foreach (var obj in objetos)
        {
            if (obj != null)
            {
                listaDestino.AddRange(obj.GetComponentsInChildren<DecalProjector>(true));
            }
        }
    }

    private void AplicarFadeDecals(List<DecalProjector> decals, float fadeValor)
    {
        for (int i = 0; i < decals.Count; i++)
        {
            if (decals[i] != null)
            {
                decals[i].fadeFactor = fadeValor;
            }
        }
    }
}
