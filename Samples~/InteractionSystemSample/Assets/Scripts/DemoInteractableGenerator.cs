using UnityEngine;
using GameplayMechanicsUMFOSS.Interaction;

namespace GameplayMechanicsUMFOSS.Samples.Interaction
{
    /// <summary>
    /// Demo interactable: a generator that requires holding the interact key
    /// for a configured duration. Demonstrates hold-to-interact behavior.
    ///
    /// The InteractionController_UMFOSS handles the hold logic — this object
    /// only needs to implement the standard IInteractable_UMFOSS interface.
    /// The controller's "Require Hold" setting must be enabled for this
    /// to work as intended.
    ///
    /// Note: For a mixed scene with both instant and hold interactions,
    /// you can have separate InteractionController_UMFOSS configurations
    /// or check the prompt text for "Hold". The current system applies
    /// hold behavior globally on the controller. A future enhancement
    /// could add RequiresHold to the interface itself.
    /// </summary>
    public class DemoInteractableGenerator : MonoBehaviour, IInteractable_UMFOSS
    {
        [Header("Generator Settings")]
        [Tooltip("Whether the generator has been activated.")]
        [SerializeField] private bool isActivated = false;

        [Header("Visual Feedback")]
        [Tooltip("Optional SpriteRenderer for color changes.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Color when the generator is activated.")]
        [SerializeField] private Color activatedColor = Color.green;

        [Tooltip("Color when focused but not yet activated.")]
        [SerializeField] private Color focusedColor = Color.cyan;

        [Header("Priority")]
        [SerializeField] private int priority = 0;

        // ──────────────────────────────────────────────
        // Private fields
        // ──────────────────────────────────────────────

        private Color originalColor;

        // ──────────────────────────────────────────────
        // Public properties
        // ──────────────────────────────────────────────

        public int Priority => priority;

        // ──────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                originalColor = spriteRenderer.color;
            }
        }

        // ──────────────────────────────────────────────
        // IInteractable_UMFOSS implementation
        // ──────────────────────────────────────────────

        public void Interact(GameObject interactor)
        {
            isActivated = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = activatedColor;
            }

            Debug.Log($"[InteractionSystem] Generator activated by {interactor.name}!");
        }

        public void OnFocused(GameObject interactor)
        {
            if (!isActivated && spriteRenderer != null)
            {
                spriteRenderer.color = focusedColor;
            }
        }

        public void OnUnfocused(GameObject interactor)
        {
            if (!isActivated && spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        public string GetInteractionPrompt()
        {
            return isActivated ? "Already activated" : "Hold E to activate";
        }

        public bool CanInteract(GameObject interactor)
        {
            return !isActivated;
        }
    }
}
