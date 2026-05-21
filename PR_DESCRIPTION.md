# [Mechanic] Implement Bullet Time / Slow Motion System

## Mechanic Name

**Bullet Time / Slow Motion System** — `GameplayMechanicsUMFOSS.Systems`

## What does it do?

This PR adds the original requested bullet time mechanic: a configurable slow motion singleton that smoothly changes `Time.timeScale`, keeps `Time.fixedDeltaTime` proportional for physics stability, adjusts audio pitch, drains and recharges a real-time resource, fires lifecycle events, and integrates cleanly with the existing Pause System so the two never fight over `Time.timeScale`.

## How to test it

1. Extract `Samples~/BulletTime/BulletTimeProject.zip`
2. Open the extracted project in **Unity 2021.3 LTS or later**
3. Press **Play** in the default sample scene
4. Use the on-screen buttons for `Subtle`, `Standard`, `Cinematic`, `Exit`, `Toggle`, and `Pause`
5. Watch the moving objects, particle systems, resource bar, current `timeScale`, current `fixedDeltaTime`, and unscaled UI timer update live
6. Confirm pause during active bullet time resumes back into the correct slow-motion state
7. Open `Samples~/BulletTime/BulletTimeVideos.zip` for the bundled walkthrough video

## Namespace used

`GameplayMechanicsUMFOSS.Systems`

## Folder structure

```text
Runtime/
└── Mechanic/
    └── BulletTimeSystem/
        ├── Scripts/
        ├── Script_Explainers/
        └── Configs/

Samples~/
└── BulletTime/
    ├── BulletTimeProject.zip
    └── BulletTimeVideos.zip
```

## README entry

The README now includes a quick-navigation row and a full mechanic card for Bullet Time / Slow Motion System, both linked to the new runtime folder and bundled sample/video ZIPs.

## Checklist

- [x] Bullet time runtime scripts added
- [x] Per-script explainers added
- [x] Three default SlowMo config assets added
- [x] PauseSystem integration hook added
- [x] Sample project ZIP added
- [x] Video ZIP added
- [x] README entry added
