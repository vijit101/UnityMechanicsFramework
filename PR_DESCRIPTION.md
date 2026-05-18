# [Mechanic] Add Scene Manager System

Closes #21

## Mechanic Name

**Scene Manager System** — `GameplayMechanicsUMFOSS.Systems`

## What does it do?

A centralized async scene management system that solves four real-world problems with Unity's built-in `SceneManager`:

1. **Main-thread blocking** — every load is async with `LoadSceneMode.Additive` and `allowSceneActivation = false` until 90% so there's no freeze and no half-loaded flashes
2. **Singleton destruction** — a persistent scene pattern keeps `AudioManager`, `SaveSystem`, HUD, and the SceneManager itself alive across every transition
3. **Missing fade transitions** — a fade canvas is created automatically on `Awake` (zero manual UI setup), driven by `SceneTransition_UMFOSS` ScriptableObjects with configurable colour, durations and curves
4. **No support for overlay scenes** — `Push` / `Pop` API for additive overlays (pause menus, inventory, settings) on top of the active gameplay scene

The system uses the `EventBus` so other mechanics can react to scene transitions without holding a direct reference. Seven events fire across the load lifecycle: `SceneLoadStartEvent`, `SceneLoadProgressEvent`, `SceneLoadCompleteEvent`, `ScenePushedEvent`, `ScenePoppedEvent`, `SceneReloadedEvent`, `InputLockEvent`.

## How to test it

1. Extract `Samples~/SceneManagerSample/SceneManagerSystemCompleteProject.zip` somewhere on disk and open the resulting folder in **Unity 2021.3 LTS or later**
2. Open `Assets/Scenes/PersistentScene.unity`
3. Confirm every scene under `Assets/Scenes/` is in **File → Build Settings** with `PersistentScene` at index `0` (the zip already ships this configured)
4. Press **Play** in `PersistentScene`
5. Click **START GAME** on the title screen
6. The **SLITHER** snake game runs end-to-end across three levels:
   - **Level 01 — The Garden** (5 strawberries to win, no bombs)
   - **Level 02 — Cyber Grid** (8 strawberries, 1 bomb that relocates every few seconds)
   - **Level 03 — The Void** (12 strawberries, 3 bombs reshuffling continuously)
7. **Controls:** WASD or Arrow keys to move, **Esc** to pause
8. **What to verify on the live demo:**
   - The fade-to-black transition between every level (proves `LoadScene` + `SceneTransition`)
   - The loading screen with progress bar and rotating tip text between levels (proves `SceneLoadProgressEvent` + `minimumLoadTime`)
   - Press **Esc** mid-level → pause overlay slides on top of the gameplay scene without unloading it (proves **Push**)
   - From the pause menu, click **STATS** → a second overlay appears on top of the pause menu, stack depth shows **3** (proves **push-on-push**)
   - Click **Restart Level** in pause → current level reloads cleanly (proves **ReloadScene**)
   - Click **Quit to Title** in pause → the entire stack clears and the title screen loads (proves `LoadScene` clears the stack correctly)
   - Walk into a bomb or a wall → red screen flash → game over scene loads
   - **Bottom-right HUD shows the live EventBus log** — every one of the 7 events fires in real time as you play
   - **Top-left HUD shows the persistent counter** — it never resets across any transition (proves the persistent scene pattern)
   - Test pausing while the loading bar is showing — the loading screen continues to fade in/out correctly because the system uses `WaitForSecondsRealtime` (immune to `Time.timeScale = 0`)

## Demo Video

▶ **Walkthrough videos** are bundled inside the repo at `Samples~/SceneManagerSample/SceneManagerVideos.zip`:
- `SceneManagerDemo_Basic.mp4` — basic API walkthrough
- `SceneManagerDemo_Slither.mp4` — full SLITHER game demo
- `SceneManagerDemo_Walkthrough.mp4` — scripts overview, prefab setup from scratch, and end-to-end mechanic flow

## Namespace used

`GameplayMechanicsUMFOSS.Systems`

## Folder structure

