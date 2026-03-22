using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Abstrae el agente de navegación (NavMeshAgent) para permitir distintas implementaciones.
    /// </summary>
    public interface INavigationAgent
    {
        /// <summary>Destino actual del agente.</summary>
        Vector3 Destination { get; }

        /// <summary>Velocidad actual del agente.</summary>
        float Speed { get; set; }

        /// <summary>Si el agente tiene un camino calculado.</summary>
        bool HasPath { get; }

        /// <summary>Si el agente actualiza su rotación automáticamente.</summary>
        bool UpdateRotation { get; set; }

        /// <summary>Punto de steering actual del agente.</summary>
        Vector3 SteeringTarget { get; }

        /// <summary>Distancia restante hasta el destino.</summary>
        float RemainingDistance { get; }

        /// <summary>Si el agente está activo y habilitado.</summary>
        bool IsActiveAndEnabled { get; }

        /// <summary>Establece un nuevo destino para el agente.</summary>
        void SetDestination(Vector3 target);

        /// <summary>Detiene el movimiento del agente.</summary>
        void Stop();

        /// <summary>Reanuda el movimiento del agente.</summary>
        void Resume();
    }
}
