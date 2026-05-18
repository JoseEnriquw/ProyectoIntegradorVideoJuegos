using UnityEngine;

public class ConsumableActions : MonoBehaviour
{
    [Header("Salt Settings")]
    [Tooltip("Tiempo en segundos que se añade al consumir la Sal.")]
    public float SaltTimeBonus = 60f;

    /// <summary>
    /// Método para usar desde el InventoryUseEvents cuando se consume la Sal.
    /// Si se pasa un valor > 0, se usa ese valor. Si no, se usa SaltTimeBonus.
    /// </summary>
    public void ConsumeSalt(float customBonus = 0f)
    {
        float bonus = customBonus > 0 ? customBonus : SaltTimeBonus;
        if (SurvivalTimer.Instance != null)
        {
            SurvivalTimer.Instance.AddTime(bonus);
            Debug.Log($"[ConsumableActions] Sal consumida. Se añadieron {bonus} segundos al reloj.");
        }
        else
        {
            Debug.LogWarning("[ConsumableActions] No se encontró el SurvivalTimer en la escena.");
        }
    }

    /// <summary>
    /// Método genérico por si quieres añadir un tiempo específico desde un UnityEvent.
    /// </summary>
    public void AddSurvivalTime(float seconds)
    {
        if (SurvivalTimer.Instance != null)
        {
            SurvivalTimer.Instance.AddTime(seconds);
        }
    }
}
