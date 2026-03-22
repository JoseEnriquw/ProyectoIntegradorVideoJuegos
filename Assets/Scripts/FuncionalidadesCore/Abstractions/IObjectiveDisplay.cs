namespace FuncionalidadesCore
{
    /// <summary>
    /// Interfaz para la presentación visual de objetivos.
    /// Implementa esta interfaz en tu UI concreta para mostrar objetivos.
    /// </summary>
    public interface IObjectiveDisplay
    {
        /// <summary>Se llama cuando se agrega un nuevo objetivo.</summary>
        void OnObjectiveAdded(string objectiveKey, string title);

        /// <summary>Se llama cuando un objetivo se completa.</summary>
        void OnObjectiveCompleted(string objectiveKey);

        /// <summary>Se llama cuando un objetivo se descarta.</summary>
        void OnObjectiveDiscarded(string objectiveKey);

        /// <summary>Se llama cuando se agrega un sub-objetivo.</summary>
        void OnSubObjectiveAdded(string objectiveKey, string subKey, string text);

        /// <summary>Se llama cuando un sub-objetivo se completa.</summary>
        void OnSubObjectiveCompleted(string objectiveKey, string subKey);

        /// <summary>Se llama cuando cambia el conteo de un sub-objetivo.</summary>
        void OnSubObjectiveCountChanged(string objectiveKey, string subKey, ushort currentCount, ushort requiredCount);

        /// <summary>Se llama para mostrar una notificación de objetivo.</summary>
        void ShowNotification(string title, bool isCompleted);
    }
}
