using UnityEngine;
using UnityEngine.UI;

namespace GameplayMechanicsUMFOSS.Systems.UI
{
    /// <summary>
    /// Optional UI component that displays stamina as a visual progress bar.
    /// Listens to StaminaSystem events and updates the bar fill automatically.
    /// Works with Unity's Image component in "Filled" mode.
    /// </summary>
    public class StaminaBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private StaminaSystem staminaSystem;
        [SerializeField] private Image fillImage;
        
        [Header("Optional: Smooth Animation")]
        [SerializeField] private bool useSmoothTransition = true;
        [SerializeField] private float smoothSpeed = 5f;
        
        private float targetFillAmount;
        
        void Start()
        {
            if (staminaSystem != null && fillImage != null)
            {
                // Subscribe to events
                staminaSystem.OnStaminaChanged.AddListener(UpdateBar);
                
                // Initialize immediately with current stamina (runs after StaminaSystem.Awake)
                float currentPercent = staminaSystem.CurrentStamina / staminaSystem.MaxStamina;
                fillImage.fillAmount = currentPercent;
                targetFillAmount = currentPercent;
            }
        }
        
        void OnEnable()
        {
            if (staminaSystem != null)
            {
                staminaSystem.OnStaminaChanged.AddListener(UpdateBar);
            }
        }
        
        void OnDisable()
        {
            if (staminaSystem != null)
            {
                staminaSystem.OnStaminaChanged.RemoveListener(UpdateBar);
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
        /// Updates the stamina bar fill amount based on current stamina.
        /// Called automatically when stamina changes via event subscription.
        /// </summary>
        void UpdateBar(float currentStamina, float maxStamina)
        {
            if (fillImage == null) return;
            
            targetFillAmount = currentStamina / maxStamina;
            
            if (!useSmoothTransition)
            {
                fillImage.fillAmount = targetFillAmount;
            }
        }
    }
}
