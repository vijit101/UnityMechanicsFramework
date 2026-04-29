using System.Collections.Generic;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Interaction
{
    /// <summary>
    /// Enumerates the detection strategies available for finding interactables.
    /// </summary>
    public enum DetectionMode
    {
        /// <summary>Uses OnTriggerEnter2D/OnTriggerExit2D. Requires a CircleCollider2D set to trigger.</summary>
        Trigger,
        /// <summary>Uses Physics2D.OverlapCircleAll each frame. No collider required.</summary>
        OverlapCircle,
        /// <summary>Uses a 2D raycast in the facing direction. Line-of-sight detection.</summary>
        Raycast
    }

    /// <summary>
    /// Enumerates how the best interactable is chosen when multiple are in range.
    /// </summary>
    public enum SelectionMode
    {
        /// <summary>Focus the interactable closest to the controller's position.</summary>
        Nearest,
        /// <summary>Focus the interactable with the highest Priority value.</summary>
        HighestPriority
    }

    /// <summary>
    /// The central interaction controller. Attach one per entity (player or AI).
    /// Detects nearby interactables, selects the best candidate, handles input,
    /// manages hold-to-interact, and publishes all events via EventBus.
    ///
    /// The controller never knows what it is interacting with — it only knows
    /// the <see cref="IInteractable_UMFOSS"/> interface. The object decides what happens.
    /// </summary>
    [DisallowMultipleComponent]
    public class InteractionController_UMFOSS : MonoBehaviour
    {
        // ──────────────────────────────────────────────
        // Serialized fields — Inspector configuration
        // ──────────────────────────────────────────────

        [Header("Detection")]
        [Tooltip("How interactables are found: Trigger (collider events), OverlapCircle (physics query), or Raycast (line-of-sight).")]
        [SerializeField] private DetectionMode detectionMode = DetectionMode.OverlapCircle;

        [Tooltip("Radius within which interactables are detected (used by OverlapCircle and Trigger collider size).")]
        [SerializeField] private float interactionRadius = 2.5f;

        [Tooltip("Only objects on this layer are considered for interaction.")]
        [SerializeField] private LayerMask interactableLayer;

        [Header("Selection")]
        [Tooltip("Nearest = closest by distance. HighestPriority = highest IInteractable_UMFOSS.Priority value.")]
        [SerializeField] private SelectionMode selectionMode = SelectionMode.Nearest;

        [Header("Interaction")]
        [Tooltip("If true, the player must hold the interact key for HoldDuration seconds instead of a single press.")]
        [SerializeField] private bool requireHold = false;

        [Tooltip("Seconds required for held interaction. Only active when Require Hold is true.")]
        [SerializeField] private float holdDuration = 1.5f;

        [Header("Input")]
        [Tooltip("The keyboard key used to trigger interaction.")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Tooltip("Gamepad button name from Unity's Input Manager (e.g. 'Submit' maps to the A/Cross button on most gamepads). Leave empty to disable gamepad support.")]
        [SerializeField] private string gamepadButton = "Submit";

        [Header("Raycast Settings")]
        [Tooltip("The direction the raycast fires in (used only in Raycast detection mode). Defaults to right.")]
        [SerializeField] private Vector2 raycastDirection = Vector2.right;

        [Tooltip("Length of the interaction raycast.")]
        [SerializeField] private float raycastDistance = 2.0f;

        // ──────────────────────────────────────────────
        // Private fields
        // ──────────────────────────────────────────────

        private readonly List<IInteractable_UMFOSS> detectedInteractables = new List<IInteractable_UMFOSS>();
        private IInteractable_UMFOSS currentInteractable;
        private float holdTimer;
        private bool isHolding;
        private CircleCollider2D triggerCollider;

        // Pre-allocated arrays for NonAlloc physics queries — avoids heap allocation every frame.
        private readonly Collider2D[] overlapResults = new Collider2D[MAX_OVERLAP_RESULTS];
        private readonly RaycastHit2D[] raycastResults = new RaycastHit2D[MAX_OVERLAP_RESULTS];

        // ──────────────────────────────────────────────
        // Constants
        // ──────────────────────────────────────────────

        private const float HOLD_PROGRESS_MAX = 1f;
        private const int MAX_OVERLAP_RESULTS = 20;

        // ──────────────────────────────────────────────
        // Public properties
        // ──────────────────────────────────────────────

        /// <summary>Current detection mode. Can be changed at runtime.</summary>
        public DetectionMode CurrentDetectionMode => detectionMode;

        // ──────────────────────────────────────────────
        // Unity lifecycle methods
        // ──────────────────────────────────────────────

        private void Awake()
        {
            SetupTriggerCollider();
            SetupInputSystem();
        }

        private void Update()
        {
            if (detectionMode == DetectionMode.OverlapCircle)
            {
                DetectWithOverlapCircle();
            }
            else if (detectionMode == DetectionMode.Raycast)
            {
                DetectWithRaycast();
            }

            SelectBestInteractable();
            HandleInput();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (detectionMode != DetectionMode.Trigger) return;

            if (!IsOnInteractableLayer(other.gameObject)) return;

            IInteractable_UMFOSS interactable = other.GetComponent<IInteractable_UMFOSS>();
            if (interactable != null && !detectedInteractables.Contains(interactable))
            {
                detectedInteractables.Add(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (detectionMode != DetectionMode.Trigger) return;

            IInteractable_UMFOSS interactable = other.GetComponent<IInteractable_UMFOSS>();
            if (interactable != null)
            {
                RemoveInteractable(interactable);
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize interaction radius in the editor
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, interactionRadius);

            if (detectionMode == DetectionMode.Raycast)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(transform.position, raycastDirection.normalized * raycastDistance);
            }
        }

        // ──────────────────────────────────────────────
        // Public API
        // ──────────────────────────────────────────────

        /// <summary>
        /// Attempts to interact with the current focused interactable.
        /// Checks CanInteract() before calling Interact().
        /// Does nothing if no interactable is in focus.
        /// </summary>
        public void TryInteract()
        {
            if (currentInteractable == null) return;

            // Check if the interactable is still a valid Unity object
            MonoBehaviour interactableMono = currentInteractable as MonoBehaviour;
            if (interactableMono == null || interactableMono.gameObject == null) return;

            if (!currentInteractable.CanInteract(gameObject))
            {
                EventBus.Publish(new InteractionFailedEvent
                {
                    interactable = currentInteractable,
                    reason = "Cannot interact right now."
                });
                return;
            }

            currentInteractable.Interact(gameObject);

            EventBus.Publish(new InteractionPerformedEvent
            {
                interactable = currentInteractable,
                interactor = gameObject
            });

            // Re-evaluate: the interactable may no longer be valid after Interact()
            // (e.g. a pickup that deactivates itself)
            RefreshCurrentInteractable();
        }

        /// <summary>
        /// Returns the currently focused interactable.
        /// Returns null if nothing is in range or focused.
        /// </summary>
        /// <returns>The current IInteractable_UMFOSS or null.</returns>
        public IInteractable_UMFOSS GetCurrentInteractable()
        {
            return currentInteractable;
        }

        /// <summary>
        /// Updates the detection radius at runtime.
        /// Useful for abilities that extend reach, or crouching that reduces it.
        /// Also updates the trigger collider radius if using Trigger mode.
        /// </summary>
        /// <param name="value">New interaction radius in world units.</param>
        public void SetInteractionRadius(float value)
        {
            interactionRadius = Mathf.Max(0f, value);

            if (triggerCollider != null)
            {
                triggerCollider.radius = interactionRadius;
            }
        }

        /// <summary>
        /// Returns true if at least one valid interactable is detected in range.
        /// Use for UI — grey out interact button when nothing nearby.
        /// </summary>
        /// <returns>True if any interactable is in detection range.</returns>
        public bool HasInteractableInRange()
        {
            return detectedInteractables.Count > 0;
        }

        /// <summary>
        /// Changes the detection mode at runtime.
        /// Clears the current detection list and resets the trigger collider state.
        /// </summary>
        /// <param name="mode">The new DetectionMode to use.</param>
        public void SetDetectionMode(DetectionMode mode)
        {
            // Clear existing state
            ClearFocus();
            detectedInteractables.Clear();

            detectionMode = mode;

            // Enable/disable the trigger collider based on mode
            if (triggerCollider != null)
            {
                triggerCollider.enabled = (detectionMode == DetectionMode.Trigger);
            }
        }

        // ──────────────────────────────────────────────
        // Private methods — Detection
        // ──────────────────────────────────────────────

        /// <summary>
        /// Sets up the CircleCollider2D for Trigger detection mode.
        /// Always adds a fresh dedicated collider — never grabs an existing one,
        /// which could accidentally convert the player's physics collider to a trigger.
        /// Only enabled when detection mode is Trigger.
        /// </summary>
        private void SetupTriggerCollider()
        {
            // Always AddComponent — never GetComponent — to avoid hijacking
            // an existing CircleCollider2D that the player uses for physics movement.
            triggerCollider = gameObject.AddComponent<CircleCollider2D>();
            triggerCollider.isTrigger = true;
            triggerCollider.radius = interactionRadius;
            triggerCollider.enabled = (detectionMode == DetectionMode.Trigger);
        }

        /// <summary>
        /// No additional input setup needed. This system uses Unity's legacy Input Manager
        /// which supports both keyboard (KeyCode) and gamepad (Input.GetButton) out of the box.
        /// Keyboard: configured via Interact Key (default E).
        /// Gamepad:  configured via Gamepad Button (default "Submit" = A/Cross button).
        /// No extra packages required.
        /// </summary>
        private void SetupInputSystem()
        {
            // Nothing to initialize — legacy Input works without setup.
        }

        /// <summary>
        /// Detects interactables using Physics2D.OverlapCircleNonAlloc.
        /// NonAlloc writes results into the pre-allocated overlapResults array
        /// instead of allocating a new array each frame — zero GC pressure.
        /// Runs every frame in Update when OverlapCircle mode is active.
        /// </summary>
        private void DetectWithOverlapCircle()
        {
            detectedInteractables.Clear();

            int count = Physics2D.OverlapCircleNonAlloc(
                transform.position,
                interactionRadius,
                overlapResults,
                interactableLayer
            );

            for (int i = 0; i < count; i++)
            {
                IInteractable_UMFOSS interactable = overlapResults[i].GetComponent<IInteractable_UMFOSS>();
                if (interactable != null)
                {
                    detectedInteractables.Add(interactable);
                }
            }
        }

        /// <summary>
        /// Detects interactables using Physics2D.RaycastNonAlloc.
        /// NonAlloc writes results into the pre-allocated raycastResults array
        /// instead of allocating a new array each frame — zero GC pressure.
        /// Runs every frame in Update when Raycast mode is active.
        /// </summary>
        private void DetectWithRaycast()
        {
            detectedInteractables.Clear();

            int count = Physics2D.RaycastNonAlloc(
                transform.position,
                raycastDirection.normalized,
                raycastResults,
                raycastDistance,
                interactableLayer
            );

            for (int i = 0; i < count; i++)
            {
                if (raycastResults[i].collider == null) continue;

                IInteractable_UMFOSS interactable = raycastResults[i].collider.GetComponent<IInteractable_UMFOSS>();
                if (interactable != null)
                {
                    detectedInteractables.Add(interactable);
                }
            }
        }

        // ──────────────────────────────────────────────
        // Private methods — Selection
        // ──────────────────────────────────────────────

        /// <summary>
        /// From the detected list, selects the best interactable based on
        /// the configured SelectionMode and updates focus events accordingly.
        /// CanInteract() filtering happens inside FindNearest/FindHighestPriority
        /// so the "best valid" candidate is always returned — never a random fallback.
        /// </summary>
        private void SelectBestInteractable()
        {
            // Remove any invalid entries
            CleanupDetectedList();

            IInteractable_UMFOSS bestCandidate = null;

            if (detectedInteractables.Count > 0)
            {
                bestCandidate = (selectionMode == SelectionMode.Nearest)
                    ? FindNearest()
                    : FindHighestPriority();
            }

            if (bestCandidate != currentInteractable)
            {
                UpdateFocus(bestCandidate);
            }
        }

        /// <summary>
        /// Finds the nearest interactable that can currently be interacted with.
        /// CanInteract() is checked here so that a locked/used object never steals
        /// focus from a valid one farther away — the fallback is distance-correct.
        /// </summary>
        private IInteractable_UMFOSS FindNearest()
        {
            IInteractable_UMFOSS nearest = null;
            float closestDistance = float.MaxValue;

            foreach (IInteractable_UMFOSS interactable in detectedInteractables)
            {
                // Only consider objects that can currently be interacted with.
                // This is the first CanInteract() check — gates the focus prompt.
                if (!interactable.CanInteract(gameObject)) continue;

                MonoBehaviour mono = interactable as MonoBehaviour;
                if (mono == null) continue;

                float distance = Vector2.Distance(transform.position, mono.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    nearest = interactable;
                }
            }

            return nearest;
        }

        /// <summary>
        /// Finds the interactable with the highest Priority value
        /// among those that can currently be interacted with.
        /// </summary>
        private IInteractable_UMFOSS FindHighestPriority()
        {
            IInteractable_UMFOSS best = null;
            int highestPriority = int.MinValue;

            foreach (IInteractable_UMFOSS interactable in detectedInteractables)
            {
                // Only consider objects that can currently be interacted with.
                if (!interactable.CanInteract(gameObject)) continue;

                if (interactable.Priority > highestPriority)
                {
                    highestPriority = interactable.Priority;
                    best = interactable;
                }
            }

            return best;
        }

        // ──────────────────────────────────────────────
        // Private methods — Focus management
        // ──────────────────────────────────────────────

        /// <summary>
        /// Switches focus from the current interactable to a new one.
        /// Fires OnUnfocused/OnFocused callbacks and publishes EventBus events.
        /// </summary>
        private void UpdateFocus(IInteractable_UMFOSS newTarget)
        {
            // Lose focus on the previous target
            if (currentInteractable != null)
            {
                ClearFocus();
            }

            currentInteractable = newTarget;

            // Gain focus on the new target
            if (currentInteractable != null)
            {
                currentInteractable.OnFocused(gameObject);

                EventBus.Publish(new InteractableDetectedEvent
                {
                    interactable = currentInteractable,
                    promptText = currentInteractable.GetInteractionPrompt()
                });
            }
        }

        /// <summary>
        /// Clears focus from the current interactable, fires unfocused callback,
        /// publishes InteractableLostEvent, and resets hold state.
        /// </summary>
        private void ClearFocus()
        {
            if (currentInteractable == null) return;

            currentInteractable.OnUnfocused(gameObject);

            EventBus.Publish(new InteractableLostEvent
            {
                interactable = currentInteractable
            });

            // Cancel any in-progress hold
            CancelHold();

            currentInteractable = null;
        }

        /// <summary>
        /// Re-evaluates the current interactable after an interaction.
        /// If it is no longer valid (e.g. deactivated), clears focus.
        /// </summary>
        private void RefreshCurrentInteractable()
        {
            if (currentInteractable == null) return;

            MonoBehaviour mono = currentInteractable as MonoBehaviour;
            if (mono == null || !mono.gameObject.activeInHierarchy)
            {
                detectedInteractables.Remove(currentInteractable);
                ClearFocus();
            }
        }

        // ──────────────────────────────────────────────
        // Private methods — Input and hold interaction
        // ──────────────────────────────────────────────

        /// <summary>
        /// Reads input from legacy or new Input System and handles
        /// instant or hold-to-interact behavior.
        /// </summary>
        private void HandleInput()
        {
            if (currentInteractable == null) return;

            bool isPressed = GetInteractPressed();
            bool isReleased = GetInteractReleased();
            bool isHeld = GetInteractHeld();

            if (requireHold)
            {
                HandleHoldInteraction(isHeld, isReleased);
            }
            else
            {
                if (isPressed)
                {
                    TryInteract();
                }
            }
        }

        /// <summary>
        /// Manages the hold-to-interact logic: accumulates hold time,
        /// publishes progress events, fires on completion, cancels on release.
        /// </summary>
        private void HandleHoldInteraction(bool isHeld, bool isReleased)
        {
            if (isHeld && currentInteractable != null)
            {
                if (!isHolding)
                {
                    // Start holding
                    isHolding = true;
                    holdTimer = 0f;
                }

                holdTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(holdTimer / holdDuration);

                EventBus.Publish(new HoldInteractProgressEvent
                {
                    progress = progress
                });

                if (progress >= HOLD_PROGRESS_MAX)
                {
                    TryInteract();
                    // Use ResetHold (not CancelHold) — the hold completed successfully,
                    // so we must NOT fire HoldInteractCancelledEvent.
                    ResetHold();
                }
            }
            else if (isReleased && isHolding)
            {
                CancelHold();
            }
        }

        /// <summary>
        /// Resets hold state silently — used when a hold completes successfully.
        /// Does NOT publish HoldInteractCancelledEvent.
        /// </summary>
        private void ResetHold()
        {
            isHolding = false;
            holdTimer = 0f;
        }

        /// <summary>
        /// Resets hold state and publishes HoldInteractCancelledEvent.
        /// Called when the player releases the key early or leaves range mid-hold.
        /// </summary>
        private void CancelHold()
        {
            if (isHolding)
            {
                ResetHold();
                EventBus.Publish(new HoldInteractCancelledEvent());
            }
        }

        // ──────────────────────────────────────────────
        // Private methods — Input reading
        // ──────────────────────────────────────────────

        /// <summary>
        /// Returns true on the frame the interact key is first pressed (keyboard)
        /// OR the gamepad button is first pressed.
        /// Unity's Input Manager maps "Submit" to the A/Cross button by default.
        /// </summary>
        private bool GetInteractPressed()
        {
            if (Input.GetKeyDown(interactKey)) return true;
            return IsGamepadButtonDown();
        }

        /// <summary>
        /// Returns true while the interact key is held (keyboard)
        /// OR the gamepad button is held.
        /// </summary>
        private bool GetInteractHeld()
        {
            if (Input.GetKey(interactKey)) return true;
            return IsGamepadButtonHeld();
        }

        /// <summary>
        /// Returns true on the frame the interact key is released (keyboard)
        /// OR the gamepad button is released.
        /// </summary>
        private bool GetInteractReleased()
        {
            if (Input.GetKeyUp(interactKey)) return true;
            return IsGamepadButtonReleased();
        }

        /// <summary>
        /// Checks if the configured gamepad button was pressed this frame.
        /// Uses try-catch because Input.GetButtonDown throws ArgumentException
        /// if the button name is not defined in the Input Manager.
        /// </summary>
        private bool IsGamepadButtonDown()
        {
            if (string.IsNullOrEmpty(gamepadButton)) return false;
            try { return Input.GetButtonDown(gamepadButton); }
            catch { return false; }
        }

        /// <summary>Checks if the configured gamepad button is held this frame.</summary>
        private bool IsGamepadButtonHeld()
        {
            if (string.IsNullOrEmpty(gamepadButton)) return false;
            try { return Input.GetButton(gamepadButton); }
            catch { return false; }
        }

        /// <summary>Checks if the configured gamepad button was released this frame.</summary>
        private bool IsGamepadButtonReleased()
        {
            if (string.IsNullOrEmpty(gamepadButton)) return false;
            try { return Input.GetButtonUp(gamepadButton); }
            catch { return false; }
        }

        // ──────────────────────────────────────────────
        // Private methods — Helpers
        // ──────────────────────────────────────────────

        /// <summary>
        /// Checks if a GameObject is on the configured interactable layer.
        /// </summary>
        private bool IsOnInteractableLayer(GameObject obj)
        {
            return ((1 << obj.layer) & interactableLayer) != 0;
        }

        /// <summary>
        /// Removes destroyed or deactivated interactables from the detected list.
        /// </summary>
        private void CleanupDetectedList()
        {
            for (int i = detectedInteractables.Count - 1; i >= 0; i--)
            {
                MonoBehaviour mono = detectedInteractables[i] as MonoBehaviour;
                if (mono == null || !mono.gameObject.activeInHierarchy)
                {
                    if (detectedInteractables[i] == currentInteractable)
                    {
                        ClearFocus();
                    }
                    detectedInteractables.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Removes a specific interactable from the detected list and clears focus if needed.
        /// </summary>
        private void RemoveInteractable(IInteractable_UMFOSS interactable)
        {
            if (interactable == currentInteractable)
            {
                ClearFocus();
            }

            detectedInteractables.Remove(interactable);
        }
    }
}
