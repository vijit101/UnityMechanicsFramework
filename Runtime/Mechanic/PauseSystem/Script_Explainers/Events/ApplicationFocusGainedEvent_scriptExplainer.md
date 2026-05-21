# ApplicationFocusGainedEvent — Line-by-Line Script Explainer

- **Script:** `Events/ApplicationFocusGainedEvent.cs`
- **Author:** [Souvik Kumar](https://github.com/Souvik-Cyclic)
- **Namespace:** `GameplayMechanicsUMFOSS.Systems`

---

```csharp
namespace GameplayMechanicsUMFOSS.Systems
```
**Explanation:** Same namespace as the rest of the pause module.

```csharp
public struct ApplicationFocusGainedEvent { }
```
**Explanation:** Empty payload-free struct, value type, zero GC. Pure notification — fired when the OS hands focus back to the application.

---

## Role in the system

Broadcast by `PauseSystem_UMFOSS.OnApplicationFocus(true)`. Deliberate design choice: PauseSystem **does not** auto-resume when focus returns — the player must press the pause key to come back. This event lets other systems (analytics, idle-detection, "welcome back" UI) react to focus return without coupling that behaviour to resume.
