using UnityEngine;
using UnityEngine.UI;

namespace GameplayMechanicsUMFOSS.Systems.UI
{
    /// <summary>
    /// Optional UI component that displays health as a visual progress bar.
    /// Listens to HealthSystem events and updates the bar fill automatically.
    /// Works with Unity's Image component in "Filled" mode.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private Image fillImage;
        
        [Header("Optional: Smooth Animation")]
        [SerializeField] private bool useSmoothTransition = true;
        [SerializeField] private float smoothSpeed = 5f;
        
        private float targetFillAmount;
        
        void Start()
        {
            if (healthSystem != null && fillImage != null)
            {
                // Subscribe to events
                healthSystem.OnHealthChanged.AddListener(UpdateBar);
                
                // Initialize immediately with current health (runs after HealthSystem.Awake)
                float currentPercent = healthSystem.CurrentHealth / healthSystem.MaxHealth;
                fillImage.fillAmount = currentPercent;
                targetFillAmount = currentPercent;
            }
        }
        
        void OnEnable()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged.AddListener(UpdateBar);
            }
        }
        
        void OnDisable()
        {
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged.RemoveListener(UpdateBar);
            }
        }
        
        void Update()
        {
            if (useSmoothTransition && fillImage != null)
            {
                // Smoothly lerp to target fill amount
                fillImage.fillAmount = Mathf.Lerp(
                    fillImage.fillAmount, 
                    targetFillAmount, 
                    Time.deltaTime * smoothSpeed
                );
            }
        }
        
        /// <summary>
        /// Updates the health bar fill amount based on current health.
        /// Called automatically when health changes via event subscription.
        /// </summary>
        void UpdateBar(float currentHealth, float maxHealth)
        {
            if (fillImage == null) return;
            
            targetFillAmount = currentHealth / maxHealth;
            
            if (!useSmoothTransition)
            {
                fillImage.fillAmount = targetFillAmount;
            }
        }
    }
}
