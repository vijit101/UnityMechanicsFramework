using UnityEngine;
using UnityEngine.Events;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Independent stamina management component for any entity that needs stamina.
    /// Handles stamina consumption and automatic regeneration with configurable delay.
    /// Useful for dash, jump, special attacks, or any stamina-based mechanics.
    /// </summary>
    public class StaminaSystem : MonoBehaviour
    {
        [Header("Stamina Configuration")]
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float regenRate = 20f; // Stamina regenerated per second
        [SerializeField] private float regenDelay = 1f; // Delay in seconds before regen starts
        
        private float currentStamina;
        private float timeSinceLastConsume;
        
        [Header("Events")]
        public UnityEvent<float, float> OnStaminaChanged; // (current, max)
        
        void Awake()
        {
            currentStamina = maxStamina;
            timeSinceLastConsume = regenDelay; // Start ready to regen
        }
        
        void Update()
        {
            // Auto-regenerate stamina after delay
            if (currentStamina < maxStamina)
            {
                timeSinceLastConsume += Time.deltaTime;
                
                if (timeSinceLastConsume >= regenDelay)
                {
                    RegenerateStamina();
                }
            }
        }
        
        /// <summary>
        /// Consumes the specified amount of stamina.
        /// Clamps to 0 and resets regeneration delay timer.
        /// </summary>
        /// <param name="amount">Amount of stamina to consume</param>
        public void ConsumeStamina(float amount)
        {
            currentStamina = Mathf.Max(0, currentStamina - amount);
            timeSinceLastConsume = 0f; // Reset delay timer
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        /// <summary>
        /// Regenerates stamina over time based on regenRate.
        /// Called automatically in Update() after regenDelay.
        /// Can also be called manually for instant regeneration effects.
        /// </summary>
        public void RegenerateStamina()
        {
            currentStamina = Mathf.Min(maxStamina, currentStamina + regenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke(currentStamina, maxStamina);
        }
        
        /// <summary>
        /// Returns the current stamina value (read-only).
        /// </summary>
        public float CurrentStamina => currentStamina;
        
        /// <summary>
        /// Returns the maximum stamina value (read-only).
        /// </summary>
        public float MaxStamina => maxStamina;
    }
}
