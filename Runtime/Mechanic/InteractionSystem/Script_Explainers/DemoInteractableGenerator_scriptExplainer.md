# DemoInteractableGenerator — Line-by-Line Script Explainer

```
SCRIPT     : DemoInteractableGenerator.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Samples.Interaction
LOCATION   : Samples~/InteractionSystemSample/Assets/Scripts/
```

---

## Purpose

Demo interactable: a generator that demonstrates **hold-to-interact**. The interactable itself is the same shape as every other one — five interface methods plus `Priority`. The hold logic lives entirely in the `InteractionController_UMFOSS` (its `requireHold` and `holdDuration` fields). To test this demo: enable `Require Hold` on the player's controller. The controller publishes `HoldInteractProgressEvent` every frame so the prompt's progress bar fills.

---

## Imports & Namespace

```csharp
using UnityEngine;
using GameplayMechanicsUMFOSS.Interaction;
```
**Explanation:** Same imports as door/NPC — only the runtime interface is needed.

```csharp
namespace GameplayMechanicsUMFOSS.Samples.Interaction
```
**Explanation:** Sample namespace.

---

## Class Declaration

```csharp
public class DemoInteractableGenerator : MonoBehaviour, IInteractable_UMFOSS
```
**Explanation:** Same MonoBehaviour + interface implementation pattern.

---

## Serialized Fields — Generator Settings

```csharp
[Header("Generator Settings")]
[SerializeField] private bool isActivated = false;
```
**Explanation:** Single-use latch. Once true, stays true for the rest of the play session. Exposed in the inspector mainly for testing — flip in the editor to verify the activated state visuals.

---

## Serialized Fields — Visual Feedback

```csharp
[Header("Visual Feedback")]
[SerializeField] private SpriteRenderer spriteRenderer;
[SerializeField] private Color activatedColor = Color.green;
[SerializeField] private Color focusedColor = Color.cyan;
```
**Explanation:** Three colour states: original (cached in `Awake`), focused (cyan — "ready to interact"), activated (green — "done"). Using two distinct highlight colours communicates state through colour alone, which is helpful for accessibility.

---

## Serialized Fields — Priority

```csharp
[Header("Priority")]
[SerializeField] private int priority = 0;

public int Priority => priority;
```
**Explanation:** Standard priority pattern.

---

## Private Fields

```csharp
private Color originalColor;
```
**Explanation:** Snapshot of the renderer's starting colour, captured in `Awake` and restored on unfocus. Without this, the colour would stick on cyan after the player walks away.

---

## Unity Lifecycle — Awake

```csharp
private void Awake()
{
    if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    if (spriteRenderer != null) originalColor = spriteRenderer.color;
}
```
**Explanation:** Two-step setup. (1) Auto-find renderer if not assigned in the inspector. (2) Cache its current colour. Both null-checked so missing references log a warning but don't throw.

---

## IInteractable_UMFOSS — Interact

```csharp
public void Interact(GameObject interactor)
{
    isActivated = true;

    if (spriteRenderer != null) spriteRenderer.color = activatedColor;

    Debug.Log($"[InteractionSystem] Generator activated by {interactor.name}!");
}
```
**Explanation:** Called by the controller only after the player has successfully held the key for `holdDuration` seconds. Sets the latch, swaps the colour to "activated", logs the event. From here on, `CanInteract` returns false, so further focus attempts are blocked.

---

## IInteractable_UMFOSS — OnFocused / OnUnfocused

```csharp
public void OnFocused(GameObject interactor)
{
    if (!isActivated && spriteRenderer != null) spriteRenderer.color = focusedColor;
}

public void OnUnfocused(GameObject interactor)
{
    if (!isActivated && spriteRenderer != null) spriteRenderer.color = originalColor;
}
```
**Explanation:** Hover tint, but **only when not yet activated**. Once activated, the green colour overrides everything — focus-tinting an already-activated generator would be misleading. The `!isActivated` guard preserves the activated state through any subsequent focus events.

---

## IInteractable_UMFOSS — GetInteractionPrompt

```csharp
public string GetInteractionPrompt()
{
    return isActivated ? "Already activated" : "Hold E to activate";
}
```
**Explanation:** Dynamic prompt based on state. Once activated, the prompt reads "Already activated" — except `CanInteract` returns false at that point so the prompt never actually shows. The state-aware prompt is mostly for clarity if the controller is ever in a mode that bypasses the gate.

---

## IInteractable_UMFOSS — CanInteract

```csharp
public bool CanInteract(GameObject interactor)
{
    return !isActivated;
}
```
**Explanation:** Standard single-use gate. Combined with the controller's hold logic, the full sequence is: focus → hold E → progress bar fills → `Interact` fires → `isActivated` flips → next frame `CanInteract` returns false → focus clears → prompt hides.

---

## Important Note On Hold Configuration

The hold logic lives **on the controller**, not on the interactable. This means the controller's `requireHold` flag is global — affecting every interactable it focuses. For a mixed scene with both instant and hold targets, options are:

1. **Two controllers** with different `requireHold` settings, switching between them at runtime.
2. **A future enhancement** to add `bool RequiresHold => false` to `IInteractable_UMFOSS` and let the controller pick mode per target.

This trade-off is documented as a known limitation and is the cleanest extension point for the system going forward.
