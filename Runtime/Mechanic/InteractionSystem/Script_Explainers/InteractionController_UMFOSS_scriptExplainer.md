# InteractionController_UMFOSS — Line-by-Line Script Explainer

```
SCRIPT     : InteractionController_UMFOSS.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Interaction
```

---

## Purpose

The brain of the entire interaction system. Attached **once** per entity (player or AI). It detects nearby interactables, picks the best candidate, reads the input, manages hold-to-interact, and publishes every relevant event over `EventBus`. The controller never knows the concrete type of any interactable — it sees only the `IInteractable_UMFOSS` interface, which is what makes the whole system extensible without modification.

---

## Imports & Namespace

```csharp
using System.Collections.Generic;
```
**Explanation:** Required for `List<IInteractable_UMFOSS>` — the buffer of currently detected interactables.

```csharp
using UnityEngine;
```
**Explanation:** Core Unity types — `MonoBehaviour`, `Vector2`, `Physics2D`, `Collider2D`, `RaycastHit2D`, `LayerMask`, `KeyCode`.

```csharp
using GameplayMechanicsUMFOSS.Core;
```
**Explanation:** Imports the static `EventBus` so the controller can publish detection, focus, performed, failed, and hold events.

```csharp
namespace GameplayMechanicsUMFOSS.Interaction
```
**Explanation:** Same feature group as the interface and event structs.

---

## Enum — DetectionMode

```csharp
public enum DetectionMode
{
    Trigger,
    OverlapCircle,
    Raycast
}
```
**Explanation:** Three orthogonal strategies for finding interactables. `Trigger` uses `OnTriggerEnter2D`/`OnTriggerExit2D` callbacks (cheapest, event-driven). `OverlapCircle` runs a `Physics2D.OverlapCircleNonAlloc` every frame (most flexible — survives teleports). `Raycast` fires a 2D ray in `raycastDirection` for line-of-sight detection (best for first-person feel).

---

## Enum — SelectionMode

```csharp
public enum SelectionMode
{
    Nearest,
    HighestPriority
}
```
**Explanation:** When several interactables are in range at once, this picks the winner. `Nearest` is purely distance-based — the closest valid object wins. `HighestPriority` ignores distance and picks the object whose `Priority` integer is largest, which is essential when a quest-critical NPC must always beat ambient props nearby.

---

## Class Declaration

```csharp
[DisallowMultipleComponent]
public class InteractionController_UMFOSS : MonoBehaviour
```
**Explanation:** `[DisallowMultipleComponent]` means a single GameObject can never have two `InteractionController_UMFOSS` scripts — Unity blocks the second `Add Component` at the editor level. Enforces the "one controller per entity" rule at the engine layer rather than runtime.

---

## Serialized Fields — Detection

```csharp
[Header("Detection")]
[SerializeField] private DetectionMode detectionMode = DetectionMode.OverlapCircle;
```
**Explanation:** Default is `OverlapCircle` because it's the most forgiving for prototyping — works without any collider on the player and finds objects regardless of how they entered the radius.

```csharp
[SerializeField] private float interactionRadius = 2.5f;
```
**Explanation:** The detection radius in world units. Used by both `OverlapCircle` (as the query radius) and `Trigger` (as the auto-added `CircleCollider2D.radius`). 2.5 is a comfortable "arm's reach" for top-down games.

```csharp
[SerializeField] private LayerMask interactableLayer;
```
**Explanation:** Only objects on layers in this mask are considered. Bitmask, not a tag string — checked at the collider level so non-interactable objects never even reach `GetComponent<IInteractable_UMFOSS>()`. Inspector picker shows checkboxes per layer.

---

## Serialized Fields — Selection

```csharp
[Header("Selection")]
[SerializeField] private SelectionMode selectionMode = SelectionMode.Nearest;
```
**Explanation:** Default `Nearest` because it matches what most players expect — the thing right in front of you wins.

---

## Serialized Fields — Interaction

