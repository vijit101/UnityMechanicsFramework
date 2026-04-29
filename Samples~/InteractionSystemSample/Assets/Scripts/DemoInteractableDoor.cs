using UnityEngine;
using GameplayMechanicsUMFOSS.Interaction;

namespace GameplayMechanicsUMFOSS.Samples.Interaction
{
    /// <summary>
    /// Demo interactable: a door that opens on interact and cannot be re-opened.
    /// Shows outline on focus, hides outline on unfocus.
    /// </summary>
    public class DemoInteractableDoor : MonoBehaviour, IInteractable_UMFOSS
    {
        [Header("Door Settings")]
        [Tooltip("The angle to rotate the door to when opened.")]
        [SerializeField] private float openAngle = 90f;

        [Tooltip("Speed of the door opening animation.")]
        [SerializeField] private float openSpeed = 3f;

        [Header("Visual Feedback")]
        [Tooltip("Optional SpriteRenderer used to show highlight when focused.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Color applied when the player is in range and the door is interactable.")]
        [SerializeField] private Color highlightColor = Color.yellow;

        [Header("Priority")]
        [Tooltip("Selection priority when using HighestPriority mode.")]
        [SerializeField] private int priority = 0;

        // ──────────────────────────────────────────────
        // Private fields
        // ──────────────────────────────────────────────

        private bool isOpen = false;
        private bool isOpening = false;
        private Color originalColor;
        private Quaternion closedRotation;
        private Quaternion openRotation;

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

            closedRotation = transform.rotation;
            openRotation = Quaternion.Euler(0f, 0f, openAngle) * closedRotation;
        }

        private void Update()
        {
            if (isOpening)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    openRotation,
                    Time.deltaTime * openSpeed
                );

                // Stop animating when close enough
                if (Quaternion.Angle(transform.rotation, openRotation) < 0.5f)
                {
                    transform.rotation = openRotation;
                    isOpening = false;
                }
            }
        }

        // ──────────────────────────────────────────────
        // IInteractable_UMFOSS implementation
        // ──────────────────────────────────────────────

        public void Interact(GameObject interactor)
        {
            if (isOpen) return;

            isOpen = true;
            isOpening = true;
            Debug.Log($"[InteractionSystem] Door opened by {interactor.name}");
        }

        public void OnFocused(GameObject interactor)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }

        public void OnUnfocused(GameObject interactor)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        public string GetInteractionPrompt()
        {
            return "Press E to open";
        }

        public bool CanInteract(GameObject interactor)
        {
            return !isOpen;
        }
    }
}
