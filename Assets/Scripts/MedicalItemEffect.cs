using UnityEngine;

public class MedicalItemEffect : MonoBehaviour
{
    [Header("Medical Settings")]
    [Tooltip("Si está marcado, apaga el sistema por completo. Si no, solo resetea los síntomas pero volverán con el tiempo.")]
    public bool isPermanentCure = false;
    /// <summary>
    /// Esta función debe ser llamada desde el evento "On Use" del componente InventoryUseEvents de UHFPS.
    /// bonusWaitTime: tiempo extra de espera antes del próximo síntoma.
    /// </summary>
    public void ApplyCure(float bonusWaitTime = 0f)
    {
        PlayerSymptom symptomSystem = PlayerSymptom.Instance;

        if (symptomSystem == null)
        {
            symptomSystem = FindFirstObjectByType<PlayerSymptom>();
        }

        if (symptomSystem != null)
        {
            if (isPermanentCure)
            {
                symptomSystem.CureSymptomsFully();
            }
            else
            {
                symptomSystem.RelieveSymptomsTemporarily(bonusWaitTime);
            }
        }
    }
}
