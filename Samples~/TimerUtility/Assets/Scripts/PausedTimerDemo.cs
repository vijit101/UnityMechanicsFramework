using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Samples.TimerUtility
{
    /// <summary>
    /// Demo 4: Paused Timer with Unscaled Time — fully self-contained, creates its own UI.
    /// Pause/Resume via on-screen buttons. Freeze Time sets timeScale=0 to prove
    /// that UseUnscaledTime keeps the timer running regardless.
    /// Just attach this script to any GameObject. Press Play. No wiring needed.
    /// </summary>
    public class PausedTimerDemo : MonoBehaviour
    {
        private TimerUtility_UMFOSS pauseableTimer;
        private TMP_Text            timerText;
        private TMP_Text            statusText;
        private TMP_Text            timeScaleText;
        private Button              pauseBtn;
        private Button              resumeBtn;

        private const float TIMER_DURATION = 60f;

        private void Awake()
        {
            BuildUI();

            pauseableTimer             = gameObject.AddComponent<TimerUtility_UMFOSS>();
            pauseableTimer.SetDuration(TIMER_DURATION, false);

            // KEY: This is what makes the timer ignore Time.timeScale.
            // Without this line, freezing time would also freeze this timer.
            // With it, the timer keeps ticking even when Time.timeScale = 0.
            pauseableTimer.UseUnscaledTime = true;
        }

        private void Start()
        {
            pauseableTimer.OnTimerStart    += () => SetStatus("Running");
            pauseableTimer.OnTimerPaused   += () => SetStatus("PAUSED");
            pauseableTimer.OnTimerResumed  += () => SetStatus("Running");
            pauseableTimer.OnTimerComplete += () => SetStatus("Complete!");

            pauseBtn.onClick.AddListener(pauseableTimer.Pause);
            resumeBtn.onClick.AddListener(pauseableTimer.Resume);

            pauseableTimer.Start();
        }

        private void OnDestroy()
        {
            Time.timeScale = 1f; // always restore on exit
        }

        private void Update()
        {
            timerText.text     = $"Time Remaining: {pauseableTimer.GetTimeRemaining():F1}s";
            timeScaleText.text = $"Time.timeScale = {Time.timeScale:F1}";
        }

        private void SetStatus(string status)
        {
            if (statusText != null) statusText.text = $"Status: {status}";
        }

        private void BuildUI()
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                var cgo = new GameObject("Canvas");
                canvas = cgo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                cgo.AddComponent<CanvasScaler>();
                cgo.AddComponent<GraphicRaycaster>();
            }

            // Title
            CreateLabel(canvas.transform, "Demo4Title",
                "DEMO 4 — Pause / Unscaled Timer", new Vector2(-700f, -200f), 20);

            // Timer display
            timerText = CreateLabel(canvas.transform, "TimerText",
                "Time Remaining: --", new Vector2(-700f, -270f), 22);
            timerText.color = new Color(1f, 0.5f, 0.2f);

            // Status
            statusText = CreateLabel(canvas.transform, "StatusText",
                "Status: --", new Vector2(-700f, -330f), 18);

            // timeScale display
            timeScaleText = CreateLabel(canvas.transform, "TimeScaleText",
                "Time.timeScale = 1.0", new Vector2(-700f, -390f), 16);
            timeScaleText.color = Color.yellow;

            // Pause button
            pauseBtn = CreateButton(canvas.transform, "PauseBtn", "PAUSE",
                new Vector2(-800f, -450f), new Color(0.9f, 0.6f, 0.1f));

            // Resume button
            resumeBtn = CreateButton(canvas.transform, "ResumeBtn", "RESUME",
                new Vector2(-630f, -450f), new Color(0.2f, 0.8f, 0.3f));

            // Freeze time button
            var freezeBtn = CreateButton(canvas.transform, "FreezeBtn", "FREEZE TIME",
                new Vector2(-800f, -510f), new Color(0.4f, 0.2f, 0.8f));
            freezeBtn.onClick.AddListener(() =>
            {
                Time.timeScale = 0f;
                Debug.Log("[Demo4] timeScale=0. Unscaled timer keeps ticking!");
            });

            // Unfreeze button
            var unfreezeBtn = CreateButton(canvas.transform, "UnfreezeBtn", "UNFREEZE",
                new Vector2(-630f, -510f), new Color(0.2f, 0.5f, 0.8f));
            unfreezeBtn.onClick.AddListener(() => Time.timeScale = 1f);
        }

        private Button CreateButton(Transform parent, string name, string label,
                                    Vector2 position, Color color)
        {
            var go   = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img  = go.AddComponent<Image>();
            img.color = color;
            var btn  = go.AddComponent<Button>();
            var rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta        = new Vector2(130f, 40f);

            var txtGO = new GameObject("Label");
            txtGO.transform.SetParent(go.transform, false);
            var tmp           = txtGO.AddComponent<TextMeshProUGUI>();
            tmp.text          = label;
            tmp.fontSize      = 14;
            tmp.color         = Color.white;
            tmp.alignment     = TextAlignmentOptions.Center;
            var txtRect       = txtGO.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.sizeDelta = Vector2.zero;
            return btn;
        }

        private TMP_Text CreateLabel(Transform parent, string name, string text,
                                     Vector2 position, float fontSize)
        {
            var go            = new GameObject(name);
            go.transform.SetParent(parent, false);
            var tmp           = go.AddComponent<TextMeshProUGUI>();
            tmp.text          = text;
            tmp.fontSize      = fontSize;
            tmp.color         = Color.white;
            tmp.alignment     = TextAlignmentOptions.Center;
            var rect          = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta    = new Vector2(280f, 40f);
            return tmp;
        }
    }
}
