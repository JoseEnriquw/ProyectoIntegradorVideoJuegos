using UnityEngine;

/// <summary>
/// Hace que un objeto flote de manera sinusoidal (suba y baje) en su eje Y,
/// con opción de rotarlo continuamente.
/// </summary>
public class ObjetoFlotante : MonoBehaviour
{
    [Header("Efecto de Flotación")]
    [Tooltip("El objeto que va a flotar. Si se deja vacío, se aplicará al transform de este script.")]
    public Transform objetoAFlotar;

    [Tooltip("La altura o distancia máxima que subirá y bajará el objeto.")]
    public float amplitud = 0.2f;

    [Tooltip("La velocidad o frecuencia del movimiento de flotación.")]
    public float velocidadFlotacion = 2.0f;

    [Tooltip("Si es verdadero, añade una fase de inicio aleatoria para que varios objetos no floten sincronizados.")]
    public bool desfasarInicio = true;

    [Tooltip("Si es verdadero, flotará y rotará en su espacio local (recomendado para objetos hijos).")]
    public bool usarEspacioLocal = true;

    [Header("Efecto de Rotación (Opcional)")]
    [Tooltip("Si es verdadero, el objeto rotará continuamente.")]
    public bool rotarObjeto = false;

    [Tooltip("Velocidad de rotación expresada en grados por segundo para cada eje (X, Y, Z).")]
    public Vector3 velocidadRotacion = new Vector3(0f, 30f, 0f);

    private Vector3 posicionInicial;
    private float desfaseTemporal;

    private void Start()
    {
        if (objetoAFlotar == null)
        {
            objetoAFlotar = transform;
        }

        // Guardamos la posición inicial basándonos en si usamos espacio local o global
        posicionInicial = usarEspacioLocal ? objetoAFlotar.localPosition : objetoAFlotar.position;

        // Generamos un desfase de tiempo aleatorio para evitar la sincronía perfecta entre objetos
        if (desfasarInicio)
        {
            desfaseTemporal = Random.Range(0f, Mathf.PI * 2f);
        }
        else
        {
            desfaseTemporal = 0f;
        }
    }

    private void Update()
    {
        if (objetoAFlotar == null) return;

        // Calculamos el desfase sinusoidal basado en el tiempo
        float desfaseY = Mathf.Sin(Time.time * velocidadFlotacion + desfaseTemporal) * amplitud;

        // Aplicamos la posición flotante en el eje Y manteniendo X y Z originales
        if (usarEspacioLocal)
        {
            objetoAFlotar.localPosition = new Vector3(posicionInicial.x, posicionInicial.y + desfaseY, posicionInicial.z);
        }
        else
        {
            objetoAFlotar.position = new Vector3(posicionInicial.x, posicionInicial.y + desfaseY, posicionInicial.z);
        }

        // Aplicamos la rotación continua si está habilitada
        if (rotarObjeto)
        {
            objetoAFlotar.Rotate(velocidadRotacion * Time.deltaTime, usarEspacioLocal ? Space.Self : Space.World);
        }
    }
}
