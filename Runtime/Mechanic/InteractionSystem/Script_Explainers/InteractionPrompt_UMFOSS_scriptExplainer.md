# InteractionPrompt_UMFOSS — Line-by-Line Script Explainer

```
SCRIPT     : InteractionPrompt_UMFOSS.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Interaction
```

---

## Purpose

A standalone UI component that subscribes to `EventBus` events and shows or hides the interaction prompt — including the optional hold progress bar. It **never** references `InteractionController_UMFOSS` or any specific interactable. This means the entire UI design can be replaced (different fonts, animations, layouts) without touching a single line of gameplay code.

---

## Imports & Namespace

```csharp
using UnityEngine;
```
**Explanation:** Required for `MonoBehaviour`, `GameObject`, `SerializeField`, and the `OnEnable`/`OnDisable` lifecycle.

```csharp
using UnityEngine.UI;
```
**Explanation:** Brings in `Slider`, used for the optional hold-progress bar.

```csharp
using TMPro;
```
**Explanation:** Brings in `TextMeshProUGUI`. The prompt label uses TMP for crisp text rendering at any resolution — Unity's legacy `Text` component is not used.

```csharp
using GameplayMechanicsUMFOSS.Core;
```
**Explanation:** Imports the `EventBus` static class from the framework's `Core` namespace. All four event subscriptions in this file go through it.

```csharp
namespace GameplayMechanicsUMFOSS.Interaction
```
**Explanation:** Same namespace as the controller and events — keeps the entire interaction system grouped together.

---

## Class Declaration

```csharp
public class InteractionPrompt_UMFOSS : MonoBehaviour
```
**Explanation:** Plain `MonoBehaviour`. No `[DisallowMultipleComponent]`, because there's no harm in someone temporarily having two prompt panels for testing. No `[RequireComponent]`, because the slot fields are pure references — the prompt panel can live anywhere in the hierarchy.

---

## Serialized Fields — UI References

```csharp
[Header("UI References")]
[SerializeField] private GameObject promptPanel;
```
**Explanation:** The parent container holding the entire prompt UI. `SetActive(true/false)` toggles visibility. A `GameObject` reference is preferred over a `CanvasGroup` because it's simpler, works on any Unity version, and instantly hides children including the slider — no fade or transparency needed for the basic case.

```csharp
[SerializeField] private TextMeshProUGUI promptLabel;
```
**Explanation:** The TMP label that displays the per-interactable prompt text — set from `eventData.promptText` whenever a new target is focused.

```csharp
[SerializeField] private Slider holdProgressBar;
```
**Explanation:** Optional. If assigned, fills 0 to 1 during a hold-to-interact. If left null, hold interactions still work — the visual bar is just absent. The slider's `value` field is also 0–1, so the event's `progress` maps directly with no math.

---

## Unity Lifecycle — OnEnable

```csharp
private void OnEnable()
{
    EventBus.Subscribe<InteractableDetectedEvent>(OnDetected);
    EventBus.Subscribe<InteractableLostEvent>(OnLost);
    EventBus.Subscribe<HoldInteractProgressEvent>(OnHoldProgress);
    EventBus.Subscribe<HoldInteractCancelledEvent>(OnHoldCancelled);
}
```
**Explanation:** Subscribes to all four events on enable. `OnEnable` is the correct Unity hook for this — not `Awake` — because subscriptions must be re-established if the component is disabled and re-enabled. It mirrors the lifecycle of `OnDisable`.

**Important note in source:** The component does *not* subscribe to `InteractionPerformedEvent`. Subscribing to it caused a one-frame flicker on repeatable interactables (the NPC) — the prompt would hide on `Performed`, then re-show next frame when the NPC re-focused. `InteractableLostEvent` already covers the single-use case (pickups, doors that go non-interactable after use).

---

## Unity Lifecycle — OnDisable

