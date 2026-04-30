using UnityEngine;

[RequireComponent(typeof(Light))]
public class LuzDeVela : MonoBehaviour
{
    [Header("Configuración del Parpadeo")]
    [Tooltip("Intensidad mínima que puede alcanzar la luz.")]
    public float minIntensity = 0.5f;
    
    [Tooltip("Intensidad máxima que puede alcanzar la luz.")]
    public float maxIntensity = 2.0f;
    
    [Tooltip("Qué tan rápido parpadea la luz de la vela.")]
    public float flickerSpeed = 2.0f;
    
    private Light targetLight;
    private float randomOffset;

    void Start()
    {
        targetLight = GetComponent<Light>();
        
        // Creamos un offset aleatorio para que si pones este script en varias velas diferentes,
        // no parpadeen todas al mismo tiempo de manera sincronizada (se verá más natural).
        randomOffset = Random.Range(0.0f, 1000.0f);
    }

    void Update()
    {
        // Utilizamos Ruido de Perlin (Perlin Noise) que nos da números aleatorios pero de forma "suave"
        // ideal para simular elementos orgánicos como el fuego o las nubes.
        float noise = Mathf.PerlinNoise(randomOffset, Time.time * flickerSpeed);
        
        // Ajustamos la intensidad de la luz basada en el ruido calculado
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
    }
}