```csharp
[Header("Interaction")]
[SerializeField] private bool requireHold = false;
```
**Explanation:** When true, the player must hold the key for `holdDuration` seconds; when false, a single press fires immediately. This is a global setting per controller — for a mixed scene with both instant and hold targets, switch the flag at runtime when needed.

```csharp
[SerializeField] private float holdDuration = 1.5f;
```
**Explanation:** Seconds required for a hold to complete. Only consulted when `requireHold` is true. 1.5s feels intentional without being tedious.

---

## Serialized Fields — Input

```csharp
[Header("Input")]
[SerializeField] private KeyCode interactKey = KeyCode.E;
```
**Explanation:** The keyboard key. `KeyCode.E` is the de-facto industry standard for "interact" in PC games since Half-Life popularized it.

```csharp
[SerializeField] private string gamepadButton = "Submit";
```
**Explanation:** Name of an Input Manager button for gamepad support. Unity maps `"Submit"` to the A/Cross button on most pads by default. Setting this to an empty string disables gamepad polling entirely.

---

## Serialized Fields — Raycast

```csharp
[Header("Raycast Settings")]
[SerializeField] private Vector2 raycastDirection = Vector2.right;
```
**Explanation:** Direction the line-of-sight ray fires in. Only used by `DetectionMode.Raycast`. Defaults to `right` so a fresh setup with a right-facing player works out of the box. For a character that turns, drive this from your movement controller each frame.

```csharp
[SerializeField] private float raycastDistance = 2.0f;
```
**Explanation:** Length of the ray. Independent from `interactionRadius` because Raycast mode doesn't use a circle.

---

## Private Fields — State

```csharp
private readonly List<IInteractable_UMFOSS> detectedInteractables = new List<IInteractable_UMFOSS>();
```
**Explanation:** The buffer of every interactable currently in detection range. `readonly` so the reference can't be reassigned — only its contents change. Cleared and rebuilt every frame in `OverlapCircle`/`Raycast` modes; mutated incrementally via collider callbacks in `Trigger` mode.

```csharp
private IInteractable_UMFOSS currentInteractable;
```
**Explanation:** The currently focused target — at most one. Compared by reference equality each frame to detect focus changes, which is what drives `OnFocused`/`OnUnfocused` callbacks.

```csharp
private float holdTimer;
private bool isHolding;
```
**Explanation:** Hold-state machine. `holdTimer` accumulates `Time.deltaTime` while the key is down; `isHolding` tracks whether a hold is in progress. Both reset on completion or cancel.

```csharp
private CircleCollider2D triggerCollider;
```
**Explanation:** A reference to the `CircleCollider2D` auto-added in `Awake` for `Trigger` mode. Stored so `SetInteractionRadius` and `SetDetectionMode` can adjust its radius and `enabled` state at runtime.

---

## Private Fields — NonAlloc Buffers

```csharp
private readonly Collider2D[] overlapResults = new Collider2D[MAX_OVERLAP_RESULTS];
private readonly RaycastHit2D[] raycastResults = new RaycastHit2D[MAX_OVERLAP_RESULTS];
```
**Explanation:** Pre-allocated arrays for the `*NonAlloc` physics queries. Without these, Unity allocates a fresh array every call → 60 heap allocations per second per controller → garbage collection spikes. Reusing the same buffer means **zero GC pressure** in the detection path.

---

## Constants

```csharp
private const float HOLD_PROGRESS_MAX = 1f;
private const int MAX_OVERLAP_RESULTS = 20;
```
**Explanation:** Two named constants instead of magic numbers. `HOLD_PROGRESS_MAX` is the threshold at which a hold completes (always 1 because progress is normalized). `MAX_OVERLAP_RESULTS = 20` is the buffer size — interactions with more than 20 colliders simultaneously is unlikely in practice but won't crash, just truncate.

---

## Public Property — CurrentDetectionMode

```csharp
public DetectionMode CurrentDetectionMode => detectionMode;
```
**Explanation:** Expression-bodied read-only property exposing the current mode for UI labels and the demo mode-switcher script. The mode itself is mutated via `SetDetectionMode`, not the setter, so external scripts can't bypass the cleanup logic.

