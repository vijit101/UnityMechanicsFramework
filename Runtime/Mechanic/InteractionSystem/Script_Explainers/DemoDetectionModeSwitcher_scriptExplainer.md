# DemoDetectionModeSwitcher — Line-by-Line Script Explainer

```
SCRIPT     : DemoDetectionModeSwitcher.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Samples.Interaction
LOCATION   : Samples~/InteractionSystemSample/Assets/Scripts/
```

---

## Purpose

Demo utility: cycles through the three detection modes (Trigger / OverlapCircle / Raycast) at runtime via a key press or a UI button. Lets a tester compare how the modes feel without restarting the play session — exactly the scenario `InteractionController_UMFOSS.SetDetectionMode` was designed for.

---

## Imports & Namespace

```csharp
using UnityEngine;
using TMPro;
using GameplayMechanicsUMFOSS.Interaction;
```
**Explanation:** `TMPro` for the on-screen mode label; `Interaction` for `InteractionController_UMFOSS` and the `DetectionMode` enum.

```csharp
namespace GameplayMechanicsUMFOSS.Samples.Interaction
```
**Explanation:** Sample namespace.

---

## Class Declaration

```csharp
public class DemoDetectionModeSwitcher : MonoBehaviour
```
**Explanation:** Plain MonoBehaviour — this class doesn't implement `IInteractable_UMFOSS`. It's a utility script that operates on the controller, not an interactable itself.

---

## Serialized Fields — References

```csharp
[Header("References")]
[SerializeField] private InteractionController_UMFOSS controller;
```
**Explanation:** Direct reference to the player's controller. This script needs to call methods on it (`SetDetectionMode`) and read state from it (`CurrentDetectionMode`), so a direct reference is appropriate — unlike the prompt, which uses events.

---

## Serialized Fields — Input

```csharp
[Header("Input")]
[SerializeField] private KeyCode switchKey = KeyCode.Tab;
```
**Explanation:** The key that cycles modes. Tab is unobtrusive — doesn't conflict with WASD movement or the interact key.

---

## Serialized Fields — UI

```csharp
[Header("UI")]
[SerializeField] private TextMeshProUGUI modeLabel;
```
**Explanation:** Optional on-screen label so the player can see the current mode. Null-tolerant — without it, mode switches still work but there's no visual confirmation beyond behaviour change.

---

## Constants

```csharp
private const int DETECTION_MODE_COUNT = 3;
```
**Explanation:** Number of values in the `DetectionMode` enum. Using a named constant rather than `3` so the modulo math reads clearly. If the enum ever grows a fourth member, this is the only line that needs updating (along with adding the case in `SetDetectionMode`).

---

## Unity Lifecycle — Start

```csharp
private void Start()
{
    UpdateLabel();
}
```
**Explanation:** Initialises the label so the user sees the starting mode without having to press the switch key once.

---

## Unity Lifecycle — Update

```csharp
private void Update()
{
    if (Input.GetKeyDown(switchKey))
    {
        CycleDetectionMode();
    }
}
```
**Explanation:** Reads the configured key. `GetKeyDown` (not `GetKey`) so a single press cycles by one step — `GetKey` would cycle every frame the key is held, scrolling through modes too fast to use.

---

## Public Method — CycleDetectionMode

```csharp
public void CycleDetectionMode()
{
    if (controller == null)
    {
        Debug.LogWarning("[InteractionSystem] DemoDetectionModeSwitcher: No controller assigned.");
        return;
    }

    int currentIndex = (int)controller.CurrentDetectionMode;
    int nextIndex = (currentIndex + 1) % DETECTION_MODE_COUNT;
    DetectionMode nextMode = (DetectionMode)nextIndex;

    controller.SetDetectionMode(nextMode);
    UpdateLabel();

    Debug.Log($"[InteractionSystem] Detection mode switched to: {nextMode}");
}
```
**Explanation:** Public so a UI Button's `OnClick` can wire to it directly without going through Unity Events bindings to the keyboard. Steps: (1) defensive null check on the controller — logs and returns instead of throwing, (2) cast the current mode to int to do arithmetic, (3) modulo wraps from the last mode back to the first, (4) cast back to the enum, (5) call the controller's setter (which clears focus and toggles the trigger collider), (6) refresh the label.

---

## Private Method — UpdateLabel

```csharp
private void UpdateLabel()
{
    if (modeLabel != null && controller != null)
    {
        modeLabel.text = $"Detection: {controller.CurrentDetectionMode}\n(Press {switchKey} to switch)";
    }
}
```
**Explanation:** Updates the on-screen display with both the current mode and the key to press for the next switch. Two-line interpolated string — first line is the current state, second line is the discoverable hint. Both null checks because either ref could be unassigned in a custom setup.

---

## Why This Script Is A Demo Utility

This script is **not** part of the core interaction package — it's a tool for the demo scene. The only reason it exists in `Samples~/` is to let a tester verify all three detection modes work without writing a custom script. In a shipping game, mode would typically be set once in the inspector and never changed at runtime.

The fact that runtime mode-switching works at all is a bonus capability of `InteractionController_UMFOSS.SetDetectionMode` — useful for power-ups (e.g. a mask that switches the player from `OverlapCircle` to `Raycast` for stealth section gameplay).
