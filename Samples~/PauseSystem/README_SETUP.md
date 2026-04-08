# Pause System Sample Setup

This folder provides scripts and a setup checklist for the pause demo scene required by the mechanic issue.

## 1) Scene path

Create and save the scene at:

`Samples~/PauseSystem/Assets/Scenes/DemoScene.unity`

## 2) Required GameObjects

- `PauseSystem` object with `PauseSystem_UMFOSS`
- `DemoController` object with `PauseSystemDemoController_UMFOSS`
- One moving enemy object (simple translate patrol script is enough)
- One rotating object
- One particle system
- One AudioSource playing looping background music
- Canvas with:
  - Pause menu panel
  - Resume button wired to `PauseSystemDemoController_UMFOSS.ResumeFromButton()`
  - Quit button wired to `Application.Quit()`
  - Buttons:
    - Toggle pause by key (`Escape`) through `PauseSystem_UMFOSS`
    - Activate slow-mo wired to `PauseSystemDemoController_UMFOSS.ActivateSlowMo()`
    - Pause wired to `PauseSystemDemoController_UMFOSS.PauseFromButton()`
    - Resume wired to `PauseSystemDemoController_UMFOSS.ResumeFromButton()`
  - Text labels wired in `PauseSystemDemoController_UMFOSS`:
    - Current `Time.timeScale`
    - `IsPaused`
    - Stored `timeScale`
    - `AudioListener.pause`
    - Last event fired

## 3) Pause panel animation requirement

Attach `PauseMenuPanelAnimator_UMFOSS` to the pause panel and configure hidden/visible positions.
The script uses `Time.unscaledDeltaTime` so the panel still animates while paused.

## 4) Test sequence for reviewer

1. Press Play
2. Press slow-mo button (`Time.timeScale = 0.2`)
3. Press `Escape` to pause
4. Confirm:
   - Enemy/rotation/particles freeze
   - Audio pauses (if `pauseAudio` is true)
   - Pause panel animates in smoothly
   - Stored timeScale shows `0.2`
5. Press `Escape` to resume
6. Confirm `Time.timeScale` restores to `0.2`, not `1.0`

## 5) Video capture checklist

Record one continuous video showing:

- Inspector settings of `PauseSystem_UMFOSS`
- Normal pause/resume
- Slow-mo + pause + resume preservation
- Audio pause/resume behavior
- Focus-loss auto-pause (if on desktop)
- Pause panel animating while paused
- Event/status text changing live

After upload, add the final video URL to the root `README.md` Pause System entry.
