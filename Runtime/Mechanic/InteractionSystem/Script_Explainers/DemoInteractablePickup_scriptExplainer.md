# DemoInteractablePickup — Line-by-Line Script Explainer

```
SCRIPT     : DemoInteractablePickup.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Samples.Interaction
LOCATION   : Samples~/InteractionSystemSample/Assets/Scripts/
```

---

## Purpose

Demo interactable: a collectable item that disappears on pickup. Demonstrates two important patterns: (1) the interactable **publishes its own custom event** (`ItemPickedUpEvent`) that other systems subscribe to without coupling, and (2) the **deactivation pattern** — `gameObject.SetActive(false)` after `Interact` is what `RefreshCurrentInteractable` in the controller is designed to handle gracefully.

---

## Imports & Namespace

```csharp
using UnityEngine;
using GameplayMechanicsUMFOSS.Interaction;
using GameplayMechanicsUMFOSS.Core;
```
**Explanation:** `Core` is added because this script publishes through `EventBus` — unlike the door or NPC, which only respond to events.

```csharp
namespace GameplayMechanicsUMFOSS.Samples.Interaction
```
**Explanation:** Sample namespace.

---

## Custom Event Struct — ItemPickedUpEvent

```csharp
public struct ItemPickedUpEvent
{
    public string itemName;
    public GameObject picker;
}
```
**Explanation:** Demo-defined event. Lives next to the interactable that publishes it because it's specifically for pickups, not the general interaction pipeline. The inventory display script subscribes to this event directly without ever knowing about pickups, demonstrating that `EventBus` works for any user-defined struct, not only the framework's built-in events.

---

## Class Declaration

```csharp
public class DemoInteractablePickup : MonoBehaviour, IInteractable_UMFOSS
```
**Explanation:** Standard interactable shape.

---

## Serialized Fields — Item Settings

```csharp
[Header("Item Settings")]
[SerializeField] private string itemName = "Health Potion";
```
**Explanation:** Display name carried through the prompt and the pickup event.

---

## Serialized Fields — Visual Feedback

```csharp
[Header("Visual Feedback")]
[SerializeField] private float floatSpeed = 2f;
[SerializeField] private float floatHeight = 0.3f;
```
**Explanation:** Drives a sine-wave bobbing animation when the player is near. `floatSpeed` is the wave frequency in radians per second; `floatHeight` is the amplitude in world units.

---

## Serialized Fields — Priority

```csharp
[Header("Priority")]
[SerializeField] private int priority = 0;

public int Priority => priority;
```
**Explanation:** Standard priority backing.

---

## Public Property — ItemName

```csharp
public string ItemName => itemName;
```
**Explanation:** Read-only accessor in case other scripts want to query the name without going through the event payload.

---

## Private Fields

```csharp
private bool isFocused = false;
private Vector3 originalPosition;
```
**Explanation:** `isFocused` gates the bobbing animation — it only plays while in focus. `originalPosition` is captured in `Awake` so the bobbing oscillates around the spawn position regardless of where the item moves to during the wave.

---

## Unity Lifecycle — Awake

```csharp
private void Awake()
{
    originalPosition = transform.position;
}
```
**Explanation:** Snapshot the starting position. Recorded in `Awake`, not the first `OnFocused`, so it captures the design-time position before any code touches the transform.

---

## Unity Lifecycle — Update

```csharp
private void Update()
{
    if (isFocused)
    {
        float newY = originalPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(originalPosition.x, newY, originalPosition.z);
    }
}
```
**Explanation:** Bob animation. `Mathf.Sin(Time.time * floatSpeed)` produces a continuous -1 to +1 wave; multiplying by `floatHeight` gives the displacement in units. Using `Time.time` rather than accumulating `deltaTime` means the wave is in absolute phase — no drift, no sync issues. Only runs while `isFocused` is true so background pickups stay still.

---

## IInteractable_UMFOSS — Interact

```csharp
public void Interact(GameObject interactor)
{
    Debug.Log($"[InteractionSystem] {interactor.name} picked up {itemName}");

    EventBus.Publish(new ItemPickedUpEvent
    {
        itemName = itemName,
        picker = interactor
    });

    gameObject.SetActive(false);
}
```
**Explanation:** Three steps. (1) Log for the demo console. (2) Publish the custom event with both the item name and the picker — any subscriber (inventory UI, achievement system, audio manager) reacts without ever referencing this script. (3) `SetActive(false)` to make the pickup disappear. The controller's `RefreshCurrentInteractable` notices the deactivation the same frame and clears focus immediately.

---

## IInteractable_UMFOSS — OnFocused / OnUnfocused

```csharp
public void OnFocused(GameObject interactor)
{
    isFocused = true;
}

public void OnUnfocused(GameObject interactor)
{
    isFocused = false;
    transform.position = originalPosition;
}
```
**Explanation:** Toggles the bob flag. On unfocus, also snaps the position back to `originalPosition` — without this, the item could be left mid-bob (slightly above its base position) when the player walks away.

---

## IInteractable_UMFOSS — GetInteractionPrompt

```csharp
public string GetInteractionPrompt()
{
    return $"Press E to pick up {itemName}";
}
```
**Explanation:** Interpolated string so the prompt names the actual item, not "Press E to pick up". Personalised prompts make every pickup feel distinct.

---

## IInteractable_UMFOSS — CanInteract

```csharp
public bool CanInteract(GameObject interactor)
{
    return gameObject.activeSelf;
}
```
**Explanation:** Returns false once the item is deactivated. After `Interact` runs `SetActive(false)`, this gate stops the item from reappearing as a focus candidate. Combined with `RefreshCurrentInteractable` in the controller, the pickup vanishes cleanly on the same frame the player picked it up.
