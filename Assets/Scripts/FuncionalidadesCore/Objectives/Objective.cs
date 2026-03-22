using System;
using System.Collections.Generic;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Datos de un objetivo del juego.
    /// </summary>
    [Serializable]
    public struct Objective
    {
        public string objectiveKey;
        public string objectiveTitle;
        public SubObjective[] subObjectives;
    }

    /// <summary>
    /// Datos de un sub-objetivo.
    /// </summary>
    [Serializable]
    public struct SubObjective
    {
        public string subObjectiveKey;
        public string objectiveText;
        public ushort completeCount;
    }

    /// <summary>
    /// Estado de un objetivo activo.
    /// </summary>
    public enum ObjectiveStatus
    {
        Inactive,
        Active,
        Completed,
        Discarded
    }

    /// <summary>
    /// Estado de un sub-objetivo activo.
    /// </summary>
    public class SubObjectiveState
    {
        public string Key;
        public string Text;
        public ushort CurrentCount;
        public ushort RequiredCount;
        public bool IsCompleted;
    }

    /// <summary>
    /// Estado de un objetivo activo con todos sus sub-objetivos.
    /// </summary>
    public class ObjectiveState
    {
        public string Key;
        public string Title;
        public ObjectiveStatus Status;
        public List<SubObjectiveState> SubObjectives = new();
    }
}
