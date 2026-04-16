using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Provides a simple health driver that publishes combat presentation events through the EventBus.
    /// </summary>
    public class HealthSystem_UMFOSS : MonoBehaviour
    {
        private const float MIN_VALUE = 0f;

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;

        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;

        private void Awake()
        {
            currentHealth = Mathf.Clamp(currentHealth, MIN_VALUE, maxHealth);
        }

        /// <summary>
        /// Applies non-critical damage and publishes a damage event for UI responders.
        /// </summary>
        public void ApplyDamage(float amount, DamagePresentation presentation = DamagePresentation.Damage)
        {
            if (amount <= MIN_VALUE)
            {
                return;
            }

            currentHealth = Mathf.Max(MIN_VALUE, currentHealth - amount);
            global::EventBus.Publish(new DamageTakenEvent(transform, amount, presentation));
        }

        /// <summary>
        /// Applies a critical hit and publishes a dedicated critical event so listeners only spawn one popup.
        /// </summary>
        public void ApplyCriticalDamage(float amount)
        {
            if (amount <= MIN_VALUE)
            {
                return;
            }

            currentHealth = Mathf.Max(MIN_VALUE, currentHealth - amount);
            global::EventBus.Publish(new CriticalHitEvent(transform, amount));
        }

        /// <summary>
        /// Restores health and publishes the actual healed amount.
        /// </summary>
        public void Heal(float amount)
        {
            if (amount <= MIN_VALUE)
            {
                return;
            }

            float previousHealth = currentHealth;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            float healedAmount = currentHealth - previousHealth;
            if (healedAmount > MIN_VALUE)
            {
                global::EventBus.Publish(new HealedEvent(transform, healedAmount));
            }
        }

        /// <summary>
        /// Publishes a shield block presentation event without changing health.
        /// </summary>
        public void BlockDamage(float amount)
        {
            if (amount <= MIN_VALUE)
            {
                return;
            }

            global::EventBus.Publish(new ShieldBlockEvent(transform, amount));
        }

        /// <summary>
        /// Publishes a miss event for this target.
        /// </summary>
        public void RegisterMiss()
        {
            global::EventBus.Publish(new MissEvent(transform));
        }

        /// <summary>
        /// Publishes a sample experience gain event for this target.
        /// </summary>
        public void GainExperience(float amount)
        {
            if (amount <= MIN_VALUE)
            {
                return;
            }

            global::EventBus.Publish(new ExperienceGainedEvent(transform, amount));
        }

        /// <summary>
        /// Resets the component back to full health.
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
        }
    }
}