---

## Unity Lifecycle — Awake

```csharp
private void Awake()
{
    SetupTriggerCollider();
    SetupInputSystem();
}
```
**Explanation:** Two initialization steps. `SetupTriggerCollider` adds the `CircleCollider2D` for `Trigger` mode. `SetupInputSystem` is currently a no-op — kept as a hook for future Input System support without changing the lifecycle shape.

---

## Unity Lifecycle — Update

```csharp
private void Update()
{
    if (detectionMode == DetectionMode.OverlapCircle)
        DetectWithOverlapCircle();
    else if (detectionMode == DetectionMode.Raycast)
        DetectWithRaycast();

    SelectBestInteractable();
    HandleInput();
}
```
**Explanation:** The frame loop in three steps. (1) Detect — only run for query-based modes; `Trigger` mode populates the list via collider callbacks instead. (2) Select — pick the best candidate from the detected list. (3) Handle input — read the interact key and either fire instantly or accumulate hold progress.

---

## Unity Lifecycle — OnTriggerEnter2D

```csharp
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
```
**Explanation:** Trigger-mode entry callback. Three guards: (1) skip if the controller isn't in `Trigger` mode, (2) skip if the entering object isn't on the interactable layer, (3) skip if the object doesn't implement the interface or is already in the list. Duplicate suppression via `Contains` prevents the same interactable being added twice if it has multiple colliders.

---

## Unity Lifecycle — OnTriggerExit2D

