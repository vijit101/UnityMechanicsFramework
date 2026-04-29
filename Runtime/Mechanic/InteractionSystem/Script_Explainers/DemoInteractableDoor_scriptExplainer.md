# DemoInteractableDoor — Line-by-Line Script Explainer

```
SCRIPT     : DemoInteractableDoor.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Samples.Interaction
LOCATION   : Samples~/InteractionSystemSample/Assets/Scripts/
```

---

## Purpose

Demo interactable: a single-use door that swings open on interact. Demonstrates the **single-use pattern** — `CanInteract` returns false after the first interaction, so the prompt disappears and the door can't be re-triggered. Also shows visual focus feedback via sprite tint.

---

## Imports & Namespace

```csharp
using UnityEngine;
using GameplayMechanicsUMFOSS.Interaction;
```
**Explanation:** `UnityEngine` for the standard types; `Interaction` namespace to access `IInteractable_UMFOSS`. The interface lives in the runtime package — samples depend on it but the runtime never depends on samples.

```csharp
namespace GameplayMechanicsUMFOSS.Samples.Interaction
```
**Explanation:** Sample namespace — keeps demo classes out of the runtime API surface.

---

## Class Declaration

```csharp
public class DemoInteractableDoor : MonoBehaviour, IInteractable_UMFOSS
```
**Explanation:** Multi-implementation: `MonoBehaviour` so Unity can attach it, plus `IInteractable_UMFOSS` so the controller treats it as an interactable. Note: there's no shared base class — every demo interactable independently inherits MonoBehaviour and implements the same interface, which is exactly the point of using an interface instead of a class hierarchy.

---

## Serialized Fields — Door Settings

```csharp
[Header("Door Settings")]
[SerializeField] private float openAngle = 90f;
```
**Explanation:** How far the door rotates when opened — 90 degrees is the natural quarter-turn most players expect.

```csharp
[SerializeField] private float openSpeed = 3f;
```
**Explanation:** Lerp rate for the opening animation. Higher = snappier. Multiplied by `Time.deltaTime` so it's frame-rate-independent.

---

## Serialized Fields — Visual Feedback

```csharp
[Header("Visual Feedback")]
[SerializeField] private SpriteRenderer spriteRenderer;
[SerializeField] private Color highlightColor = Color.yellow;
```
**Explanation:** Optional renderer reference + the colour used while the player is in range. `Awake` falls back to `GetComponent` if not assigned in inspector.

---

## Serialized Fields — Priority

```csharp
[Header("Priority")]
[SerializeField] private int priority = 0;

public int Priority => priority;
```
**Explanation:** Inspector-tweakable priority backing field, exposed read-only via the interface property. 0 means "no preference" — only matters when the controller's `SelectionMode` is set to `HighestPriority`.

---

## Private Fields

```csharp
private bool isOpen = false;
private bool isOpening = false;
private Color originalColor;
private Quaternion closedRotation;
private Quaternion openRotation;
```
**Explanation:** State flags and cached transforms. `isOpen` is the permanent state; `isOpening` is the transient animating-now flag. `closedRotation` and `openRotation` are computed once in `Awake` so the door always opens to the same target regardless of where Update happens to be when interact is pressed.

---

## Unity Lifecycle — Awake

```csharp
private void Awake()
{
    if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    if (spriteRenderer != null) originalColor = spriteRenderer.color;

    closedRotation = transform.rotation;
    openRotation = Quaternion.Euler(0f, 0f, openAngle) * closedRotation;
}
```
**Explanation:** Three setup steps. (1) Auto-find the sprite renderer if not pre-assigned. (2) Cache the original colour so `OnUnfocused` can restore it. (3) Cache the open rotation as `openAngle` z-rotation applied to the closed rotation — quaternion multiplication composes rotations correctly even if the door starts at a non-zero base angle.

---

## Unity Lifecycle — Update

```csharp
private void Update()
{
    if (isOpening)
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, Time.deltaTime * openSpeed);

        if (Quaternion.Angle(transform.rotation, openRotation) < 0.5f)
        {
            transform.rotation = openRotation;
            isOpening = false;
        }
    }
}
```
**Explanation:** Drives the opening animation. `Quaternion.Lerp` with `Time.deltaTime * openSpeed` produces an exponential approach — fast at first, slowing as it nears the target. The 0.5-degree threshold snaps the final position exactly to the target and turns off the flag, since Lerp never reaches the target asymptotically. `Update` does nothing when `isOpening` is false — no per-frame cost when the door isn't animating.

---

## IInteractable_UMFOSS — Interact

```csharp
public void Interact(GameObject interactor)
{
    if (isOpen) return;

    isOpen = true;
    isOpening = true;
    Debug.Log($"[InteractionSystem] Door opened by {interactor.name}");
}
```
**Explanation:** The action. Defensive `isOpen` early-exit (the controller already gates on `CanInteract`, but a belt-and-braces guard means external callers can't bypass it). Sets both flags — `isOpen` is the permanent state, `isOpening` triggers the Update animation loop.

---

## IInteractable_UMFOSS — OnFocused / OnUnfocused

```csharp
public void OnFocused(GameObject interactor)
{
    if (spriteRenderer != null) spriteRenderer.color = highlightColor;
}

public void OnUnfocused(GameObject interactor)
{
    if (spriteRenderer != null) spriteRenderer.color = originalColor;
}
```
**Explanation:** Pure visual feedback — tint to highlight on focus, restore the cached original on unfocus. Null check because the renderer reference is optional.

---

## IInteractable_UMFOSS — GetInteractionPrompt

```csharp
public string GetInteractionPrompt()
{
    return "Press E to open";
}
```
**Explanation:** Static prompt string. Could be data-driven (e.g., reading from a localisation table), but for a demo plain text is clearer.

---

## IInteractable_UMFOSS — CanInteract

```csharp
public bool CanInteract(GameObject interactor)
{
    return !isOpen;
}
```
**Explanation:** The single-use gate. After `Interact` flips `isOpen` to true, `CanInteract` returns false forever — the controller's `SelectBestInteractable` skips this door from then on, the prompt disappears, and a second press won't trigger anything. No need to manually unfocus or destroy the object.
