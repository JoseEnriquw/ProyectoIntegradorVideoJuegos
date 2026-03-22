using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Provee información sobre el objetivo/target de un NPC (típicamente el jugador).
    /// Abstrae PlayerPresenceManager para la IA.
    /// </summary>
    public interface ITargetProvider
    {
        /// <summary>Transform del objetivo actual.</summary>
        Transform GetTarget();

        /// <summary>Si el objetivo está muerto.</summary>
        bool IsTargetDead { get; }

        /// <summary>Posición del objetivo.</summary>
        Vector3 TargetPosition { get; }

        /// <summary>Calcula la distancia desde un punto al target.</summary>
        float DistanceToTarget(Vector3 fromPosition);
    }
}
