using UnityEngine;
using GameplayMechanicsUMFOSS.Interaction;

namespace GameplayMechanicsUMFOSS.Samples.Interaction
{
    /// <summary>
    /// Demo interactable: an NPC that starts a conversation on interact.
    /// Repeatable — can be interacted with again after the dialogue ends.
    /// Shows a name tag on focus, hides it on unfocus.
    /// </summary>
    public class DemoInteractableNPC : MonoBehaviour, IInteractable_UMFOSS
    {
        [Header("NPC Settings")]
        [Tooltip("The name displayed above the NPC when focused.")]
        [SerializeField] private string npcName = "Village Elder";

        [Tooltip("Dialogue lines the NPC will cycle through.")]
        [SerializeField] private string[] dialogueLines = new string[]
        {
            "Hello, traveler! Welcome to our village.",
            "The forest to the east holds many secrets.",
            "Be careful out there — danger lurks in the shadows."
        };

        [Header("Visual Feedback")]
        [Tooltip("Optional GameObject that acts as a name tag above the NPC. Shown on focus.")]
        [SerializeField] private GameObject nameTagObject;

        [Header("Priority")]
        [Tooltip("Selection priority. Set higher than nearby objects to always win focus.")]
        [SerializeField] private int priority = 5;

        // ──────────────────────────────────────────────
        // Private fields
        // ──────────────────────────────────────────────

        private bool isTalking = false;
        private int currentDialogueIndex = 0;

        // ──────────────────────────────────────────────
        // Public properties
        // ──────────────────────────────────────────────

        public int Priority => priority;

        // ──────────────────────────────────────────────
        // Unity lifecycle
        // ──────────────────────────────────────────────

        private void Awake()
        {
            if (nameTagObject != null)
            {
                nameTagObject.SetActive(false);
            }
        }

        // ──────────────────────────────────────────────
        // IInteractable_UMFOSS implementation
        // ──────────────────────────────────────────────

        public void Interact(GameObject interactor)
        {
            isTalking = true;

            string line = dialogueLines[currentDialogueIndex];
            Debug.Log($"[InteractionSystem] {npcName}: \"{line}\"");

            currentDialogueIndex = (currentDialogueIndex + 1) % dialogueLines.Length;

            // Simulate dialogue ending after a short delay
            // In a real game, this would be handled by a DialogueSystem callback
            Invoke(nameof(EndDialogue), 1.5f);
        }

        public void OnFocused(GameObject interactor)
        {
            if (nameTagObject != null)
            {
                nameTagObject.SetActive(true);
            }

            Debug.Log($"[InteractionSystem] NPC name tag shown: {npcName}");
        }

        public void OnUnfocused(GameObject interactor)
        {
            if (nameTagObject != null)
            {
                nameTagObject.SetActive(false);
            }
        }

        public string GetInteractionPrompt()
        {
            return "Press E to talk";
        }

        public bool CanInteract(GameObject interactor)
        {
            return !isTalking;
        }

        // ──────────────────────────────────────────────
        // Private methods
        // ──────────────────────────────────────────────

        private void EndDialogue()
        {
            isTalking = false;
            Debug.Log($"[InteractionSystem] Dialogue with {npcName} ended.");
        }
    }
}
