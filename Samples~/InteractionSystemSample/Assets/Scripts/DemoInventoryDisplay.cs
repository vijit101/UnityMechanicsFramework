using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Interaction;

namespace GameplayMechanicsUMFOSS.Samples.Interaction
{
    /// <summary>
    /// Demo UI component that displays collected items on-screen.
    /// Subscribes to ItemPickedUpEvent and InteractionPerformedEvent via EventBus.
    /// Also shows the current detection mode of the InteractionController.
    /// </summary>
    public class DemoInventoryDisplay : MonoBehaviour
    {
        [Header("UI References")]
        [Tooltip("TextMeshPro label that displays the list of collected items.")]
        [SerializeField] private TextMeshProUGUI inventoryLabel;

        [Tooltip("TextMeshPro label that displays the current detection mode.")]
        [SerializeField] private TextMeshProUGUI detectionModeLabel;

        [Header("References")]
        [Tooltip("Reference to the InteractionController to read detection mode from.")]
        [SerializeField] private InteractionController_UMFOSS controller;

        // ──────────────────────────────────────────────
        // Private fields
        // ──────────────────────────────────────────────

        private readonly List<string> collectedItems = new List<string>();

        // ──────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────

        private void OnEnable()
        {
            EventBus.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
        }

        private void Start()
        {
            UpdateInventoryUI();
            UpdateDetectionModeUI();
        }

        private void Update()
        {
            UpdateDetectionModeUI();
        }

        // ──────────────────────────────────────────────
        // Event handlers
        // ──────────────────────────────────────────────

        private void OnItemPickedUp(ItemPickedUpEvent eventData)
        {
            collectedItems.Add(eventData.itemName);
            UpdateInventoryUI();
        }

        // ──────────────────────────────────────────────
        // Private methods
        // ──────────────────────────────────────────────

        private void UpdateInventoryUI()
        {
            if (inventoryLabel == null) return;

            if (collectedItems.Count == 0)
            {
                inventoryLabel.text = "Inventory: (empty)";
            }
            else
            {
                inventoryLabel.text = "Inventory:\n" + string.Join("\n- ", collectedItems.ToArray());
            }
        }

        private void UpdateDetectionModeUI()
        {
            if (detectionModeLabel == null || controller == null) return;

            detectionModeLabel.text = $"Detection: {controller.CurrentDetectionMode}";
        }
    }
}
