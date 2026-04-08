using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Centralised pause system. One instance, persistent across scenes.
    ///
    /// Responsibilities:
    ///   - Store Time.timeScale before pausing, restore it exactly on resume
    ///     (compatible with bullet time — never hardcodes 1.0)
    ///   - Pause all audio globally via AudioListener.pause
    ///   - Auto-pause on application focus loss (optional)
    ///   - Fire EventBus events so every other system can react without coupling
    ///
    /// This system knows nothing about PlayerController, EnemyFSM, UI, or timers.
    /// Those systems subscribe to GamePausedEvent / GameResumedEvent independently.
    /// </summary>
    public class PauseSystem_UMFOSS : MonoSingletongeneric<PauseSystem_UMFOSS>
    {
        [Header("Pause Settings")]
        [Tooltip("Key that toggles pause. Default: Escape.")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

        [Tooltip("Auto-pause when the application loses focus (alt-tab, app switch). " +
                 "Recommended for desktop games. Disable for mobile.")]
        [SerializeField] private bool toggleOnFocusLoss = true;

        [Header("Audio")]
        [Tooltip("Pause all audio via AudioListener.pause. " +
                 "Set false if you want music to continue playing during pause.")]
        [SerializeField] private bool pauseAudio = true;

        [Header("Debug")]
        [Tooltip("Log pause/resume state changes to the console during development.")]
        [SerializeField] private bool logStateChanges = false;

        // ── Private State ────────────────────────────────────────────────────────

        private float storedTimeScale = 1f;
        private bool  isPaused        = false;

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Read-only current pause state. Query this without subscribing to events.</summary>
        public bool IsPaused => isPaused;

        /// <summary>
        /// Pause the game.
        /// Stores current Time.timeScale, sets it to 0, optionally pauses audio,
        /// and fires GamePausedEvent.
        /// Does nothing if already paused — no duplicate event is fired.
        /// </summary>
        public void Pause()
        {
            if (isPaused) return;

            storedTimeScale  = Time.timeScale;   // capture whatever is active (could be bullet time)
            Time.timeScale   = 0f;
            isPaused         = true;

            if (pauseAudio)
                AudioListener.pause = true;

            if (logStateChanges)
                Debug.Log($"[PauseSystem] Paused. Stored timeScale: {storedTimeScale}");

            EventBus.Publish(new GamePausedEvent { previousTimeScale = storedTimeScale });
        }

        /// <summary>
        /// Resume the game.
        /// Restores Time.timeScale to the stored value — NOT hardcoded 1.0,
        /// so slow motion and bullet time are fully preserved.
        /// Fires GameResumedEvent.
        /// Does nothing if not paused — no duplicate event is fired.
        /// </summary>
        public void Resume()
        {
            if (!isPaused) return;

            Time.timeScale   = storedTimeScale;  // restore exactly — not 1.0
            isPaused         = false;

            if (pauseAudio)
                AudioListener.pause = false;

            if (logStateChanges)
                Debug.Log($"[PauseSystem] Resumed. Restored timeScale: {storedTimeScale}");

            EventBus.Publish(new GameResumedEvent { restoredTimeScale = storedTimeScale });
        }

        /// <summary>
        /// Toggle pause state.
        /// This is the only method that should be called from input — not Pause() or Resume() directly.
        /// </summary>
        public void TogglePause()
        {
            if (isPaused) Resume();
            else          Pause();
        }

        /// <summary>
        /// Returns the timeScale stored at the moment the game was last paused.
        /// Useful for systems that need to know whether slow motion was active.
        /// </summary>
        public float GetPausedTimeScale() => storedTimeScale;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
                TogglePause();
        }

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
                // The player must press the pause key to resume.
                // Auto-resume would catch players off-guard after alt-tab.
            }
        }
    }
}