```
Runtime/Systems/1. SceneManagerSystem/
├── Scripts/
│   ├── SceneManager_UMFOSS.cs        ← main coordinator + auto fade canvas + scene validation
│   ├── SceneTransition_UMFOSS.cs     ← ScriptableObject (fade colour, durations, loading screen flag)
│   ├── PersistentScene_UMFOSS.cs     ← marks a scene as never-unload
│   ├── LoadingScreen_UMFOSS.cs       ← optional loading screen UI subscriber
│   ├── SceneEvents.cs                ← all 7 event structs
│   └── EventBus.cs                   ← shared static pub/sub used by the events
└── Script_Explainers/                ← one line-by-line explainer per script
    ├── SceneManager_UMFOSS_Explainer.txt
    ├── SceneTransition_UMFOSS_Explainer.txt
    ├── PersistentScene_UMFOSS_Explainer.txt
    ├── LoadingScreen_UMFOSS_Explainer.txt
    ├── SceneEvents_Explainer.txt
    └── EventBus_Explainer.txt

Samples~/SceneManagerSample/
├── SceneManagerSystemCompleteProject.zip   ← complete standalone Unity project (Assets/ + ProjectSettings/ + Packages/)
└── SceneManagerVideos.zip                  ← demo + walkthrough videos
```

## README entry

A new mechanic card has been added at `README.md` Section 6 (Mechanics Library) as entry **#3 — Scene Manager System**, plus a new row in the Quick Navigation table linking to the video files in the repo.

## Acceptance criteria coverage (from Issue #21)

| Criterion | Met? | Where |
|---|:---:|---|
| `LoadScene` fades out → loads async → fades in (no main-thread freeze) | ✅ | `SceneManager_UMFOSS.LoadSceneRoutine` |
| `Push` loads additively, current scene remains beneath | ✅ | `PushRoutine` |
| `Pop` unloads top scene, scene below resumes | ✅ | `PopRoutine` |
| `ReloadScene` reloads current, persistent scene untouched | ✅ | `ReloadRoutine` + `Restart Level` button in PauseMenu |
| Singletons in persistent scene survive every load | ✅ | `GameStats` + persistent counter in HUD |
| `isTransitioning` guard prevents double-load on rapid clicks | ✅ | `GuardCanTransition` |
| `SceneLoadProgressEvent` fires with normalized 0–1 progress | ✅ | Loading screen progress bar |
| `allowSceneActivation = false` until 90% | ✅ | `LoadSceneRoutine` step 5–6 |
| `minimumLoadTime` holds loading screen open | ✅ | `LoadingBar.asset` transition uses `minimumLoadTime = 1.5f` |
| All seven events fire with correct data | ✅ | Live `EventLogPanel` in the HUD shows them in real time |
| Scene name not in Build Settings logs clear error, no crash | ✅ | `IsSceneInBuildSettings` validation |
| `WaitForSecondsRealtime` works with `Time.timeScale = 0` | ✅ | Pause uses `Time.timeScale = 0`, fades + loading still animate correctly |
| Fade canvas auto-created on Awake | ✅ | `CreateFadeCanvas()` in `SceneManager_UMFOSS.Awake` |
| Three default transitions shipped (Instant, FadeBlack, FadeWhite) | ✅ | Inside `SceneManagerSystemCompleteProject.zip` → `Assets/SceneTransitions/` (plus `LoadingBar.asset` for the loading-screen variant) |
| Per-script explainer included | ✅ | `Runtime/Systems/1. SceneManagerSystem/Script_Explainers/` (one file per script) |
| Demo scene shows all four operations working | ✅ | SLITHER demo: LoadScene (level → level), Push (Esc pause), Pop (Resume), Reload (Restart Level) |
| Demo video included | ✅ | `Samples~/SceneManagerSample/SceneManagerVideos.zip` |
| Video link in README | ✅ | Quick Nav row + mechanic card (relative repo path) |
| README Quick Navigation row added | ✅ | `README.md` |
| README full mechanic card added | ✅ | `README.md` |

## Checklist

- [x] Compiles with zero errors and zero warnings
- [x] Folder structure followed (`Runtime/Systems/1. SceneManagerSystem/` for code, `Samples~/SceneManagerSample/` for demo)
- [x] Namespace `GameplayMechanicsUMFOSS.Systems`
- [x] No magic numbers, no direct cross-mechanic dependencies
- [x] Public APIs have XML `<summary>` documentation
- [x] One Script Explainer per script in `Runtime/Systems/1. SceneManagerSystem/Script_Explainers/`
- [x] Demo scene runs immediately on Play with no missing references
- [x] Three default transition assets shipped (Instant, FadeBlack, FadeWhite) — plus a fourth (LoadingBar) for the loading screen variant
- [x] Walkthrough videos zipped inside repo at `Samples~/SceneManagerSample/SceneManagerVideos.zip` (no external links)
- [x] Complete project ZIP included at `Samples~/SceneManagerSample/SceneManagerSystemCompleteProject.zip` (contains `Assets/`, `ProjectSettings/`, `Packages/` only)
- [x] README Quick Navigation row added with working anchor and video link
- [x] README full mechanic card added (metadata table, what it does, code example, highlights)