```csharp
private void OnTriggerExit2D(Collider2D other)
{
    if (detectionMode != DetectionMode.Trigger) return;

    IInteractable_UMFOSS interactable = other.GetComponent<IInteractable_UMFOSS>();
    if (interactable != null) RemoveInteractable(interactable);
}
```
**Explanation:** Trigger-mode exit callback. No layer check needed — the object was on the layer when it entered (otherwise it wouldn't be in the list), and `RemoveInteractable` is a no-op if the entry isn't there.

---

## Unity Lifecycle — OnDrawGizmosSelected

```csharp
private void OnDrawGizmosSelected()
{
    Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
    Gizmos.DrawWireSphere(transform.position, interactionRadius);

    if (detectionMode == DetectionMode.Raycast)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, raycastDirection.normalized * raycastDistance);
    }
}
```
**Explanation:** Editor-only visualisation. Always draws the interaction radius as a translucent green wire sphere. In `Raycast` mode, also draws a yellow ray showing the direction and distance — invaluable for tuning `raycastDirection` without entering Play Mode.

---

## Public API — TryInteract

```csharp
public void TryInteract()
{
    if (currentInteractable == null) return;

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

    RefreshCurrentInteractable();
}
```
**Explanation:** The single public entry point for performing an interaction. Five gates in order: (1) something is focused, (2) the focus is still a valid Unity object (not destroyed), (3) `CanInteract` returns true (otherwise publish `InteractionFailedEvent` and return), (4) call `Interact()` on the target, (5) publish `InteractionPerformedEvent` for any listening system. Finally `RefreshCurrentInteractable` re-checks the focus, because pickups commonly deactivate themselves inside their own `Interact` call.

The `as MonoBehaviour` + null check guards against Unity's "fake null" — destroyed objects equal `null` even though the C# reference is still set. Calling methods on them throws.

---

## Public API — GetCurrentInteractable

```csharp
public IInteractable_UMFOSS GetCurrentInteractable()
{
    return currentInteractable;
}
```
**Explanation:** Read-only accessor for the focused target. Useful when other scripts want to query — for example, a tutorial popup that wants to know "is the player looking at the door right now?".

---

## Public API — SetInteractionRadius

```csharp
public void SetInteractionRadius(float value)
{
    interactionRadius = Mathf.Max(0f, value);

    if (triggerCollider != null)
    {
        triggerCollider.radius = interactionRadius;
    }
}
```
**Explanation:** Runtime mutator for the radius. `Mathf.Max(0f, value)` clamps negatives to zero — a negative radius would be undefined. Updates the trigger collider too so `Trigger` mode immediately reflects the new size.

---

## Public API — HasInteractableInRange

```csharp
public bool HasInteractableInRange()
{
    return detectedInteractables.Count > 0;
}
```
**Explanation:** Cheap query for UI — grey out the on-screen interact button when the list is empty. Doesn't filter on `CanInteract`; "in range" is the question, "usable" is a separate concept.

---

## Public API — SetDetectionMode

```csharp
public void SetDetectionMode(DetectionMode mode)
{
    ClearFocus();
    detectedInteractables.Clear();

    detectionMode = mode;

    if (triggerCollider != null)
    {
        triggerCollider.enabled = (detectionMode == DetectionMode.Trigger);
    }
}
```
**Explanation:** Runtime mode swap. Three steps in order: (1) clear current focus and fire `InteractableLostEvent`, (2) wipe the detection list (otherwise stale entries from the old mode would persist), (3) flip the trigger collider on/off so `Trigger` mode is the only mode that uses it.

---

## Private — SetupTriggerCollider

```csharp
private void SetupTriggerCollider()
{
    triggerCollider = gameObject.AddComponent<CircleCollider2D>();
    triggerCollider.isTrigger = true;
    triggerCollider.radius = interactionRadius;
    triggerCollider.enabled = (detectionMode == DetectionMode.Trigger);
}
```
**Explanation:** Always `AddComponent` — never `GetComponent`. Why? If the player already has a `CircleCollider2D` for physics movement, `GetComponent` would grab that one and flip its `isTrigger` to true, silently breaking ground detection or physics collisions. Adding a fresh dedicated collider avoids hijacking the player's existing physics setup.

---

## Private — SetupInputSystem

```csharp
private void SetupInputSystem()
{
    // Nothing to initialize — legacy Input works without setup.
}
```
**Explanation:** Empty by design. The legacy `Input` API (`Input.GetKey`, `Input.GetButton`) requires no initialization. Kept as an explicit method so future Input System integration has a natural place to live without restructuring `Awake`.

---

## Private — DetectWithOverlapCircle

```csharp
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
```
**Explanation:** Per-frame circle query. `Physics2D.OverlapCircleNonAlloc` writes hits into the pre-allocated `overlapResults` buffer and returns the count — no garbage. Each hit is checked for the interface; objects on the layer that don't implement `IInteractable_UMFOSS` are silently skipped.

---

## Private — DetectWithRaycast

```csharp
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
```
**Explanation:** Per-frame ray query — same shape as `OverlapCircle` but with `RaycastNonAlloc`. The extra `raycastResults[i].collider == null` check handles a Unity quirk where some rows of the buffer can carry stale or null hits past `count`. `raycastDirection.normalized` ensures the ray length is exactly `raycastDistance` regardless of how the field is set in the inspector.

---

## Private — SelectBestInteractable

```csharp
private void SelectBestInteractable()
{
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
```
**Explanation:** Three responsibilities. (1) Prune destroyed/disabled entries. (2) Choose a winner using the configured strategy. (3) Switch focus only if the winner changed — reference equality means no spurious `OnFocused`/`OnUnfocused` callbacks when the same target stays in focus across frames.

---

## Private — FindNearest

```csharp
private IInteractable_UMFOSS FindNearest()
{
    IInteractable_UMFOSS nearest = null;
    float closestDistance = float.MaxValue;

    foreach (IInteractable_UMFOSS interactable in detectedInteractables)
    {
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
```
**Explanation:** Linear scan with two filters before measuring distance: (1) `CanInteract` must return true — a locked door doesn't steal focus from a usable lever even if it's closer, (2) the interactable must still be a live `MonoBehaviour`. Only after both gates pass do we measure distance. The result is "nearest valid", not "nearest" — a critical distinction.

---

## Private — FindHighestPriority

```csharp
private IInteractable_UMFOSS FindHighestPriority()
{
    IInteractable_UMFOSS best = null;
    int highestPriority = int.MinValue;

    foreach (IInteractable_UMFOSS interactable in detectedInteractables)
    {
        if (!interactable.CanInteract(gameObject)) continue;

        if (interactable.Priority > highestPriority)
        {
            highestPriority = interactable.Priority;
            best = interactable;
        }
    }

    return best;
}
```
**Explanation:** Mirror of `FindNearest`, but ordering by `Priority` instead of distance. Same `CanInteract` gate so the highest-priority object that's actually usable wins. Initialised to `int.MinValue` so even priority `int.MinValue + 1` would win over no candidate at all.

---

## Private — UpdateFocus

```csharp
private void UpdateFocus(IInteractable_UMFOSS newTarget)
{
    if (currentInteractable != null)
    {
        ClearFocus();
    }

    currentInteractable = newTarget;

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
```
**Explanation:** The focus-swap routine. Always unfocuses the old target first (which fires `OnUnfocused` and `InteractableLostEvent`), then sets the new one, then calls `OnFocused` and publishes `InteractableDetectedEvent` with the snapshotted prompt text. The two `null` checks let this same method handle gain-only and loss-only transitions, not just swaps.

---

## Private — ClearFocus

```csharp
private void ClearFocus()
{
    if (currentInteractable == null) return;

    currentInteractable.OnUnfocused(gameObject);

    EventBus.Publish(new InteractableLostEvent
    {
        interactable = currentInteractable
    });

    CancelHold();

    currentInteractable = null;
}
```
**Explanation:** Releases the current focus and tells the world. Calls `OnUnfocused` on the interactable so it can undo its hover state, publishes `InteractableLostEvent` so the UI hides the prompt, cancels any in-progress hold (otherwise leaving range mid-hold would leave the bar at 70% forever), then nulls the reference.

---

## Private — RefreshCurrentInteractable

```csharp
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
```
**Explanation:** Called immediately after a successful `Interact()`. If the interactable deactivated itself inside its own `Interact` call (pickups do this with `SetActive(false)`), this catches it the same frame instead of waiting for the next detection sweep — preventing one frame of "ghost focus" on a dead object.

---

## Private — HandleInput

```csharp
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
```
**Explanation:** Reads three input states up-front (pressed-this-frame, currently-held, released-this-frame) so each helper is called only once per frame. Branches on `requireHold` to either accumulate a hold or fire instantly. Early-exit if there's nothing focused — no input check is needed when there's no target.

---

## Private — HandleHoldInteraction

```csharp
private void HandleHoldInteraction(bool isHeld, bool isReleased)
{
    if (isHeld && currentInteractable != null)
    {
        if (!isHolding)
        {
            isHolding = true;
            holdTimer = 0f;
        }

        holdTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(holdTimer / holdDuration);

        EventBus.Publish(new HoldInteractProgressEvent { progress = progress });

        if (progress >= HOLD_PROGRESS_MAX)
        {
            TryInteract();
            ResetHold();
        }
    }
    else if (isReleased && isHolding)
    {
        CancelHold();
    }
}
```
**Explanation:** The hold state machine. On the frame the hold begins, initialize the timer. Every subsequent frame: accumulate time, normalize against `holdDuration`, publish `HoldInteractProgressEvent`. When progress hits 1.0, fire the interaction and call `ResetHold` (silent — no cancellation event).

The second branch handles early release: only fire `CancelHold` if a hold was actually in progress, otherwise releasing the key normally would spam cancel events.

---

## Private — ResetHold

```csharp
private void ResetHold()
{
    isHolding = false;
    holdTimer = 0f;
}
```
**Explanation:** Silent reset — clears the timer without publishing `HoldInteractCancelledEvent`. Used after a successful hold completion. If `CancelHold` were used instead, the UI would receive a "cancelled" signal one frame after a successful interaction, which would briefly flash a "fail" state.

---

## Private — CancelHold

```csharp
private void CancelHold()
{
    if (isHolding)
    {
        ResetHold();
        EventBus.Publish(new HoldInteractCancelledEvent());
    }
}
```
**Explanation:** Public-facing cancel — resets state and fires the cancelled event. The `if (isHolding)` guard prevents publishing the event when no hold was in progress (e.g., player released a key they never held).

---

## Private — Input Reading

```csharp
private bool GetInteractPressed()
{
    if (Input.GetKeyDown(interactKey)) return true;
    return IsGamepadButtonDown();
}

private bool GetInteractHeld()
{
    if (Input.GetKey(interactKey)) return true;
    return IsGamepadButtonHeld();
}

private bool GetInteractReleased()
{
    if (Input.GetKeyUp(interactKey)) return true;
    return IsGamepadButtonReleased();
}
```
**Explanation:** Three corresponding helpers — keyboard first via `KeyCode`, then gamepad via the named Input Manager button. `GetKeyDown` fires only on the press frame; `GetKey` fires every frame the key is down; `GetKeyUp` fires only on the release frame. This three-way split is what enables both instant-press and hold-to-interact without state hacks.

---

## Private — Gamepad Polling

```csharp
private bool IsGamepadButtonDown()
{
    if (string.IsNullOrEmpty(gamepadButton)) return false;
    try { return Input.GetButtonDown(gamepadButton); }
    catch { return false; }
}
```
**Explanation:** `Input.GetButtonDown` throws `ArgumentException` if the button name isn't defined in the Input Manager (this is a real Unity quirk, not over-defensive coding). The `try/catch` swallows it so projects without "Submit" configured don't crash — gamepad support degrades gracefully to keyboard-only.

```csharp
private bool IsGamepadButtonHeld()
{
    if (string.IsNullOrEmpty(gamepadButton)) return false;
    try { return Input.GetButton(gamepadButton); }
    catch { return false; }
}

private bool IsGamepadButtonReleased()
{
    if (string.IsNullOrEmpty(gamepadButton)) return false;
    try { return Input.GetButtonUp(gamepadButton); }
    catch { return false; }
}
```
**Explanation:** Same defensive shape as `IsGamepadButtonDown`, for the held and released states. The empty-string short-circuit makes "no gamepad" the cheapest path — no exception thrown, no try/catch entered.

---

## Private — IsOnInteractableLayer

```csharp
private bool IsOnInteractableLayer(GameObject obj)
{
    return ((1 << obj.layer) & interactableLayer) != 0;
}
```
**Explanation:** Standard LayerMask check. `obj.layer` is an integer 0-31; `1 << obj.layer` shifts a single bit into that position; bitwise AND against the mask keeps only matching bits; non-zero means "matches at least one selected layer". This is how every layer-filtered Unity API works internally.

---

## Private — CleanupDetectedList

```csharp
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
```
**Explanation:** Reverse iteration so `RemoveAt(i)` doesn't shift indices we haven't visited yet — a classic Unity-list pruning pattern. Removes any entry that's been destroyed or disabled. If the dead entry was the focused target, calls `ClearFocus` first to fire the lost-event.

---

## Private — RemoveInteractable

```csharp
private void RemoveInteractable(IInteractable_UMFOSS interactable)
{
    if (interactable == currentInteractable)
    {
        ClearFocus();
    }

    detectedInteractables.Remove(interactable);
}
```
**Explanation:** Used by `OnTriggerExit2D`. Always clears focus first if the removed item was the focused target — ensures `InteractableLostEvent` fires before the list shrinks.

---

## Why The Architecture Works

Three separate axes of configuration, each independent of the others:

- **Detection** (Trigger / OverlapCircle / Raycast) — *how* the controller finds candidates.
- **Selection** (Nearest / HighestPriority) — *which* candidate wins when multiple are in range.
- **Hold** (instant / hold) — *how* the input fires.

Mixing these covers FPS line-of-sight, top-down proximity, point-and-click, stealth, and adventure-game styles — all from one component.

The controller never references a concrete interactable. Adding `TreasureChest`, `WeaponPickup`, `LightSwitch`, `ConversationTrigger` requires zero changes here. That's the whole point of the interface.
