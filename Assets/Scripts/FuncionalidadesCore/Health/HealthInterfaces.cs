using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Interfaz para entidades que pueden recibir daño.
    /// </summary>
    public interface IDamagable
    {
        /// <summary>Aplicar daño a la entidad.</summary>
        void OnApplyDamage(int damage, Transform sender = null);

        /// <summary>Aplicar daño máximo (matar instantáneamente).</summary>
        void ApplyDamageMax(Transform sender = null);
    }

    /// <summary>
    /// Interfaz para entidades que pueden ser curadas.
    /// </summary>
    public interface IHealable
    {
        /// <summary>Aplicar curación a la entidad.</summary>
        void OnApplyHeal(int healAmount);

        /// <summary>Curar al máximo.</summary>
        void ApplyHealMax();
    }

    /// <summary>
    /// Interfaz completa para entidades con sistema de salud (daño + curación).
    /// </summary>
    public interface IHealthEntity : IDamagable, IHealable
    {
        /// <summary>Salud actual de la entidad.</summary>
        int EntityHealth { get; set; }

        /// <summary>Salud máxima de la entidad.</summary>
        int MaxEntityHealth { get; set; }
    }

    /// <summary>
    /// Interfaz para entidades que pueden ser destruidas (objetos rompibles).
    /// </summary>
    public interface IBreakableEntity : IDamagable
    {
        /// <summary>Salud actual de la entidad.</summary>
        int EntityHealth { get; set; }
    }
}
