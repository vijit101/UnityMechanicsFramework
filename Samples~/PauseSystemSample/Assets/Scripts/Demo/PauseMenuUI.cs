using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.PauseSystem
{
    /// <summary>
    /// Pause menu panel that slides in/out using Time.unscaledDeltaTime so it
    /// animates correctly even while Time.timeScale is 0.
    ///
    /// Setup:
    ///   - Attach to a UI Panel GameObject.
    ///   - Set hiddenAnchorY to a value off-screen (e.g. -800) and shownAnchorY to 0.
    ///   - The panel slides in when GamePausedEvent fires, slides out on GameResumedEvent.
    /// </summary>
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The RectTransform of the pause panel.")]
        [SerializeField] private RectTransform panelRect;

        [Header("Animation")]
        [Tooltip("Y anchor position when the panel is hidden (off-screen).")]
        [SerializeField] private float hiddenAnchorY = -800f;

        [Tooltip("Y anchor position when the panel is fully shown.")]
        [SerializeField] private float shownAnchorY = 0f;

        [Tooltip("Speed of the slide animation.")]
        [SerializeField] private float slideSpeed = 1800f;

        // ── Private State ────────────────────────────────────────────────────────

        private float targetY;
        private bool  isVisible = false;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            // Start hidden
            SetAnchorY(hiddenAnchorY);
            targetY = hiddenAnchorY;
        }

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
            // IMPORTANT: Use unscaledDeltaTime — Time.deltaTime is 0 when paused.
            // This is what makes the slide animation work during pause.
            float currentY = panelRect.anchoredPosition.y;
            float newY = Mathf.MoveTowards(currentY, targetY, slideSpeed * Time.unscaledDeltaTime);
            SetAnchorY(newY);
        }

        // ── Public Methods (called by UI buttons) ────────────────────────────────

        public void OnResumeButtonClicked()
        {
            PauseSystem_UMFOSS.Instance.Resume();
        }

        public void OnQuitButtonClicked()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }

        // ── Event Handlers ───────────────────────────────────────────────────────

        private void OnGamePaused(GamePausedEvent e)
        {
            targetY    = shownAnchorY;
            isVisible  = true;
        }

        private void OnGameResumed(GameResumedEvent e)
        {
            targetY    = hiddenAnchorY;
            isVisible  = false;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SetAnchorY(float y)
        {
            Vector2 pos = panelRect.anchoredPosition;
            pos.y = y;
            panelRect.anchoredPosition = pos;
        }
    }
}
