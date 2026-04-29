# DemoInventoryDisplay — Line-by-Line Script Explainer

```
SCRIPT     : DemoInventoryDisplay.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Samples.Interaction
LOCATION   : Samples~/InteractionSystemSample/Assets/Scripts/
```

---

## Purpose

Demo UI: an on-screen inventory list plus a detection-mode readout. Demonstrates **pure EventBus subscription** — collected items are tracked here without this script ever knowing about `DemoInteractablePickup`. This is the decoupling pattern at full strength: the publisher doesn't know about the listener, the listener doesn't know about the publisher, and either side can be replaced without breaking the other.

---

## Imports & Namespace

```csharp
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Interaction;
```
**Explanation:** `Generic` for `List<string>`. `TMPro` for the labels. `Core` for `EventBus`. `Interaction` for `InteractionController_UMFOSS` so the detection-mode readout works.

```csharp
namespace GameplayMechanicsUMFOSS.Samples.Interaction
```
**Explanation:** Sample namespace.

---

## Class Declaration

```csharp
public class DemoInventoryDisplay : MonoBehaviour
```
**Explanation:** Plain UI MonoBehaviour. No interface implementation — this class only consumes events, never produces them.

---

## Serialized Fields — UI References

```csharp
[Header("UI References")]
[SerializeField] private TextMeshProUGUI inventoryLabel;
[SerializeField] private TextMeshProUGUI detectionModeLabel;
```
**Explanation:** Two TMP labels — one shows the running list of collected items, the other shows the controller's current detection mode. Anchored top-left and top-right respectively in the demo scene.

---

## Serialized Fields — References

```csharp
[Header("References")]
[SerializeField] private InteractionController_UMFOSS controller;
```
**Explanation:** Reference to the player's controller, used only for reading `CurrentDetectionMode`. The inventory side of this script doesn't need this reference at all — pickups publish through `EventBus`, completely independently.

---

## Private Fields

```csharp
private readonly List<string> collectedItems = new List<string>();
```
**Explanation:** The inventory model — a list of item names. `readonly` prevents reassignment of the list reference (its contents still change). Strings are sufficient for the demo; a real inventory would store a richer object (`ItemData` ScriptableObject, quantity, durability, etc.).

---

## Unity Lifecycle — OnEnable

```csharp
private void OnEnable()
{
    EventBus.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
}
```
**Explanation:** Subscribes to the custom `ItemPickedUpEvent` defined in `DemoInteractablePickup.cs`. Note: this script doesn't need to import the pickup script's namespace beyond what's already there — the event struct is a public type accessible from the same assembly.

---

## Unity Lifecycle — OnDisable

```csharp
private void OnDisable()
{
    EventBus.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
}
```
**Explanation:** Mandatory unsubscribe to prevent memory leaks. Without it, `EventBus` retains a dead delegate pointing at this disabled component.

---

## Unity Lifecycle — Start

```csharp
private void Start()
{
    UpdateInventoryUI();
    UpdateDetectionModeUI();
}
```
**Explanation:** Initialises both labels so they show "(empty)" and the starting mode immediately on scene load — without it, both would show whatever placeholder text was typed in the inspector until the first event arrived.

---

## Unity Lifecycle — Update

```csharp
private void Update()
{
    UpdateDetectionModeUI();
}
```
**Explanation:** Polls the controller's mode every frame to keep the label fresh. **Could** be event-driven if `InteractionController_UMFOSS` published a `DetectionModeChangedEvent` — but for a demo, polling once per frame is cheap and correct.

---

## Event Handler — OnItemPickedUp

```csharp
private void OnItemPickedUp(ItemPickedUpEvent eventData)
{
    collectedItems.Add(eventData.itemName);
    UpdateInventoryUI();
}
```
**Explanation:** Appends the item name to the list and refreshes the display. The handler ignores `eventData.picker` because this single demo only has one player — multiplayer would key the inventory by `picker` instead.

---

## Private — UpdateInventoryUI

```csharp
private void UpdateInventoryUI()
{
    if (inventoryLabel == null) return;

    if (collectedItems.Count == 0)
    {
        inventoryLabel.text = "Inventory: (empty)";
    }
    else
    {
        inventoryLabel.text = "Inventory:\n" + string.Join("\n- ", collectedItems.ToArray());
    }
}
```
**Explanation:** Renders the inventory list. Empty state shows a placeholder so the label doesn't disappear. `string.Join("\n- ", ...)` produces the bullet list — note the joiner contains both the newline and the bullet, so each item except the first is prefixed.

---

## Private — UpdateDetectionModeUI

```csharp
private void UpdateDetectionModeUI()
{
    if (detectionModeLabel == null || controller == null) return;

    detectionModeLabel.text = $"Detection: {controller.CurrentDetectionMode}";
}
```
**Explanation:** Reads the controller's current mode and writes it to the label. Both null checks because either reference could be unassigned in a partial setup.

---

## Why This Script Proves The Decoupling Works

`DemoInventoryDisplay` does NOT reference `DemoInteractablePickup` anywhere. It listens for `ItemPickedUpEvent` and that's it.

Implications:

- A new pickup type (`DemoQuestItem`, `DemoCurrencyDrop`) just needs to publish the same event — no changes here.
- The pickup system can be ripped out entirely; this UI quietly stays empty without errors.
- A second listener (e.g. an `AchievementSystem` checking for "Picked Up 10 Items") can subscribe in parallel without touching anything.

This is the framework's central thesis: **systems communicate through events, not references**.
