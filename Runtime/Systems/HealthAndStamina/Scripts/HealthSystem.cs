using UnityEngine;
using UnityEngine.Events;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Independent health management component for any entity (player, enemy, destructible).
    /// Handles damage, healing, and death state with event-driven architecture.
    /// Attach to any GameObject that needs health tracking.
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [Header("Health Configuration")]
        [SerializeField] private float maxHealth = 100f;
        
        private float currentHealth;
        
        [Header("Events")]
        public UnityEvent<float, float> OnHealthChanged; // (current, max)
        public UnityEvent OnDeath;
        
        void Awake()
        {
            currentHealth = maxHealth;
        }
        
        /// <summary>
        /// Reduces current health by the specified amount.
        /// Clamps to 0 and triggers death event when health reaches 0.
        /// </summary>
        /// <param name="amount">Amount of damage to apply</param>
        public void TakeDamage(float amount)
        {
            if (currentHealth <= 0) return; // Already dead, ignore damage
            
            currentHealth = Mathf.Max(0, currentHealth - amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (currentHealth <= 0)
            {
                OnDeath?.Invoke();
            }
        }
        
        /// <summary>
        /// Increases current health by the specified amount.
        /// Clamps to maxHealth. Cannot heal when dead.
        /// </summary>
        /// <param name="amount">Amount of health to restore</param>
        public void Heal(float amount)
        {
            if (currentHealth <= 0) return; // Cannot heal when dead
            
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        /// <summary>
        /// Returns the current health value (read-only).
        /// </summary>
        public float CurrentHealth => currentHealth;
        
        /// <summary>
        /// Returns the maximum health value (read-only).
        /// </summary>
        public float MaxHealth => maxHealth;
    }
}
