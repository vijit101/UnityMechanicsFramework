using System.Collections.Generic;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

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
        [Tooltip("The key used to trigger interaction when using legacy input.")]
        [SerializeField] private KeyCode interactKey = KeyCode.E;

        [Tooltip("If true, reads from Unity's new Input System instead of legacy Input.GetKey.")]
        [SerializeField] private bool useInputSystem = false;

        [Tooltip("Input Action name when using the new Input System (e.g. 'Interact').")]
        [SerializeField] private string inputActionName = "Interact";

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

        #if ENABLE_INPUT_SYSTEM
        private InputAction interactAction;
        #endif

        // ──────────────────────────────────────────────
        // Constants
        // ──────────────────────────────────────────────

        private const float HOLD_PROGRESS_MIN = 0f;
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
        /// Creates one if it doesn't exist. Only enabled when detection mode is Trigger.
        /// </summary>
        private void SetupTriggerCollider()
        {
            triggerCollider = GetComponent<CircleCollider2D>();

            if (triggerCollider == null)
            {
                triggerCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            triggerCollider.isTrigger = true;
            triggerCollider.radius = interactionRadius;
            triggerCollider.enabled = (detectionMode == DetectionMode.Trigger);
        }

        /// <summary>
        /// Sets up the Input System action reference if enabled.
        /// Wrapped in preprocessor directives so the project compiles without the Input System package.
        /// </summary>
        private void SetupInputSystem()
        {
            #if ENABLE_INPUT_SYSTEM
            if (useInputSystem && !string.IsNullOrEmpty(inputActionName))
            {
                interactAction = new InputAction(inputActionName, InputActionType.Button);
                interactAction.Enable();
            }
            #endif
        }

        /// <summary>
        /// Detects interactables using Physics2D.OverlapCircleAll.
        /// Runs every frame in Update when OverlapCircle mode is active.
        /// </summary>
        private void DetectWithOverlapCircle()
        {
            detectedInteractables.Clear();

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                transform.position,
                interactionRadius,
                interactableLayer
            );

            foreach (Collider2D hit in hits)
            {
                IInteractable_UMFOSS interactable = hit.GetComponent<IInteractable_UMFOSS>();
                if (interactable != null)
                {
                    detectedInteractables.Add(interactable);
                }
            }
        }

        /// <summary>
        /// Detects interactables using a 2D raycast in the configured direction.
        /// Runs every frame in Update when Raycast mode is active.
        /// </summary>
        private void DetectWithRaycast()
        {
            detectedInteractables.Clear();

            RaycastHit2D[] hits = Physics2D.RaycastAll(
                transform.position,
                raycastDirection.normalized,
                raycastDistance,
                interactableLayer
            );

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null) continue;

                IInteractable_UMFOSS interactable = hit.collider.GetComponent<IInteractable_UMFOSS>();
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

                // Only choose candidates that can actually be interacted with
                if (bestCandidate != null && !bestCandidate.CanInteract(gameObject))
                {
                    // Still allow focus if it's in range, but only for display purposes;
                    // the prompt text will handle showing the right message.
                    // We check CanInteract() again in TryInteract() for the actual gate.
                    // However, per the spec: "CanInteract() checked before showing prompt"
                    // So we skip non-interactable candidates.
                    bestCandidate = FindFirstInteractable();
                }
            }

            if (bestCandidate != currentInteractable)
            {
                UpdateFocus(bestCandidate);
            }
        }

        /// <summary>
        /// Finds the nearest interactable by distance to this transform.
        /// </summary>
        private IInteractable_UMFOSS FindNearest()
        {
            IInteractable_UMFOSS nearest = null;
            float closestDistance = float.MaxValue;

            foreach (IInteractable_UMFOSS interactable in detectedInteractables)
            {
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
        /// Finds the interactable with the highest Priority value.
        /// </summary>
        private IInteractable_UMFOSS FindHighestPriority()
        {
            IInteractable_UMFOSS best = null;
            int highestPriority = int.MinValue;

            foreach (IInteractable_UMFOSS interactable in detectedInteractables)
            {
                if (interactable.Priority > highestPriority)
                {
                    highestPriority = interactable.Priority;
                    best = interactable;
                }
            }

            return best;
        }

        /// <summary>
        /// Finds the first interactable in the list that returns CanInteract() == true.
        /// Used as a fallback when the best candidate (by distance/priority) cannot be interacted with.
        /// </summary>
        private IInteractable_UMFOSS FindFirstInteractable()
        {
            foreach (IInteractable_UMFOSS interactable in detectedInteractables)
            {
                if (interactable.CanInteract(gameObject))
                {
                    return interactable;
                }
            }

            return null;
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
                    CancelHold();
                }
            }
            else if (isReleased && isHolding)
            {
                CancelHold();
            }
        }

        /// <summary>
        /// Resets hold state and publishes cancellation event.
        /// </summary>
        private void CancelHold()
        {
            if (isHolding)
            {
                isHolding = false;
                holdTimer = 0f;

                EventBus.Publish(new HoldInteractCancelledEvent());
            }
        }

        // ──────────────────────────────────────────────
        // Private methods — Input reading
        // ──────────────────────────────────────────────

        /// <summary>Returns true on the frame the interact key/button is first pressed.</summary>
        private bool GetInteractPressed()
        {
            #if ENABLE_INPUT_SYSTEM
            if (useInputSystem && interactAction != null)
            {
                return interactAction.WasPressedThisFrame();
            }
            #endif

            return Input.GetKeyDown(interactKey);
        }

        /// <summary>Returns true while the interact key/button is held down.</summary>
        private bool GetInteractHeld()
        {
            #if ENABLE_INPUT_SYSTEM
            if (useInputSystem && interactAction != null)
            {
                return interactAction.IsPressed();
            }
            #endif

            return Input.GetKey(interactKey);
        }

        /// <summary>Returns true on the frame the interact key/button is released.</summary>
        private bool GetInteractReleased()
        {
            #if ENABLE_INPUT_SYSTEM
            if (useInputSystem && interactAction != null)
            {
                return interactAction.WasReleasedThisFrame();
            }
            #endif

            return Input.GetKeyUp(interactKey);
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
