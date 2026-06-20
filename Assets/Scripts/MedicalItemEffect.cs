using UnityEngine;

public class MedicalItemEffect : MonoBehaviour
{
    [Header("Medical Settings")]
    [Tooltip("Si está marcado, apaga el sistema por completo. Si no, solo resetea los síntomas pero volverán con el tiempo.")]
    public bool isPermanentCure = false;

    [Header("Audio Settings")]
    [Tooltip("El sonido que se reproducirá al usar este objeto médico.")]
    public AudioClip cureSound;
    [Tooltip("El AudioSource que reproducirá el sonido. Si está vacío, se buscará o agregará uno automáticamente.")]
    public AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0f; // Sonido 2D por defecto
                audioSource.playOnAwake = false;
            }
        }
    }

    /// <summary>
    /// Esta función debe ser llamada desde el evento "On Use" del componente InventoryUseEvents de UHFPS.
    /// bonusWaitTime: tiempo extra de espera antes del próximo síntoma.
    /// </summary>
    public void ApplyCure(float bonusWaitTime = 0f)
    {
        StartCoroutine(CureRoutine(bonusWaitTime));
    }

    private System.Collections.IEnumerator CureRoutine(float bonusWaitTime)
    {
        // 1. Reproducir el sonido nuevo de esta cura
        PlayCureSound();

        // 2. Buscar el sistema de síntomas del jugador
        PlayerSymptom symptomSystem = PlayerSymptom.Instance;
        if (symptomSystem == null)
        {
            symptomSystem = FindFirstObjectByType<PlayerSymptom>();
        }

        // 3. Aliviar síntomas de inmediato, pero sin disparar el sonido global aún (playCureSound: false)
        if (symptomSystem != null)
        {
            if (isPermanentCure)
            {
                symptomSystem.CureSymptomsFully();
            }
            else
            {
                symptomSystem.RelieveSymptomsTemporarily(bonusWaitTime, false);
            }
        }

        // 4. Esperar el tiempo exacto que dura el sonido nuevo (si está asignado)
        float waitTime = cureSound != null ? cureSound.length : 0f;
        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }

        // 5. Reproducir el sonido de cura global del jugador (como los suspiros, latidos, etc.)
        if (symptomSystem != null)
        {
            symptomSystem.PlayGlobalCureSounds();
        }
    }

    /// <summary>
    /// Reproduce el sonido de curación asignado.
    /// </summary>
    public void PlayCureSound()
    {
        if (audioSource != null && cureSound != null)
        {
            audioSource.PlayOneShot(cureSound);
            Debug.Log($"[MedicalItemEffect] Reproduciendo sonido de curación: {cureSound.name}");
        }
    }
}
