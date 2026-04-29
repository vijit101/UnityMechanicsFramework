using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Interaction
{
    /// <summary>
    /// A standalone UI component that subscribes to EventBus events and shows
    /// or hides the interaction prompt. It never references the controller or
    /// any interactable directly — all communication is event-driven.
    ///
    /// This component can live anywhere in the scene hierarchy. It can be
    /// swapped for a completely different UI design without touching any
    /// gameplay code.
    /// </summary>
    public class InteractionPrompt_UMFOSS : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        // Serialized fields
        // ──────────────────────────────────────────────

        [Header("UI References")]
        [Tooltip("The parent panel or container that holds the entire prompt UI. Activated/deactivated to show/hide.")]
        [SerializeField] private GameObject promptPanel;

        [Tooltip("The TextMeshPro label that displays the interaction prompt text (e.g. 'Press E to open').")]
        [SerializeField] private TextMeshProUGUI promptLabel;

        [Tooltip("Optional progress bar slider shown during hold-to-interact. Set fill to 0-1.")]
        [SerializeField] private Slider holdProgressBar;

        // ──────────────────────────────────────────────
        // Unity lifecycle methods
        // ──────────────────────────────────────────────

        private void OnEnable()
        {
            EventBus.Subscribe<InteractableDetectedEvent>(OnDetected);
            EventBus.Subscribe<InteractableLostEvent>(OnLost);
            EventBus.Subscribe<HoldInteractProgressEvent>(OnHoldProgress);
            EventBus.Subscribe<HoldInteractCancelledEvent>(OnHoldCancelled);
            // Note: we do NOT subscribe to InteractionPerformedEvent here.
            // Subscribing to it caused a 1-frame prompt flicker on repeatable objects (NPC):
            // the prompt would hide on performed, then re-show next frame when NPC re-focused.
            // InteractableLostEvent already handles hiding for single-use objects (pickups, doors).
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<InteractableDetectedEvent>(OnDetected);
            EventBus.Unsubscribe<InteractableLostEvent>(OnLost);
            EventBus.Unsubscribe<HoldInteractProgressEvent>(OnHoldProgress);
            EventBus.Unsubscribe<HoldInteractCancelledEvent>(OnHoldCancelled);
        }

        private void Start()
        {
            // Start hidden
            HidePrompt();
        }

        // ──────────────────────────────────────────────
        // Event handlers
        // ──────────────────────────────────────────────

        /// <summary>
        /// Called when a new interactable is focused. Shows the prompt panel
        /// and sets the label text from the event data.
        /// </summary>
        private void OnDetected(InteractableDetectedEvent eventData)
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(true);
            }

            if (promptLabel != null)
            {
                promptLabel.text = eventData.promptText;
            }

            // Reset progress bar when a new target is detected
            ResetProgressBar();
        }

        /// <summary>
        /// Called when the focused interactable is lost. Hides the entire prompt.
        /// </summary>
        private void OnLost(InteractableLostEvent eventData)
        {
            HidePrompt();
        }

        /// <summary>
        /// Called every frame during hold-to-interact. Updates the progress bar fill.
        /// </summary>
        private void OnHoldProgress(HoldInteractProgressEvent eventData)
        {
            if (holdProgressBar != null)
            {
                holdProgressBar.gameObject.SetActive(true);
                holdProgressBar.value = eventData.progress;
            }
        }

        /// <summary>
        /// Called when hold interaction is cancelled (released early or left range).
        /// Resets the progress bar to zero.
        /// </summary>
        private void OnHoldCancelled(HoldInteractCancelledEvent eventData)
        {
            ResetProgressBar();
        }

        // ──────────────────────────────────────────────
        // Private methods
        // ──────────────────────────────────────────────

        /// <summary>
        /// Hides the prompt panel and resets the progress bar.
        /// </summary>
        private void HidePrompt()
        {
            if (promptPanel != null)
            {
                promptPanel.SetActive(false);
            }

            ResetProgressBar();
        }

        /// <summary>
        /// Resets the hold progress bar to zero and hides it.
        /// </summary>
        private void ResetProgressBar()
        {
            if (holdProgressBar != null)
            {
                holdProgressBar.value = 0f;
                holdProgressBar.gameObject.SetActive(false);
            }
        }
    }
}
