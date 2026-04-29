# InteractionEvents — Line-by-Line Script Explainer

```
SCRIPT     : InteractionEvents.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Interaction
```

---

## Purpose

Defines the six event payloads published over `EventBus` by `InteractionController_UMFOSS`. Every event is a `struct` (value type) — allocated on the stack with zero garbage-collector pressure, which matters because some of these events fire every single frame during a hold interaction.

---

## Imports & Namespace

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity types — needed because `InteractionPerformedEvent` carries a `GameObject interactor` field.

```csharp
namespace GameplayMechanicsUMFOSS.Interaction
```
**Explanation:** Same feature-group namespace as the rest of the interaction system, so the events live next to the interface and the controller they describe.

---

## Event — InteractableDetectedEvent

```csharp
public struct InteractableDetectedEvent
{
    public IInteractable_UMFOSS interactable;
    public string promptText;
}
```
**Explanation:** Fired by the controller whenever focus shifts to a new interactable. Carries the interactable itself (so listeners that want to do something rich, like show an icon, can query it) and the prompt text snapshotted at focus time. `InteractionPrompt_UMFOSS` subscribes to this and calls `promptPanel.SetActive(true)` plus `promptLabel.text = promptText`.

---

## Event — InteractableLostEvent

```csharp
public struct InteractableLostEvent
{
    public IInteractable_UMFOSS interactable;
}
```
**Explanation:** Fired when focus is cleared — the player walked out of range, the interactable was deactivated, or focus shifted to a different target. Carries only the interactable that was lost; subscribers that just want to hide the prompt UI can ignore the field entirely. The prompt UI uses this — *not* `InteractionPerformedEvent` — to decide when to hide, because hiding on `Performed` causes a one-frame flicker on repeatable interactables that re-focus immediately.

---

## Event — InteractionPerformedEvent

```csharp
public struct InteractionPerformedEvent
{
    public IInteractable_UMFOSS interactable;
    public GameObject interactor;
}
```
**Explanation:** Fired *after* a successful `Interact()` call — meaning `CanInteract()` returned true and the interactable's `Interact()` ran. Carries both ends of the transaction. Audio managers, achievement systems, analytics, and quest trackers all subscribe to this single event without ever knowing the interaction system exists.

---

## Event — InteractionFailedEvent

```csharp
public struct InteractionFailedEvent
{
    public IInteractable_UMFOSS interactable;
    public string reason;
}
```
**Explanation:** Fired when `TryInteract()` was called but `CanInteract()` returned false. The `reason` string lets the interactable explain *why* — "Requires Iron Key", "Already used", "Not enough mana". A floating-text or notification system can subscribe and display the reason on screen. The current controller publishes a generic "Cannot interact right now." reason; richer interactables can override their own pre-checks before reaching here.

---

## Event — HoldInteractProgressEvent

```csharp
public struct HoldInteractProgressEvent
{
    public float progress;
}
```
**Explanation:** Fired every single frame while a hold-to-interact is in progress. The `progress` field is normalized between `0.0` and `1.0`. The UI progress bar maps this directly to its `value` (Unity's `Slider.value` is also 0–1). Per-frame allocations are why this is a struct — a class would generate one heap allocation per frame and trash the garbage collector.

---

## Event — HoldInteractCancelledEvent

```csharp
public struct HoldInteractCancelledEvent { }
```
**Explanation:** An empty signal struct. Fired when the player releases the hold key early or walks out of range mid-hold. The UI uses this to reset the progress bar to zero. **Not** fired on a successful hold completion — see the `ResetHold` vs `CancelHold` separation in the controller. An empty struct still has a unique `Type`, which is all `EventBus` keys on, so payload-less signalling works perfectly.

---

## Why Structs Instead Of Classes

Every event here is a `struct`:

- **Value semantics** — passed by copy through `EventBus.Publish<T>`, so subscribers can't mutate the original.
- **Zero allocation** — structs live on the stack (or are inlined into the caller's frame), so they don't generate garbage.
- **Cheap** — these payloads are small (one or two reference fields, plus a string in two cases). Below the threshold where boxing into a delegate would actually beat a struct.

`HoldInteractProgressEvent` fires roughly 60 times per second per active hold. If it were a class, that's 60 heap allocations per second per active hold, each of which the GC eventually has to sweep up. Structs make the cost zero.
