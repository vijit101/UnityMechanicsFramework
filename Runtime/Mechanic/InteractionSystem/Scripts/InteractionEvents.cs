using UnityEngine;

namespace GameplayMechanicsUMFOSS.Interaction
{
    /// <summary>
    /// Fired when focus shifts to a new interactable.
    /// InteractionPrompt_UMFOSS subscribes — shows prompt with promptText.
    /// </summary>
    public struct InteractableDetectedEvent
    {
        /// <summary>The interactable that is now in focus.</summary>
        public IInteractable_UMFOSS interactable;

        /// <summary>The prompt text to display (e.g. "Press E to open").</summary>
        public string promptText;
    }

    /// <summary>
    /// Fired when the focused interactable leaves range or becomes invalid.
    /// InteractionPrompt_UMFOSS subscribes — hides prompt.
    /// </summary>
    public struct InteractableLostEvent
    {
        /// <summary>The interactable that was previously focused.</summary>
        public IInteractable_UMFOSS interactable;
    }

    /// <summary>
    /// Fired after Interact() is called successfully on an interactable.
    /// AudioManager, AchievementSystem, or any other system can subscribe.
    /// </summary>
    public struct InteractionPerformedEvent
    {
        /// <summary>The interactable that was interacted with.</summary>
        public IInteractable_UMFOSS interactable;

        /// <summary>The GameObject that performed the interaction.</summary>
        public GameObject interactor;
    }

    /// <summary>
    /// Fired when TryInteract() is called but CanInteract() returns false.
    /// UI systems can subscribe to display failure messages like "Requires Iron Key".
    /// </summary>
    public struct InteractionFailedEvent
    {
        /// <summary>The interactable that rejected the interaction.</summary>
        public IInteractable_UMFOSS interactable;

        /// <summary>A human-readable reason for the failure.</summary>
        public string reason;
    }

    /// <summary>
    /// Fired every frame during a hold interaction. Value ranges from 0.0 to 1.0.
    /// UI progress bar subscribes to show fill amount.
    /// </summary>
    public struct HoldInteractProgressEvent
    {
        /// <summary>Current hold progress normalized between 0 and 1.</summary>
        public float progress;
    }

    /// <summary>
    /// Fired when hold is released before completion or when the player
    /// moves out of range mid-hold.
    /// </summary>
    public struct HoldInteractCancelledEvent { }
}
