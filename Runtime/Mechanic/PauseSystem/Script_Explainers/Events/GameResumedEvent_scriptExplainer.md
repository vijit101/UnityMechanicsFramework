# GameResumedEvent — Line-by-Line Script Explainer

- **Script:** `Events/GameResumedEvent.cs`
- **Author:** [Souvik Kumar](https://github.com/Souvik-Cyclic)
- **Namespace:** `GameplayMechanicsUMFOSS.Systems`

---

```csharp
namespace GameplayMechanicsUMFOSS.Systems
```
**Explanation:** Same namespace as `PauseSystem_UMFOSS` and the other pause events. Keeps all pause-related types together.

```csharp
public struct GameResumedEvent
```
**Explanation:** Value-type event — no heap allocation per resume. Symmetric with `GamePausedEvent`.

```csharp
public float restoredTimeScale;
```
**Explanation:** The exact `Time.timeScale` value pushed back after resume. Equals `previousTimeScale` from the matching `GamePausedEvent` — preserves bullet time / slow motion across pause cycles. Subscribers can use it to verify the time scale they observe matches what the pause system intended.

---

## Role in the system

Fired by `PauseSystem_UMFOSS.Resume()` after `Time.timeScale` is restored and `AudioListener.pause` is set back to false. Mirror of `GamePausedEvent` — every system that reacted to pause should also react to resume to flip its state back.
