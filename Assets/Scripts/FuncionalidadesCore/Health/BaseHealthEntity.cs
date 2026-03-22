using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Clase base abstracta para entidades con sistema de salud.
    /// Maneja la lógica pura de daño, curación, y muerte.
    /// No contiene referencias a UI - ideal para heredar y agregar efectos visuales.
    /// </summary>
    public abstract class BaseHealthEntity : MonoBehaviour, IHealthEntity
    {
        [Header("Health Settings")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int startHealth = 100;

        private int currentHealth;
        private bool isDead;

        /// <summary>Salud actual de la entidad.</summary>
        public int EntityHealth
        {
            get => currentHealth;
            set
            {
                int oldHealth = currentHealth;
                currentHealth = Mathf.Clamp(value, 0, maxHealth);

                if (oldHealth != currentHealth)
                    OnHealthChanged(oldHealth, currentHealth);

                if (currentHealth <= 0 && !isDead)
                {
                    isDead = true;
                    OnHealthZero();
                }
                else if (currentHealth >= maxHealth)
                {
                    OnHealthMax();
                }
            }
        }

        /// <summary>Salud máxima de la entidad.</summary>
        public int MaxEntityHealth
        {
            get => maxHealth;
            set => maxHealth = value;
        }

        /// <summary>Si la entidad está muerta (salud <= 0).</summary>
        public bool IsDead => isDead;

        /// <summary>Salud como porcentaje [0-1].</summary>
        public float HealthPercent => maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;

        protected virtual void Awake()
        {
            currentHealth = startHealth;
        }

        /// <summary>Recibir daño.</summary>
        public virtual void OnApplyDamage(int damage, Transform sender = null)
        {
            if (isDead) return;
            EntityHealth -= damage;
        }

        /// <summary>Aplicar daño máximo (matar).</summary>
        public virtual void ApplyDamageMax(Transform sender = null)
        {
            if (isDead) return;
            EntityHealth = 0;
        }

        /// <summary>Recibir curación.</summary>
        public virtual void OnApplyHeal(int healAmount)
        {
            if (isDead) return;
            EntityHealth += healAmount;
        }

        /// <summary>Curar al máximo.</summary>
        public virtual void ApplyHealMax()
        {
            if (isDead) return;
            EntityHealth = maxHealth;
        }

        /// <summary>
        /// Callback cuando la salud cambia. Override para actualizar UI (barra de vida, números, etc.).
        /// </summary>
        protected virtual void OnHealthChanged(int oldHealth, int newHealth) { }

        /// <summary>
        /// Callback cuando la salud llega a 0. Override para muerte (animación, ragdoll, UI de game over).
        /// </summary>
        protected virtual void OnHealthZero() { }

        /// <summary>
        /// Callback cuando la salud llega al máximo. Override para efectos de full health.
        /// </summary>
        protected virtual void OnHealthMax() { }

        /// <summary>Resetear la salud y el estado de muerte.</summary>
        public virtual void ResetHealth()
        {
            isDead = false;
            EntityHealth = startHealth;
        }
    }
}
