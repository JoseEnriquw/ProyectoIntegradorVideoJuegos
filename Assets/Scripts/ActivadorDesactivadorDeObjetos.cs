using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using UHFPS.Runtime;

/// <summary>
/// Un script premium, limpio y sumamente útil para activar un objeto (o conjunto de objetos)
/// y desactivar otro (o conjunto de objetos) simultáneamente.
/// Soporta retrasos (delays), disparadores por trigger, interacción del jugador (UHFPS) y llamados manuales.
/// </summary>
public class ActivadorDesactivadorDeObjetos : MonoBehaviour, IInteractStart, ISaveable
{
    public enum ModoEjecucion { Manual, AlIniciar, AlHabilitar, AlEntrarTrigger, AlSalirTrigger, AlInteractuar }

    [Header("Objetos a Activar")]
    [Tooltip("El objeto principal que se va a ACTIVAR (SetActive(true)).")]
    public GameObject objetoAActivar;

    [Tooltip("Lista adicional de objetos opcionales que también se van a ACTIVAR.")]
    public List<GameObject> otrosAActivar = new List<GameObject>();

    [Header("Objetos a Desactivar")]
    [Tooltip("El objeto principal que se va a DESACTIVAR (SetActive(false)).")]
    public GameObject objetoADesactivar;

    [Tooltip("Lista adicional de objetos opcionales que también se van a DESACTIVAR.")]
    public List<GameObject> otrosADesactivar = new List<GameObject>();

    [Header("Configuración de Disparo")]
    [Tooltip("Cómo quieres que se desencadene el cambio.")]
    public ModoEjecucion dispararCon = ModoEjecucion.Manual;

    [Tooltip("Tiempo en segundos que se esperará antes de realizar la acción (0 = inmediato).")]
    [Min(0f)]
    public float delayAccion = 0f;

    [Tooltip("¿Solo se debe poder ejecutar una única vez en toda la partida?")]
    public bool unaSolaVez = false;

    [Header("Configuración de Trigger (Colisiones)")]
    [Tooltip("El tag del objeto que debe activar el trigger (generalmente 'Player').")]
    public string tagTrigger = "Player";

    [Header("Eventos de Unity")]
    public UnityEvent AlEjecutarAccion;
    public UnityEvent AlRevertirAccion;

    private bool yaSeEjecuto = false;
    private Coroutine rutinaEjecucion;

    private void Start()
    {
        if (dispararCon == ModoEjecucion.AlIniciar)
        {
            EjecutarAccion();
        }
    }

    private void OnEnable()
    {
        if (dispararCon == ModoEjecucion.AlHabilitar)
        {
            EjecutarAccion();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (dispararCon != ModoEjecucion.AlEntrarTrigger) return;
        if (unaSolaVez && yaSeEjecuto) return;

        if (string.IsNullOrEmpty(tagTrigger) || other.CompareTag(tagTrigger))
        {
            EjecutarAccion();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (dispararCon != ModoEjecucion.AlSalirTrigger) return;
        if (unaSolaVez && yaSeEjecuto) return;

        if (string.IsNullOrEmpty(tagTrigger) || other.CompareTag(tagTrigger))
        {
            EjecutarAccion();
        }
    }

    /// <summary>
    /// Implementación de la interfaz IInteractStart de UHFPS.
    /// Permite al jugador activar este cambio interactuando con el objeto (ej. haciendo click).
    /// </summary>
    public void InteractStart()
    {
        if (dispararCon != ModoEjecucion.AlInteractuar) return;
        if (unaSolaVez && yaSeEjecuto) return;

        EjecutarAccion();
    }

    /// <summary>
    /// Método público para ejecutar la acción de activar y desactivar los objetos.
    /// </summary>
    public void EjecutarAccion()
    {
        if (unaSolaVez && yaSeEjecuto) return;

        yaSeEjecuto = true;

        if (delayAccion > 0f)
        {
            if (rutinaEjecucion != null) StopCoroutine(rutinaEjecucion);
            rutinaEjecucion = StartCoroutine(RutinaEjecucion(true));
        }
        else
        {
            AplicarCambioDeEstado(true);
        }
    }

    /// <summary>
    /// Método público para revertir la acción (desactivar lo que se activó, y activar lo que se desactivó).
    /// </summary>
    public void RevertirAccion()
    {
        if (delayAccion > 0f)
        {
            if (rutinaEjecucion != null) StopCoroutine(rutinaEjecucion);
            rutinaEjecucion = StartCoroutine(RutinaEjecucion(false));
        }
        else
        {
            AplicarCambioDeEstado(false);
        }
    }

    private IEnumerator RutinaEjecucion(bool activar)
    {
        yield return new WaitForSeconds(delayAccion);
        AplicarCambioDeEstado(activar);
        rutinaEjecucion = null;
    }

    private void AplicarCambioDeEstado(bool activar)
    {
        // 1. Manejo del objeto principal a activar
        if (objetoAActivar != null)
        {
            objetoAActivar.SetActive(activar);
        }

        // Manejo de la lista opcional a activar
        foreach (var obj in otrosAActivar)
        {
            if (obj != null) obj.SetActive(activar);
        }

        // 2. Manejo del objeto principal a desactivar
        if (objetoADesactivar != null)
        {
            objetoADesactivar.SetActive(!activar);
        }

        // Manejo de la lista opcional a desactivar
        foreach (var obj in otrosADesactivar)
        {
            if (obj != null) obj.SetActive(!activar);
        }

        if (activar)
        {
            AlEjecutarAccion?.Invoke();
            Debug.Log($"[ActivadorDesactivador] Ejecutado en '{gameObject.name}': Objetos activados/desactivados.", this);
        }
        else
        {
            AlRevertirAccion?.Invoke();
            Debug.Log($"[ActivadorDesactivador] Revertido en '{gameObject.name}': Objetos devueltos a su estado original.", this);
        }
    }

    private void OnDrawGizmos()
    {
        if (dispararCon == ModoEjecucion.AlEntrarTrigger || dispararCon == ModoEjecucion.AlSalirTrigger)
        {
            Collider col = GetComponent<Collider>();
            if (col != null && col.isTrigger)
            {
                Gizmos.color = new Color(0.1f, 0.8f, 0.4f, 0.35f); // Verde translúcido
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(Vector3.zero, col.bounds.size / transform.lossyScale.x);
            }
        }
    }

    public StorableCollection OnSave()
    {
        StorableCollection storableCollection = new();
        storableCollection.Add("yaSeEjecuto", yaSeEjecuto);
        return storableCollection;
    }

    public void OnLoad(JToken data)
    {
        if (data["yaSeEjecuto"] != null)
        {
            yaSeEjecuto = (bool)data["yaSeEjecuto"];
        }
    }
}
