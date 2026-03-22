using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Sistema de objetivos desacoplado de UI.
    /// Gestiona objetivos y sub-objetivos con sus estados.
    /// La presentación visual se delega a IObjectiveDisplay.
    /// </summary>
    public class ObjectiveManagerCore : MonoBehaviour
    {
        [Header("Objectives Configuration")]
        [SerializeField] private Objective[] availableObjectives;

        private IObjectiveDisplay display;
        private readonly Dictionary<string, ObjectiveState> activeObjectives = new();
        private readonly Dictionary<string, ObjectiveState> completedObjectives = new();

        // --- Eventos ---
        public event Action<string> OnObjectiveAdded;
        public event Action<string> OnObjectiveCompleted;
        public event Action<string> OnObjectiveDiscarded;
        public event Action<string, string> OnSubObjectiveCompleted;

        /// <summary>Objetivos activos actualmente.</summary>
        public IReadOnlyDictionary<string, ObjectiveState> ActiveObjectives => activeObjectives;

        /// <summary>Objetivos completados.</summary>
        public IReadOnlyDictionary<string, ObjectiveState> CompletedObjectives => completedObjectives;

        /// <summary>Inyectar la implementación de display de UI.</summary>
        public void SetDisplay(IObjectiveDisplay objectiveDisplay)
        {
            display = objectiveDisplay;
        }

        /// <summary>Agregar un nuevo objetivo al sistema.</summary>
        public bool AddObjective(string objectiveKey)
        {
            if (activeObjectives.ContainsKey(objectiveKey))
            {
                Debug.LogWarning($"[ObjectiveManager] Objective '{objectiveKey}' is already active.");
                return false;
            }

            var objective = availableObjectives.FirstOrDefault(o => o.objectiveKey == objectiveKey);
            if (string.IsNullOrEmpty(objective.objectiveKey))
            {
                Debug.LogError($"[ObjectiveManager] Objective '{objectiveKey}' not found in available objectives.");
                return false;
            }

            var state = new ObjectiveState
            {
                Key = objectiveKey,
                Title = objective.objectiveTitle,
                Status = ObjectiveStatus.Active,
                SubObjectives = objective.subObjectives?.Select(so => new SubObjectiveState
                {
                    Key = so.subObjectiveKey,
                    Text = so.objectiveText,
                    CurrentCount = 0,
                    RequiredCount = so.completeCount,
                    IsCompleted = false
                }).ToList() ?? new()
            };

            activeObjectives.Add(objectiveKey, state);
            display?.OnObjectiveAdded(objectiveKey, state.Title);
            display?.ShowNotification(state.Title, false);
            OnObjectiveAdded?.Invoke(objectiveKey);

            // Agregar sub-objetivos al display
            foreach (var sub in state.SubObjectives)
            {
                display?.OnSubObjectiveAdded(objectiveKey, sub.Key, sub.Text);
            }

            return true;
        }

        /// <summary>Completar un sub-objetivo (incrementa su conteo).</summary>
        public bool CompleteSubObjective(string objectiveKey, string subKey, ushort count = 1)
        {
            if (!activeObjectives.TryGetValue(objectiveKey, out var state))
            {
                Debug.LogWarning($"[ObjectiveManager] Objective '{objectiveKey}' is not active.");
                return false;
            }

            var sub = state.SubObjectives.FirstOrDefault(s => s.Key == subKey);
            if (sub == null)
            {
                Debug.LogWarning($"[ObjectiveManager] SubObjective '{subKey}' not found.");
                return false;
            }

            if (sub.IsCompleted) return false;

            sub.CurrentCount += count;
            display?.OnSubObjectiveCountChanged(objectiveKey, subKey, sub.CurrentCount, sub.RequiredCount);

            if (sub.CurrentCount >= sub.RequiredCount)
            {
                sub.IsCompleted = true;
                display?.OnSubObjectiveCompleted(objectiveKey, subKey);
                OnSubObjectiveCompleted?.Invoke(objectiveKey, subKey);

                // Verificar si todos los sub-objetivos están completos
                if (state.SubObjectives.All(s => s.IsCompleted))
                {
                    CompleteObjective(objectiveKey);
                }
            }

            return true;
        }

        /// <summary>Completar un objetivo manualmente.</summary>
        public void CompleteObjective(string objectiveKey)
        {
            if (!activeObjectives.TryGetValue(objectiveKey, out var state)) return;

            state.Status = ObjectiveStatus.Completed;
            activeObjectives.Remove(objectiveKey);
            completedObjectives[objectiveKey] = state;

            display?.OnObjectiveCompleted(objectiveKey);
            display?.ShowNotification(state.Title, true);
            OnObjectiveCompleted?.Invoke(objectiveKey);
        }

        /// <summary>Descartar un objetivo.</summary>
        public void DiscardObjective(string objectiveKey)
        {
            if (!activeObjectives.TryGetValue(objectiveKey, out var state)) return;

            state.Status = ObjectiveStatus.Discarded;
            activeObjectives.Remove(objectiveKey);

            display?.OnObjectiveDiscarded(objectiveKey);
            OnObjectiveDiscarded?.Invoke(objectiveKey);
        }

        /// <summary>Verificar si un objetivo está activo.</summary>
        public bool IsObjectiveActive(string objectiveKey) => activeObjectives.ContainsKey(objectiveKey);

        /// <summary>Verificar si un objetivo está completado.</summary>
        public bool IsObjectiveCompleted(string objectiveKey) => completedObjectives.ContainsKey(objectiveKey);

        // --- Save/Load ---
        public StorableCollection OnSave()
        {
            var data = new StorableCollection();
            data.Add("activeObjectives", activeObjectives.Values.ToList());
            data.Add("completedObjectives", completedObjectives.Values.ToList());
            return data;
        }
    }
}
