using UnityEngine;
using TMPro;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.PauseSystem
{
    /// <summary>
    /// On-screen HUD that displays live pause system state for the demo scene.
    /// Uses Time.unscaledDeltaTime for its own update so it stays readable while paused.
    ///
    /// Setup — assign in Inspector:
    ///   timeScaleText      — shows current Time.timeScale
    ///   isPausedText       — shows IsPaused state
    ///   storedScaleText    — shows the timeScale that will be restored on resume
    ///   audioPausedText    — shows AudioListener.pause state
    ///   lastEventText      — shows the last EventBus event fired
    ///
    /// Buttons (wire OnClick in Inspector):
    ///   OnActivateSlowMoClicked() — sets Time.timeScale to 0.2 to simulate bullet time
    ///   OnPauseClicked()          — calls PauseSystem.Pause()
    ///   OnResumeClicked()         — calls PauseSystem.Resume()
    ///   (Escape key calls TogglePause() automatically via PauseSystem_UMFOSS)
    /// </summary>
    public class PauseDemoHUD : MonoBehaviour
    {
        [Header("Status Text")]
        [SerializeField] private TMP_Text timeScaleText;
        [SerializeField] private TMP_Text isPausedText;
        [SerializeField] private TMP_Text storedScaleText;
        [SerializeField] private TMP_Text audioPausedText;
        [SerializeField] private TMP_Text lastEventText;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void OnEnable()
        {
            EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
        }

        private void Update()
        {
            // Uses unscaledDeltaTime implicitly — we're just reading values, not moving.
            // Text.text assignment works regardless of timeScale.
            if (PauseSystem_UMFOSS.Instance == null) return;

            SetText(timeScaleText,   $"Time.timeScale: {Time.timeScale:F2}");
            SetText(isPausedText,    $"IsPaused: {PauseSystem_UMFOSS.Instance.IsPaused}");
            SetText(storedScaleText, $"Stored timeScale: {PauseSystem_UMFOSS.Instance.GetPausedTimeScale():F2}");
            SetText(audioPausedText, $"AudioListener.pause: {AudioListener.pause}");
        }

        // ── Button Handlers ──────────────────────────────────────────────────────

        /// <summary>Sets timeScale to 0.2 to simulate bullet time. Then pause to verify stored scale is 0.2.</summary>
        public void OnActivateSlowMoClicked()
        {
            Time.timeScale = 0.2f;
            SetText(lastEventText, "Slow Mo activated (timeScale = 0.20)");
        }

        /// <summary>Calls Pause() directly. Does nothing if already paused.</summary>
        public void OnPauseClicked()
        {
            PauseSystem_UMFOSS.Instance.Pause();
        }

        /// <summary>Calls Resume() directly. Does nothing if not paused.</summary>
        public void OnResumeClicked()
        {
            PauseSystem_UMFOSS.Instance.Resume();
        }

        // ── Event Handlers ───────────────────────────────────────────────────────

        private void OnGamePaused(GamePausedEvent e)
        {
            SetText(lastEventText, $"GamePausedEvent (prev scale: {e.previousTimeScale:F2})");
        }

        private void OnGameResumed(GameResumedEvent e)
        {
            SetText(lastEventText, $"GameResumedEvent (restored scale: {e.restoredTimeScale:F2})");
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SetText(TMP_Text textComponent, string value)
        {
            if (textComponent != null)
                textComponent.text = value;
        }
    }
}
