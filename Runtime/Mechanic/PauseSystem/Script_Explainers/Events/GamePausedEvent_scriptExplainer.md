# GamePausedEvent — Line-by-Line Script Explainer

- **Script:** `Events/GamePausedEvent.cs`
- **Author:** [Souvik Kumar](https://github.com/Souvik-Cyclic)
- **Namespace:** `GameplayMechanicsUMFOSS.Systems`

---

```csharp
namespace GameplayMechanicsUMFOSS.Systems
```
**Explanation:** Places the event struct in the `Systems` namespace alongside `PauseSystem_UMFOSS`. Subscribers import this namespace to listen.

```csharp
public struct GamePausedEvent
```
**Explanation:** Declared as a `struct` (value type) — zero GC allocation when broadcast through the EventBus. Pause/resume can fire many times per session, so avoiding heap allocations matters.

```csharp
public float previousTimeScale;
```
**Explanation:** Carries the `Time.timeScale` that was active **before** pausing. Critical for the store-and-restore pattern — could be `1.0` (normal), `0.3` (bullet time), or any custom slow-mo value. Subscribers that depend on time scale can read this instead of caching their own copy.

---

## Role in the system

Fired by `PauseSystem_UMFOSS.Pause()` after `Time.timeScale` is set to 0 and audio is muted. Decouples the pause source from every reactor: PlayerController disables input, EnemyFSM freezes ticks, PauseMenuUI shows panel, SceneManager blocks loads — none of them hold a reference to `PauseSystem_UMFOSS`.
