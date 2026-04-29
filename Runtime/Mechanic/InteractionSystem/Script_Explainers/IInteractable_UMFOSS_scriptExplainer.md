# IInteractable_UMFOSS — Line-by-Line Script Explainer

```
SCRIPT     : IInteractable_UMFOSS.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Interaction
```

---

## Purpose

The contract that every interactable object in the game must implement. The `InteractionController_UMFOSS` only ever sees this interface — it has no idea whether the thing in front of the player is a door, an NPC, a chest, or a generator. Any new kind of interactable can be added later without touching a single line of the controller.

---

## Imports & Namespace

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity's core engine namespace. Required because `Interact(GameObject interactor)` and `OnFocused(GameObject interactor)` accept a `GameObject` parameter.

```csharp
namespace GameplayMechanicsUMFOSS.Interaction
```
**Explanation:** Places the interface inside the `Interaction` feature group of the framework. Keeps it isolated from other systems and avoids type collisions with any user code in the global namespace.

---

## Interface Declaration

```csharp
public interface IInteractable_UMFOSS
```
**Explanation:** Declared as an `interface` rather than an abstract class so that any `MonoBehaviour` can implement it — even if that MonoBehaviour already inherits from another base class. C# allows multiple interface implementations but only single class inheritance, so an interface is the right choice for a behavior contract that has to plug into many different concrete types.

---

## Method — Interact

```csharp
void Interact(GameObject interactor);
```
**Explanation:** Called by the controller when the player presses the interact key (or completes a hold). The implementer decides what happens — open the door, take damage, start dialogue. The `interactor` parameter is the entity performing the action (typically the player), so the implementer can query their inventory, stats, or position if needed. No return value: success/failure is signalled via the `CanInteract` gate before this method is even called.

---

## Method — OnFocused

```csharp
void OnFocused(GameObject interactor);
```
**Explanation:** Fires when this interactable becomes the *best candidate* in the controller's range — not just when the player walks into range, but when the controller decides this is the one to highlight. Use this to show outline, glow, name tag, or hover animation. Only one object is focused at a time per controller, even if many are in range.

---

## Method — OnUnfocused

```csharp
void OnUnfocused(GameObject interactor);
```
**Explanation:** Fires when focus leaves this object — either because a different interactable was selected, the player walked out of range, or `CanInteract` flipped to false. Use this to undo whatever `OnFocused` did. This is called even if the player never actually interacted.

---

## Method — GetInteractionPrompt

```csharp
string GetInteractionPrompt();
```
**Explanation:** Returns the human-readable text shown in the UI prompt — for example, "Press E to open" or "Hold E to activate". The controller snapshots this string once when focus is gained and publishes it through `InteractableDetectedEvent`. The interactable never references the UI directly — it just returns text.

---

## Method — CanInteract

```csharp
bool CanInteract(GameObject interactor);
```
**Explanation:** The gate. Returns `false` to suppress focus and the prompt entirely. Use this for: a chest that's already opened, a door requiring a key the player doesn't have, or an NPC mid-conversation. Checked **twice** — once during focus selection (so locked items can't steal focus from a usable item nearby) and once again right before `Interact` is called (in case state changed during the same frame).

---

## Property — Priority

```csharp
int Priority { get; }
```
**Explanation:** Used only when the controller's `SelectionMode` is set to `HighestPriority`. When multiple valid interactables are in range, the one with the highest priority wins regardless of distance. A quest-critical NPC at priority 10 will always be focused over a barrel at priority 0, even if the barrel is closer. Default implementations should return 0.

---

## How To Implement This Interface

```csharp
public class TreasureChest : MonoBehaviour, IInteractable_UMFOSS
{
    private bool isOpen = false;

    public int Priority => 0;

    public void Interact(GameObject interactor)       { isOpen = true; }
    public void OnFocused(GameObject interactor)      { /* glow */ }
    public void OnUnfocused(GameObject interactor)    { /* unglow */ }
    public string GetInteractionPrompt()              => "Press E to open";
    public bool CanInteract(GameObject interactor)    => !isOpen;
}
```
**Explanation:** A complete new interactable type in five methods + one property. Drop this onto a GameObject, set its layer to the configured `interactableLayer`, and the controller picks it up automatically. **Zero changes** to `InteractionController_UMFOSS` are needed — that is the entire point of the interface.
