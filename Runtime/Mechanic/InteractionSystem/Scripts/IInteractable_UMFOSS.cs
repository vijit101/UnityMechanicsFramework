using UnityEngine;

namespace GameplayMechanicsUMFOSS.Interaction
{
    /// <summary>
    /// The contract for any object that can be interacted with.
    /// Implement this interface on any MonoBehaviour to make it detectable
    /// and usable by <see cref="InteractionController_UMFOSS"/>.
    ///
    /// The interactable decides what happens — the controller only knows
    /// the interface, never the concrete type. This is what makes the
    /// system infinitely extensible with zero changes to the core.
    /// </summary>
    public interface IInteractable_UMFOSS
    {
        /// <summary>
        /// Called when the interactor successfully interacts with this object.
        /// The object decides entirely what happens — open, talk, pick up, unlock.
        /// </summary>
        /// <param name="interactor">The GameObject performing the interaction (e.g. the player).</param>
        void Interact(GameObject interactor);

        /// <summary>
        /// Called when this object enters the interactor's focus (best candidate in range).
        /// Use for: highlight outline, tooltip, hover animation.
        /// </summary>
        /// <param name="interactor">The GameObject whose focus this object has entered.</param>
        void OnFocused(GameObject interactor);

        /// <summary>
        /// Called when the interactor's focus leaves this object without interacting.
        /// Use for: remove highlight, hide tooltip.
        /// </summary>
        /// <param name="interactor">The GameObject whose focus this object has left.</param>
        void OnUnfocused(GameObject interactor);

        /// <summary>
        /// The text shown in the interaction prompt.
        /// Examples: "Press E to open", "Press E to talk", "Hold E to activate".
        /// </summary>
        /// <returns>A human-readable prompt string.</returns>
        string GetInteractionPrompt();

        /// <summary>
        /// Whether this object can currently be interacted with.
        /// Return false to suppress the prompt entirely.
        /// Examples of returning false:
        ///   — chest already opened
        ///   — door requires key player does not have
        ///   — NPC already in dialogue
        /// </summary>
        /// <param name="interactor">The GameObject attempting to interact.</param>
        /// <returns>True if interaction is allowed; false otherwise.</returns>
        bool CanInteract(GameObject interactor);

        /// <summary>
        /// Priority value used by the HighestPriority selection mode.
        /// When multiple interactables are in range, the one with the highest
        /// priority is focused regardless of distance.
        /// Default implementations should return 0.
        /// </summary>
        int Priority { get; }
    }
}
