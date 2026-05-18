# ApplicationFocusLostEvent — Line-by-Line Script Explainer

- **Script:** `Events/ApplicationFocusLostEvent.cs`
- **Author:** [Souvik Kumar](https://github.com/Souvik-Cyclic)
- **Namespace:** `GameplayMechanicsUMFOSS.Systems`

---

```csharp
namespace GameplayMechanicsUMFOSS.Systems
```
**Explanation:** Shared namespace with `PauseSystem_UMFOSS`. Subscribers import this once.

```csharp
public struct ApplicationFocusLostEvent { }
```
**Explanation:** Empty struct — pure signal, no payload. Value type means zero GC. Fired the moment the OS reports focus loss (alt-tab, window switch, mobile app backgrounded).

---

## Role in the system

Broadcast by `PauseSystem_UMFOSS.OnApplicationFocus(false)` **before** any auto-pause logic runs. Order matters: analytics or telemetry systems can record the focus-loss as a distinct event, even when `toggleOnFocusLoss` is disabled and no pause actually happens.