```csharp
private void OnDisable()
{
    EventBus.Unsubscribe<InteractableDetectedEvent>(OnDetected);
    EventBus.Unsubscribe<InteractableLostEvent>(OnLost);
    EventBus.Unsubscribe<HoldInteractProgressEvent>(OnHoldProgress);
    EventBus.Unsubscribe<HoldInteractCancelledEvent>(OnHoldCancelled);
}
```
**Explanation:** Mandatory cleanup. Without these unsubscribes, `EventBus` would still hold delegates pointing at this (now disabled) instance. The next time an event fires, those delegates would invoke methods on a stale object — causing null reference exceptions and silent memory leaks across scene loads.

---

## Unity Lifecycle — Start

```csharp
private void Start()
{
    HidePrompt();
}
```
**Explanation:** The prompt should not show on scene load — only when the player is actually near an interactable. `Start` runs after the first `Awake` of every component so it's safe to manipulate the UI here.

---

## Event Handler — OnDetected

```csharp
private void OnDetected(InteractableDetectedEvent eventData)
{
    if (promptPanel != null) promptPanel.SetActive(true);
    if (promptLabel != null) promptLabel.text = eventData.promptText;
    ResetProgressBar();
}
```
**Explanation:** Called when focus shifts to a new interactable. Activates the panel, sets the label text from the event data, and resets the progress bar — the previous target's hold-progress (if any) shouldn't bleed into the new target's display. Each null check exists because the slot fields are optional in the inspector.

---

## Event Handler — OnLost

```csharp
private void OnLost(InteractableLostEvent eventData)
{
    HidePrompt();
}
```
**Explanation:** Called when focus is cleared — out of range, deactivated, or shifted to a different target. Just hides the entire prompt. The event payload is unused; the UI doesn't care which interactable was lost.

---

## Event Handler — OnHoldProgress

```csharp
private void OnHoldProgress(HoldInteractProgressEvent eventData)
{
    if (holdProgressBar != null)
    {
        holdProgressBar.gameObject.SetActive(true);
        holdProgressBar.value = eventData.progress;
    }
}
```
**Explanation:** Fires every frame during a hold. Reveals the slider (in case it was hidden) and writes the 0-1 progress directly into `Slider.value`. No interpolation needed — the controller already updates 60 times per second.

---

## Event Handler — OnHoldCancelled

```csharp
private void OnHoldCancelled(HoldInteractCancelledEvent eventData)
{
    ResetProgressBar();
}
```
**Explanation:** Called when the player released the hold key early or walked out of range mid-hold. Resets the bar to zero and hides it. Importantly: not called on successful completion — the controller deliberately uses a silent `ResetHold()` on success to avoid sending a "cancelled" signal after a victory.

---

## Private Methods — HidePrompt

```csharp
private void HidePrompt()
{
    if (promptPanel != null) promptPanel.SetActive(false);
    ResetProgressBar();
}
```
**Explanation:** Hides the entire prompt UI. Always resets the progress bar at the same time, so the next time the panel re-shows it doesn't briefly flash an old value before `OnDetected` clears it.

---

## Private Methods — ResetProgressBar

```csharp
private void ResetProgressBar()
{
    if (holdProgressBar != null)
    {
        holdProgressBar.value = 0f;
        holdProgressBar.gameObject.SetActive(false);
    }
}
```
**Explanation:** Clears the bar value and hides its GameObject. The bar is only shown when an active hold exists — visible all the time would be visual noise for instant-press interactables.

---

## Why This Component Is Decoupled

`InteractionPrompt_UMFOSS` has zero references to `InteractionController_UMFOSS`. It doesn't know anything about layers, detection modes, hold durations, or input keys. It only knows four event types.

Consequences:

- The prompt can live on a different GameObject, in a different scene, or be entirely missing — the controller still works.
- A completely different prompt UI (animated, audio-driven, fading) can replace this script without changing the controller or any interactable.
- Multiple prompts can listen at once (e.g., a debug overlay alongside the player UI) by simply enabling another `InteractionPrompt_UMFOSS` instance.

This is the EventBus pattern at its strongest: the publisher does not know who is listening, and the listeners do not know who is publishing.
