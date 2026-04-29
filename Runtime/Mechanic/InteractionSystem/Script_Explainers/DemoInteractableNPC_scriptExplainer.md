# DemoInteractableNPC — Line-by-Line Script Explainer

```
SCRIPT     : DemoInteractableNPC.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Samples.Interaction
LOCATION   : Samples~/InteractionSystemSample/Assets/Scripts/
```

---

## Purpose

Demo interactable: an NPC the player can talk to repeatedly. Demonstrates the **repeatable pattern** with a temporary cooldown — `CanInteract` returns false during dialogue, then re-enables itself after a delay. Also shows a name-tag that appears on focus.

---

## Imports & Namespace

```csharp
using UnityEngine;
using GameplayMechanicsUMFOSS.Interaction;
```
**Explanation:** Standard pair: Unity engine + the Interaction interface.

```csharp
namespace GameplayMechanicsUMFOSS.Samples.Interaction
```
**Explanation:** Sample namespace, distinct from runtime.

---

## Class Declaration

```csharp
public class DemoInteractableNPC : MonoBehaviour, IInteractable_UMFOSS
```
**Explanation:** MonoBehaviour + interface — same shape as every other interactable.

---

## Serialized Fields — NPC Settings

```csharp
[Header("NPC Settings")]
[SerializeField] private string npcName = "Village Elder";
```
**Explanation:** Shown in the debug log and could feed into a name-tag UI. String, not a localised key, since this is a demo.

```csharp
[SerializeField] private string[] dialogueLines = new string[]
{
    "Hello, traveler! Welcome to our village.",
    "The forest to the east holds many secrets.",
    "Be careful out there — danger lurks in the shadows."
};
```
**Explanation:** Array of lines cycled through on each successive interaction. Default values are pre-populated so dropping the script onto a GameObject yields a working NPC immediately — no inspector setup required to test.

---

## Serialized Fields — Visual Feedback

```csharp
[Header("Visual Feedback")]
[SerializeField] private GameObject nameTagObject;
```
**Explanation:** Optional child GameObject (typically a TextMeshPro) shown above the NPC on focus. Null-tolerant — if not assigned, focus simply has no visual effect.

---

## Serialized Fields — Dialogue

```csharp
[Header("Dialogue")]
[SerializeField] private float dialogueCooldown = 1.5f;
```
**Explanation:** Seconds before the NPC becomes interactable again after a conversation begins. Simulates dialogue duration without needing a full dialogue system. In a real game this would be replaced by a callback from the dialogue system (`OnDialogueEnded`).

---

## Serialized Fields — Priority

```csharp
[Header("Priority")]
[SerializeField] private int priority = 0;

public int Priority => priority;
```
**Explanation:** Standard priority field. Could be set higher (e.g. 5) so a quest NPC always wins focus over nearby props.

---

## Private Fields

```csharp
private bool isTalking = false;
private int currentDialogueIndex = 0;
```
**Explanation:** Two pieces of state. `isTalking` gates the second interaction during the cooldown. `currentDialogueIndex` cycles through `dialogueLines`.

---

## Unity Lifecycle — Awake

```csharp
private void Awake()
{
    if (nameTagObject != null) nameTagObject.SetActive(false);
}
```
**Explanation:** Hide the name tag on scene load. The tag should only appear when the player approaches.

---

## IInteractable_UMFOSS — Interact

```csharp
public void Interact(GameObject interactor)
{
    isTalking = true;

    string line = dialogueLines[currentDialogueIndex];
    Debug.Log($"[InteractionSystem] {npcName}: \"{line}\"");

    currentDialogueIndex = (currentDialogueIndex + 1) % dialogueLines.Length;

    Invoke(nameof(EndDialogue), dialogueCooldown);
}
```
**Explanation:** Five steps. (1) Flip `isTalking` so subsequent interactions are blocked. (2) Read the current line. (3) Log it (a real game would call into `DialogueUIManager` here instead). (4) Advance the index modulo the array length — wraps cleanly back to 0. (5) Schedule `EndDialogue` to run after the cooldown via `Invoke`. The whole cycle re-enables interaction after the cooldown elapses.

---

## IInteractable_UMFOSS — OnFocused / OnUnfocused

```csharp
public void OnFocused(GameObject interactor)
{
    if (nameTagObject != null) nameTagObject.SetActive(true);
    Debug.Log($"[InteractionSystem] NPC name tag shown: {npcName}");
}

public void OnUnfocused(GameObject interactor)
{
    if (nameTagObject != null) nameTagObject.SetActive(false);
}
```
**Explanation:** Show/hide the name tag in lockstep with focus. Null check because the tag is optional.

---

## IInteractable_UMFOSS — GetInteractionPrompt

```csharp
public string GetInteractionPrompt()
{
    return "Press E to talk";
}
```
**Explanation:** Distinguishes this NPC from a door's "Press E to open". Different prompts per interactable type are exactly what `IInteractable_UMFOSS.GetInteractionPrompt` is for.

---

## IInteractable_UMFOSS — CanInteract

```csharp
public bool CanInteract(GameObject interactor)
{
    return !isTalking;
}
```
**Explanation:** Returns false during the cooldown window. Once `EndDialogue` flips `isTalking` back to false, the controller's selection logic re-includes this NPC and the prompt re-appears.

---

## Private — EndDialogue

```csharp
private void EndDialogue()
{
    isTalking = false;
    Debug.Log($"[InteractionSystem] Dialogue with {npcName} ended.");
}
```
**Explanation:** Invoked after `dialogueCooldown` seconds via `Invoke`. Re-enables interaction. In a real game, this method (or its equivalent) would be called by a dialogue-end callback rather than a timer — but for the demo a fixed delay communicates the pattern clearly.
