using UnityEngine;
using UnityEngine.Events;

namespace FuncionalidadesCore.Health
{
    /// <summary>
    /// Implementación concreta y lista para usar de BaseHealthEntity.
    /// Exponelo en el Inspector para configurar la vida y conectar acciones en UnityEvents (UI, muertes, efectos).
    /// </summary>
    public class EntityHealth : BaseHealthEntity
    {
        [Header("Eventos en Unity (Inspector)")]
        [Tooltip("Se dispara cada vez que la salud cambia. Pasa el número de vida restante.")]
        public UnityEvent<int> OnHealthChangedEvent;
        
        [Tooltip("Se dispara cuando este objeto recibe daño.")]
        public UnityEvent OnDamagedEvent;
        
        [Tooltip("Se dispara cuando este objeto es curado.")]
        public UnityEvent OnHealedEvent;
        
        [Tooltip("Se dispara cuando la vida llega a cero.")]
        public UnityEvent OnDeathEvent;

        protected override void OnHealthChanged(int oldHealth, int newHealth)
        {
            OnHealthChangedEvent?.Invoke(newHealth);
            
            if (newHealth < oldHealth)
                OnDamagedEvent?.Invoke();
            else if (newHealth > oldHealth)
                OnHealedEvent?.Invoke();
        }

        protected override void OnHealthZero()
        {
            OnDeathEvent?.Invoke();
            
            // Opcional: Desactivar o destruir el objeto por defecto si es un enemigo básico
            // Destroy(gameObject);
        }

        protected override void OnHealthMax()
        {
            // Lógica cuando llega al 100%
        }
    }
}
