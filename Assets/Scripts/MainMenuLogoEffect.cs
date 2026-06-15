using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuLogoEffect : MonoBehaviour
{
    private Image logoImage;
    private Vector3 baseScale;
    
    [Header("Breathing Settings")]
    public float breatheSpeed = 1.5f;
    public float breatheAmount = 0.025f;
    
    [Header("Glitch Settings")]
    public float minGlitchInterval = 4f;
    public float maxGlitchInterval = 12f;
    
    private float baseAlpha = 1f;
    
    void Start()
    {
        logoImage = GetComponent<Image>();
        baseScale = transform.localScale;
        if (logoImage != null)
        {
            baseAlpha = logoImage.color.a;
        }
        
        StartCoroutine(GlitchLoop());
    }
    

    
    IEnumerator GlitchLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minGlitchInterval, maxGlitchInterval));
            
            if (logoImage == null) continue;
            
            // Perform rapid glitchy flickering
            int flickers = Random.Range(2, 6);
            Color originalColor = logoImage.color;
            
            for (int i = 0; i < flickers; i++)
            {
                // Set to low opacity
                logoImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, Random.Range(0.15f, 0.5f) * baseAlpha);
                yield return new WaitForSeconds(Random.Range(0.03f, 0.1f));
                
                // Restore
                logoImage.color = originalColor;
                yield return new WaitForSeconds(Random.Range(0.02f, 0.07f));
            }
        }
    }
}
