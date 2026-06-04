using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Script personalizado y robusto para deslizar cualquier objeto en su espacio local
/// hacia una posición configurada de manera suave y limpia.
/// </summary>
public class DesplazadorDeObjeto : MonoBehaviour
{
    [Header("Configuración del Objeto")]
    [Tooltip("El objeto que se va a mover. Si se deja vacío, se moverá el objeto al que está unido este script.")]
    public Transform objetoAMover;

    [Header("Posición de Destino (Local)")]
    [Tooltip("La posición local final a la que se desplazará el objeto (X, Y, Z).")]
    public Vector3 posicionObjetivoLocal;

    [Header("Configuración del Movimiento")]
    [Tooltip("Duración del desplazamiento en segundos.")]
    [Min(0.01f)]
    public float duracion = 2f;

    [Tooltip("Curva que define la velocidad y suavizado del movimiento.")]
    public AnimationCurve curvaMovimiento = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Sonido (Opcional)")]
    [Tooltip("AudioSource que reproducirá el sonido. Si se deja vacío, intentará usar el del propio objeto.")]
    public AudioSource audioSource;
    [Tooltip("Sonido que se reproducirá al iniciar el desplazamiento.")]
    public AudioClip sonidoAlDesplazar;

    [Header("Eventos")]
    public UnityEvent AlIniciarMovimiento;
    public UnityEvent AlFinalizarMovimiento;

    private Vector3 posicionInicialLocal;
    private bool estaDesplazado = false;
    private Coroutine rutinaMovimiento;

    private void Start()
    {
        if (objetoAMover == null)
            objetoAMover = transform;

        // Guardamos la posición inicial local en la que empieza la escena
        posicionInicialLocal = objetoAMover.localPosition;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Inicia el desplazamiento hacia la posición objetivo local configurada.
    /// </summary>
    public void IniciarDesplazamiento()
    {
        MoverA(posicionObjetivoLocal);
        estaDesplazado = true;
    }

    /// <summary>
    /// Mueve el objeto de vuelta a su posición inicial original.
    /// </summary>
    public void RevertirDesplazamiento()
    {
        MoverA(posicionInicialLocal);
        estaDesplazado = false;
    }

    /// <summary>
    /// Alterna entre la posición inicial y la de destino.
    /// </summary>
    public void AlternarDesplazamiento()
    {
        if (estaDesplazado)
            RevertirDesplazamiento();
        else
            IniciarDesplazamiento();
    }

    private void MoverA(Vector3 destino)
    {
        if (rutinaMovimiento != null)
            StopCoroutine(rutinaMovimiento);

        if (gameObject.activeInHierarchy)
        {
            rutinaMovimiento = StartCoroutine(RutinaMovimiento(destino));
        }
        else
        {
            // Salvaguarda por si el objeto está desactivado, aplicando el cambio directo
            if (objetoAMover != null)
                objetoAMover.localPosition = destino;
        }
    }

    private IEnumerator RutinaMovimiento(Vector3 destino)
    {
        AlIniciarMovimiento?.Invoke();

        if (audioSource != null && sonidoAlDesplazar != null)
        {
            audioSource.PlayOneShot(sonidoAlDesplazar);
        }

        Vector3 origen = objetoAMover.localPosition;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < duracion)
        {
            tiempoTranscurrido += Time.deltaTime;
            float tNormalized = Mathf.Clamp01(tiempoTranscurrido / duracion);
            float tCurva = curvaMovimiento.Evaluate(tNormalized);

            objetoAMover.localPosition = Vector3.Lerp(origen, destino, tCurva);
            yield return null;
        }

        objetoAMover.localPosition = destino;
        rutinaMovimiento = null;

        AlFinalizarMovimiento?.Invoke();
    }
}
