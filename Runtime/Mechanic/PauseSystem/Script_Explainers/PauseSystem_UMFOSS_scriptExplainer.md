# PauseSystem_UMFOSS — Line-by-Line Script Explainer

- **Script:** `PauseSystem_UMFOSS.cs`
- **Author:** [Souvik Kumar](https://github.com/Souvik-Cyclic)
- **Namespace:** `GameplayMechanicsUMFOSS.Systems`

## Purpose

A centralised singleton pause system. Freezes gameplay by setting `Time.timeScale` to `0`, pauses all audio globally via `AudioListener.pause`, and broadcasts events so every other system can react without coupling to this one. Supports a configurable pause key, optional auto-pause on application focus loss, and the **store-and-restore `timeScale`** pattern so bullet time and slow motion survive a pause/resume cycle.

---

## Imports & Namespace

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity's core namespace. Required for `MonoBehaviour`, `Time`, `Input`, `AudioListener`, `KeyCode`, `SerializeField`, and `Debug`.

```csharp
using GameplayMechanicsUMFOSS.Core;
```
**Explanation:** Imports the framework's Core namespace to access `MonoSingletongeneric<T>` (enforces single instance) and `EventBus` (decoupled event broadcasting).

```csharp
namespace GameplayMechanicsUMFOSS.Systems
```
**Explanation:** Places this script in the `Systems` feature group. Keeps pause logic separate from `Core`, `Movement`, and other feature groups, and prevents naming conflicts.

---

## Class Declaration

```csharp
public class PauseSystem_UMFOSS : MonoSingletongeneric<PauseSystem_UMFOSS>
```
**Explanation:** Inherits `MonoSingletongeneric<T>` so only one `PauseSystem` can ever exist at runtime. If a second instance is created (e.g. on scene load), the base class destroys it automatically. Pause state stays consistent across all scenes without `DontDestroyOnLoad` boilerplate in this file.

---

## Inspector Fields

```csharp
[SerializeField] private KeyCode pauseKey = KeyCode.Escape;
```
**Explanation:** Exposes the pause key in the Inspector while keeping it `private` in code. Any project can remap it without editing the script — input contract lives in the Inspector, not in source.

```csharp
[SerializeField] private bool toggleOnFocusLoss = true;
```
**Explanation:** When `true`, the game auto-pauses when the OS takes focus away (alt-tab, app switch). Default `true` for desktop. Disable on mobile where focus loss is normal lifecycle.

```csharp
[SerializeField] private bool pauseAudio = true;
```
**Explanation:** Toggles whether `AudioListener.pause` is set during pause. Disable if music should keep playing through the pause menu (common UX choice).

```csharp
[SerializeField] private bool logStateChanges = false;
```
**Explanation:** Debug-only console logging of pause/resume transitions. Off by default to keep production logs clean.

---

## Private State

```csharp
private float storedTimeScale = 1f;
```
**Explanation:** Captures `Time.timeScale` at the moment of pause. **Never hardcoded back to `1.0` on resume** — this is what makes bullet time and slow motion survive a pause cycle.

```csharp
private bool isPaused = false;
```
**Explanation:** Single source of truth for pause state. Guards against duplicate `Pause()` / `Resume()` calls that would fire spurious events.

---

## Public API

```csharp
public bool IsPaused => isPaused;
```
**Explanation:** Read-only accessor. Other systems can poll pause state without subscribing to events — useful for one-shot checks (e.g. UI button click handlers).

### `Pause()`

```csharp
public void Pause()
{
    if (isPaused) return;

    storedTimeScale  = Time.timeScale;
    Time.timeScale   = 0f;
    isPaused         = true;

    if (pauseAudio)
        AudioListener.pause = true;

    if (logStateChanges)
        Debug.Log($"[PauseSystem] Paused. Stored timeScale: {storedTimeScale}");

    EventBus.Publish(new GamePausedEvent { previousTimeScale = storedTimeScale });
}
```
**Explanation:**
- **Early return** when already paused — prevents duplicate `GamePausedEvent` broadcasts.
- **Captures `Time.timeScale` first**, *then* sets it to `0`. Order matters: if bullet time was active at `0.3`, that value is preserved for restore.
- Audio mute is opt-in via `pauseAudio`.
- `EventBus.Publish` carries the **previous** time scale — subscribers that need to know the pre-pause speed can read it from the event payload, no extra coupling.

### `Resume()`

```csharp
public void Resume()
{
    if (!isPaused) return;

    Time.timeScale   = storedTimeScale;
    isPaused         = false;

    if (pauseAudio)
        AudioListener.pause = false;

    if (logStateChanges)
        Debug.Log($"[PauseSystem] Resumed. Restored timeScale: {storedTimeScale}");

    EventBus.Publish(new GameResumedEvent { restoredTimeScale = storedTimeScale });
}
```
**Explanation:**
- **Restores `storedTimeScale`, not `1.0`** — the entire point of the store-and-restore pattern.
- Symmetric with `Pause()`: same guard, same audio toggle, same logging shape, same event-publish step.
- `restoredTimeScale` in the event payload equals the matching `previousTimeScale` from the prior `GamePausedEvent`.

### `TogglePause()`

```csharp
public void TogglePause()
{
    if (isPaused) Resume();
    else          Pause();
}
```
**Explanation:** The **only** method input should call. UI buttons and the keyboard handler both route through here so `isPaused` stays the single source of truth — no caller needs to know the current state.

### `GetPausedTimeScale()`

```csharp
public float GetPausedTimeScale() => storedTimeScale;
```
**Explanation:** Lets external systems inspect the stored time scale (e.g. a slow-mo HUD that wants to show "Bullet Time" if `storedTimeScale < 1`). Read-only — modification still goes through `Pause()` / `Resume()`.

---

## Unity Lifecycle

### `Update()`

```csharp
private void Update()
{
    if (Input.GetKeyDown(pauseKey))
        TogglePause();
}
```
**Explanation:** Polls the pause key every frame. Routes through `TogglePause()` rather than calling `Pause()` / `Resume()` directly so the keyboard path matches the UI button path exactly.

### `OnApplicationFocus(bool hasFocus)`

```csharp
private void OnApplicationFocus(bool hasFocus)
{
    if (!toggleOnFocusLoss) return;

    if (!hasFocus)
    {
        EventBus.Publish(new ApplicationFocusLostEvent());
        if (!isPaused) Pause();
    }
    else
    {
        EventBus.Publish(new ApplicationFocusGainedEvent());
        // Deliberately NOT auto-resuming on focus return.
    }
}
```
**Explanation:**
- Early-out when the feature is disabled — no events fire when the user has opted out, so analytics aren't polluted either.
- **Loss path:** broadcast first, then pause. Order matters — pure analytics listeners get the focus-loss event even when already paused.
- **Gain path:** broadcast only. **Deliberate non-symmetry** — auto-resuming on focus return would catch players off-guard right after they alt-tab back. The player presses the pause key to resume.

---

## Patterns Demonstrated

- **Singleton (`MonoSingletongeneric<T>`)** — globally accessible pause state, lifecycle managed by the base class.
- **EventBus / Pub-Sub** — `PauseSystem` knows nothing about `PlayerController`, `EnemyFSM`, UI, timers. Subscribers attach independently.
- **Store-and-restore** — `timeScale` capture before mutation, exact restore on resume. Composes cleanly with bullet time and slow motion.
- **Single source of truth** — `isPaused` guards every public mutator; `TogglePause()` is the single entry point for input.
