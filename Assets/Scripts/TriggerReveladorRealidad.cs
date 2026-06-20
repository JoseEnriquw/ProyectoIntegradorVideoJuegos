using UnityEngine;

/// <summary>
/// Script premium que activa/desactiva objetos cuando el jugador entra o sale de un trigger,
/// variando dinámicamente según la realidad activa (Linda o Podrida/Fea).
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerReveladorRealidad : MonoBehaviour
{
    [Header("Objetos a revelar en la Realidad Linda")]
    [Tooltip("Objetos que aparecerán solo si estás dentro del trigger y la realidad activa es Linda.")]
    public GameObject[] objetosRealidadLinda;

    [Header("Objetos a revelar en la Realidad Podrida / Fea")]
    [Tooltip("Objetos que aparecerán solo si estás dentro del trigger y la realidad activa es Podrida/Fea.")]
    public GameObject[] objetosRealidadPodrida;

    [Header("Configuración del Trigger")]
    [Tooltip("Tag del objeto que activa el trigger (generalmente 'Player').")]
    public string tagActivador = "Player";

    private bool jugadorAdentro = false;
    private bool ultimaRealidadRegistrada = false;

    private void Start()
    {
        // Asegurar que el collider sea un trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        // Al inicio del juego, nos aseguramos de que todos los objetos revelables estén apagados
        DesactivarTodosLosObjetos();
    }

    private void Update()
    {
        if (jugadorAdentro)
        {
            // Obtener el estado actual del gestor de realidades
            bool esMundoPodrido = false;
            if (ControladorCambioRealidad.Instancia != null)
            {
                esMundoPodrido = ControladorCambioRealidad.Instancia.EsMundoPodrido;
            }

            // Si la realidad cambia mientras el jugador está adentro del trigger, actualizamos los objetos revelados
            if (esMundoPodrido != ultimaRealidadRegistrada)
            {
                ActualizarObjetosRevelados(esMundoPodrido);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagActivador))
        {
            jugadorAdentro = true;

            // Consultar la realidad actual al entrar
            bool esMundoPodrido = false;
            if (ControladorCambioRealidad.Instancia != null)
            {
                esMundoPodrido = ControladorCambioRealidad.Instancia.EsMundoPodrido;
            }

            ActualizarObjetosRevelados(esMundoPodrido);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagActivador))
        {
            jugadorAdentro = false;
            DesactivarTodosLosObjetos();
        }
    }

    /// <summary>
    /// Activa la lista de objetos de la realidad correspondiente y desactiva la otra.
    /// </summary>
    private void ActualizarObjetosRevelados(bool esMundoPodrido)
    {
        ultimaRealidadRegistrada = esMundoPodrido;

        // Si es mundo podrido, activamos los de la realidad podrida y desactivamos los de la linda
        foreach (var obj in objetosRealidadLinda)
        {
            if (obj != null) obj.SetActive(!esMundoPodrido);
        }

        foreach (var obj in objetosRealidadPodrida)
        {
            if (obj != null) obj.SetActive(esMundoPodrido);
        }
    }

    /// <summary>
    /// Apaga absolutamente todos los objetos revelables cuando el jugador sale del trigger.
    /// </summary>
    private void DesactivarTodosLosObjetos()
    {
        foreach (var obj in objetosRealidadLinda)
        {
            if (obj != null) obj.SetActive(false);
        }

        foreach (var obj in objetosRealidadPodrida)
        {
            if (obj != null) obj.SetActive(false);
        }
    }
}
