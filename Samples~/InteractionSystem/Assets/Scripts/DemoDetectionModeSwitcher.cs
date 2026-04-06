using UnityEngine;
using TMPro;
using GameplayMechanicsUMFOSS.Interaction;

namespace GameplayMechanicsUMFOSS.Samples.Interaction
{
    /// <summary>
    /// Demo utility that cycles through detection modes at runtime via a button
    /// or keyboard shortcut. Useful for comparing how Trigger, OverlapCircle,
    /// and Raycast detection feel in-game.
    /// </summary>
    public class DemoDetectionModeSwitcher : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The InteractionController to switch detection modes on.")]
        [SerializeField] private InteractionController_UMFOSS controller;

        [Header("Input")]
        [Tooltip("Key to press to cycle to the next detection mode.")]
        [SerializeField] private KeyCode switchKey = KeyCode.Tab;

        [Header("UI")]
        [Tooltip("Optional label to display the current mode.")]
        [SerializeField] private TextMeshProUGUI modeLabel;

        // ──────────────────────────────────────────────
        // Private fields
        // ──────────────────────────────────────────────

        private const int DETECTION_MODE_COUNT = 3;

        // ──────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────

        private void Start()
        {
            UpdateLabel();
        }

        private void Update()
        {
            if (Input.GetKeyDown(switchKey))
            {
                CycleDetectionMode();
            }
        }

        // ──────────────────────────────────────────────
        // Public methods
        // ──────────────────────────────────────────────

        /// <summary>
        /// Cycles to the next detection mode. Can be called from a UI Button's OnClick.
        /// </summary>
        public void CycleDetectionMode()
        {
            if (controller == null)
            {
                Debug.LogWarning("[InteractionSystem] DemoDetectionModeSwitcher: No controller assigned.");
                return;
            }

            int currentIndex = (int)controller.CurrentDetectionMode;
            int nextIndex = (currentIndex + 1) % DETECTION_MODE_COUNT;
            DetectionMode nextMode = (DetectionMode)nextIndex;

            controller.SetDetectionMode(nextMode);
            UpdateLabel();

            Debug.Log($"[InteractionSystem] Detection mode switched to: {nextMode}");
        }

        // ──────────────────────────────────────────────
        // Private methods
        // ──────────────────────────────────────────────

        private void UpdateLabel()
        {
            if (modeLabel != null && controller != null)
            {
                modeLabel.text = $"Detection: {controller.CurrentDetectionMode}\n(Press {switchKey} to switch)";
            }
        }
    }
}
