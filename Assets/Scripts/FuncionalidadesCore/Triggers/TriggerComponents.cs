using UnityEngine;

namespace FuncionalidadesCore
{
    /// <summary>
    /// Trigger genérico de daño desacoplado.
    /// Causa daño a cualquier objeto con IDamagable al entrar/permanecer en el trigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class DamageTriggerCore : MonoBehaviour
    {
        public enum DamageMode { OnEnter, OnStay }

        [Header("Damage Settings")]
        [SerializeField] private int damageAmount = 10;
        [SerializeField] private DamageMode damageMode = DamageMode.OnEnter;
        [SerializeField] private float damageInterval = 1f;
        [SerializeField] private bool applyOnce = false;

        private float lastDamageTime;
        private bool hasDamaged;

        private void OnTriggerEnter(Collider other)
        {
            if (damageMode == DamageMode.OnEnter && (!applyOnce || !hasDamaged))
            {
                ApplyDamage(other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (damageMode == DamageMode.OnStay && Time.time >= lastDamageTime + damageInterval)
            {
                ApplyDamage(other);
                lastDamageTime = Time.time;
            }
        }

        private void ApplyDamage(Collider target)
        {
            if (target.TryGetComponent<IDamagable>(out var damagable))
            {
                damagable.OnApplyDamage(damageAmount, transform);
                hasDamaged = true;
            }
        }
    }

    /// <summary>
    /// Trigger genérico de eventos - dispara UnityEvents al entrar/salir del trigger.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class TriggerEventsCore : MonoBehaviour
    {
        [Header("Trigger Settings")]
        [SerializeField] private LayerMask triggerLayer = ~0;
        [SerializeField] private bool triggerOnce = false;
        [SerializeField] private bool disableAfterTrigger = false;

        public UnityEngine.Events.UnityEvent OnTriggerEnterEvent;
        public UnityEngine.Events.UnityEvent OnTriggerExitEvent;
        public UnityEngine.Events.UnityEvent<Collider> OnTriggerEnterCollider;

        private bool hasTriggered;

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered && triggerOnce) return;
            if (((1 << other.gameObject.layer) & triggerLayer) == 0) return;

            OnTriggerEnterEvent?.Invoke();
            OnTriggerEnterCollider?.Invoke(other);
            hasTriggered = true;

            if (disableAfterTrigger)
                gameObject.SetActive(false);
        }

        private void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & triggerLayer) == 0) return;
            OnTriggerExitEvent?.Invoke();
        }
    }
}
