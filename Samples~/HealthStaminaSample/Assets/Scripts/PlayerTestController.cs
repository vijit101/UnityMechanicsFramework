using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Demo controller for testing HealthSystem and StaminaSystem.
    /// Handles keyboard input and displays current values on screen.
    /// This script is for demonstration purposes only - not intended for production use.
    /// Uses the new Unity Input System.
    /// </summary>
    public class PlayerTestController : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private HealthSystem healthSystem;
        [SerializeField] private StaminaSystem staminaSystem;
        
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI staminaText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        
        [Header("Test Values")]
        [SerializeField] private float damageAmount = 20f;
        [SerializeField] private float healAmount = 15f;
        [SerializeField] private float staminaCost = 25f;
        
        void Start()
        {
            // Auto-find components if not assigned
            if (healthSystem == null)
                healthSystem = GetComponent<HealthSystem>();
            if (staminaSystem == null)
                staminaSystem = GetComponent<StaminaSystem>();
            
            // Subscribe to events
            if (healthSystem != null)
            {
                healthSystem.OnHealthChanged.AddListener(UpdateHealthUI);
                healthSystem.OnDeath.AddListener(OnPlayerDeath);
            }
            
            if (staminaSystem != null)
            {
                staminaSystem.OnStaminaChanged.AddListener(UpdateStaminaUI);
            }
            
            // Initial UI update
            UpdateHealthUI(healthSystem.CurrentHealth, healthSystem.MaxHealth);
            UpdateStaminaUI(staminaSystem.CurrentStamina, staminaSystem.MaxStamina);
            
            // Display instructions
            if (instructionsText != null)
            {
                instructionsText.text = "CONTROLS:\n" +
                    "D - Take Damage (20)\n" +
                    "H - Heal (15)\n" +
                    "SPACE - Use Stamina (25)\n" +
                    "R - Reset Scene";
            }
        }
        
        void Update()
        {
            // Check if Keyboard is available (new Input System)
            if (Keyboard.current == null) return;
            
            // Damage input
            if (Keyboard.current.dKey.wasPressedThisFrame)
            {
                healthSystem?.TakeDamage(damageAmount);
                Debug.Log($"Took {damageAmount} damage!");
            }
            
            // Heal input
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                healthSystem?.Heal(healAmount);
                Debug.Log($"Healed {healAmount} health!");
            }
            
            // Consume stamina input
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (staminaSystem != null && staminaSystem.CurrentStamina >= staminaCost)
                {
                    staminaSystem.ConsumeStamina(staminaCost);
                    Debug.Log($"Consumed {staminaCost} stamina!");
                }
                else
                {
                    Debug.Log("Not enough stamina!");
                }
            }
            
            // Reset scene
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
                );
            }
        }
        
        void UpdateHealthUI(float current, float max)
        {
            if (healthText != null)
            {
                float percentage = (current / max) * 100f;
                healthText.text = $"HEALTH: {current:F0} / {max:F0} ({percentage:F0}%)";
                
                // Color based on health percentage
                if (percentage > 50f)
                    healthText.color = Color.green;
                else if (percentage > 25f)
                    healthText.color = Color.yellow;
                else
                    healthText.color = Color.red;
            }
        }
        
        void UpdateStaminaUI(float current, float max)
        {
            if (staminaText != null)
            {
                float percentage = (current / max) * 100f;
                staminaText.text = $"STAMINA: {current:F0} / {max:F0} ({percentage:F0}%)";
                
                // Color based on stamina percentage
                if (percentage > 50f)
                    staminaText.color = Color.cyan;
                else if (percentage > 25f)
                    staminaText.color = Color.yellow;
                else
                    staminaText.color = Color.red;
            }
        }
        
        void OnPlayerDeath()
        {
            Debug.Log("Player died! Press R to restart.");
            if (healthText != null)
            {
                healthText.text = "DEAD - Press R to Restart";
                healthText.color = Color.red;
            }
        }
    }
}
