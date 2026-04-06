using UnityEngine;
using GameplayMechanicsUMFOSS.Interaction;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Samples.Interaction
{
    /// <summary>
    /// Event published when an item is picked up.
    /// Other systems (UI, inventory, achievements) can subscribe.
    /// </summary>
    public struct ItemPickedUpEvent
    {
        public string itemName;
        public GameObject picker;
    }

    /// <summary>
    /// Demo interactable: an item pickup that deactivates itself after collection.
    /// Floats up and down while in range. Single-use.
    /// </summary>
    public class DemoInteractablePickup : MonoBehaviour, IInteractable_UMFOSS
    {
        [Header("Item Settings")]
        [Tooltip("Display name of the item.")]
        [SerializeField] private string itemName = "Health Potion";

        [Header("Visual Feedback")]
        [Tooltip("Speed of the floating animation when focused.")]
        [SerializeField] private float floatSpeed = 2f;

        [Tooltip("Height of the floating animation.")]
        [SerializeField] private float floatHeight = 0.3f;

        [Header("Priority")]
        [SerializeField] private int priority = 0;

        // ──────────────────────────────────────────────
        // Private fields
        // ──────────────────────────────────────────────

        private bool isFocused = false;
        private Vector3 originalPosition;

        // ──────────────────────────────────────────────
        // Public properties
        // ──────────────────────────────────────────────

        public int Priority => priority;

        /// <summary>The display name of this pickup item.</summary>
        public string ItemName => itemName;

        // ──────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────

        private void Awake()
        {
            originalPosition = transform.position;
        }

        private void Update()
        {
            if (isFocused)
            {
                // Float up and down using a sine wave
                float newY = originalPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
                transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
            }
        }

        // ──────────────────────────────────────────────
        // IInteractable_UMFOSS implementation
        // ──────────────────────────────────────────────

        public void Interact(GameObject interactor)
        {
            Debug.Log($"[InteractionSystem] {interactor.name} picked up {itemName}");

            EventBus.Publish(new ItemPickedUpEvent
            {
                itemName = itemName,
                picker = interactor
            });

            gameObject.SetActive(false);
        }

        public void OnFocused(GameObject interactor)
        {
            isFocused = true;
        }

        public void OnUnfocused(GameObject interactor)
        {
            isFocused = false;
            transform.position = originalPosition;
        }

        public string GetInteractionPrompt()
        {
            return $"Press E to pick up {itemName}";
        }

        public bool CanInteract(GameObject interactor)
        {
            return gameObject.activeSelf;
        }
    }
}
