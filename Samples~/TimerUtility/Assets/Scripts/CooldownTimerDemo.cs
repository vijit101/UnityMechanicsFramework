using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Samples.TimerUtility
{
    /// <summary>
    /// Demo 1: Cooldown Timer
    /// Greys out a UI button for 5 seconds and re-enables it when the cooldown ends.
    /// Auto-finds AbilityButton and CooldownLabel by name — no manual wiring needed.
    /// </summary>
    public class CooldownTimerDemo : MonoBehaviour
    {
        [Header("References (auto-found by name if left empty)")]
        [SerializeField] private Button              abilityButton;
        [SerializeField] private TMP_Text            cooldownLabel;
        [SerializeField] private TimerUtility_UMFOSS cooldownTimer;

        private void Awake()
        {
            // Auto-find if not manually assigned in Inspector
            if (abilityButton == null)
            {
                var go = GameObject.Find("AbilityButton");
                if (go != null) abilityButton = go.GetComponent<Button>();
            }

            if (cooldownLabel == null)
            {
                var go = GameObject.Find("CooldownLabel");
                if (go != null) cooldownLabel = go.GetComponent<TMP_Text>();
            }

            if (cooldownTimer == null)
                cooldownTimer = GetComponent<TimerUtility_UMFOSS>();
        }

        private void Start()
        {
            if (cooldownTimer == null) { Debug.LogError("[CooldownTimerDemo] TimerUtility_UMFOSS not found!"); return; }
            if (abilityButton == null) { Debug.LogError("[CooldownTimerDemo] AbilityButton not found! Make sure a GameObject named 'AbilityButton' exists."); return; }

            cooldownTimer.OnTimerStart    += OnCooldownStart;
            cooldownTimer.OnTimerTick     += OnCooldownTick;
            cooldownTimer.OnTimerComplete += OnCooldownEnd;

            abilityButton.onClick.AddListener(OnAbilityPressed);

            if (cooldownLabel != null)
                cooldownLabel.text = "Ability Ready!";
        }

        private void OnDestroy()
        {
            if (cooldownTimer == null) return;
            cooldownTimer.OnTimerStart    -= OnCooldownStart;
            cooldownTimer.OnTimerTick     -= OnCooldownTick;
            cooldownTimer.OnTimerComplete -= OnCooldownEnd;
        }

        private void OnAbilityPressed()
        {
            if (!cooldownTimer.IsRunning())
                cooldownTimer.Start();
        }

        private void OnCooldownStart()
        {
            if (abilityButton != null) abilityButton.interactable = false;
            if (cooldownLabel != null) cooldownLabel.text = "On Cooldown...";
        }

        private void OnCooldownTick(float timeRemaining)
        {
            if (cooldownLabel != null) cooldownLabel.text = $"Cooldown: {timeRemaining:F1}s";
        }

        private void OnCooldownEnd()
        {
            if (abilityButton != null) abilityButton.interactable = true;
            if (cooldownLabel != null) cooldownLabel.text = "Ability Ready!";
        }
    }
}
