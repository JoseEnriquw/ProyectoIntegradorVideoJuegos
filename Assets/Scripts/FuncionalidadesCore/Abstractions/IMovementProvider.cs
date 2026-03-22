using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Abstrae el sistema de movimiento del jugador (CharacterController, Rigidbody, etc.).
    /// </summary>
    public interface IMovementProvider
    {
        /// <summary>Si el personaje está en el suelo.</summary>
        bool IsGrounded { get; }

        /// <summary>Altura del controlador.</summary>
        float Height { get; set; }

        /// <summary>Centro del controlador.</summary>
        Vector3 Center { get; set; }

        /// <summary>Si el controlador está habilitado.</summary>
        bool Enabled { get; set; }

        /// <summary>Aplicar movimiento al personaje.</summary>
        CollisionFlags Move(Vector3 motion);

        /// <summary>Obtener la posición de los pies.</summary>
        Vector3 GetFeetPosition();

        /// <summary>Obtener la posición del centro.</summary>
        Vector3 GetCenterPosition();
    }
}
