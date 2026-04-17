using UnityEngine;

public class MedicalItemEffect : MonoBehaviour
{
    [Header("Medical Settings")]
    [Tooltip("Si está marcado, apaga el sistema por completo. Si no, solo resetea los síntomas pero volverán con el tiempo.")]
    public bool isPermanentCure = false;

    /// <summary>
    /// Esta función debe ser llamada desde el evento "On Use" del componente InventoryUseEvents de UHFPS.
    /// </summary>
    public void ApplyCure()
    {
        // 1. Primero intentamos usar la forma rápida: nuestro Singleton
        PlayerSymptom symptomSystem = PlayerSymptom.Instance;

        // 2. Fallback súper seguro por si el Singleton no se instanció a tiempo
        if (symptomSystem == null)
        {
            symptomSystem = FindFirstObjectByType<PlayerSymptom>();
        }
        // 3. Aplicamos el efecto configurado
        if (symptomSystem != null)
        {
            if (isPermanentCure)
            {
                symptomSystem.CureSymptomsFully();
                Debug.Log($"[{gameObject.name}] Aplicando cura PERMANENTE.");
            }
            else
            {
                symptomSystem.RelieveSymptomsTemporarily();
                Debug.Log($"[{gameObject.name}] Aplicando alivio TEMPORAL.");
            }
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Intentó curar al Player, pero PlayerSymptom no existe en la escena activa.");
        }
    }
}
