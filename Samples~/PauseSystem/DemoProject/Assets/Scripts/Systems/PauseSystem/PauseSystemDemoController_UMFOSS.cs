using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;
using UnityEngine.UI;

public class PauseSystemDemoController_UMFOSS : MonoBehaviour
{
    [Header("Optional UI Bindings")]
    [SerializeField] private Text timeScaleText;
    [SerializeField] private Text isPausedText;
    [SerializeField] private Text storedTimeScaleText;
    [SerializeField] private Text audioPauseText;
    [SerializeField] private Text lastEventText;

    private void OnEnable()
    {
        GameplayMechanicsUMFOSS.Core.EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
        GameplayMechanicsUMFOSS.Core.EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
        GameplayMechanicsUMFOSS.Core.EventBus.Subscribe<ApplicationFocusLostEvent>(OnFocusLost);
        GameplayMechanicsUMFOSS.Core.EventBus.Subscribe<ApplicationFocusGainedEvent>(OnFocusGained);
    }

    private void OnDisable()
    {
        GameplayMechanicsUMFOSS.Core.EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
        GameplayMechanicsUMFOSS.Core.EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
        GameplayMechanicsUMFOSS.Core.EventBus.Unsubscribe<ApplicationFocusLostEvent>(OnFocusLost);
        GameplayMechanicsUMFOSS.Core.EventBus.Unsubscribe<ApplicationFocusGainedEvent>(OnFocusGained);
    }

    private void Update()
    {
        RefreshUi();
    }

    public void ActivateSlowMo()
    {
        if (PauseSystem_UMFOSS.Instance != null && PauseSystem_UMFOSS.Instance.IsPaused)
        {
            return;
        }

        Time.timeScale = 0.2f;
        SetLastEvent("SlowMo Activated (0.2)");
    }

    public void PauseFromButton()
    {
        if (PauseSystem_UMFOSS.Instance == null)
        {
            return;
        }

        PauseSystem_UMFOSS.Instance.Pause();
    }

    public void ResumeFromButton()
    {
        if (PauseSystem_UMFOSS.Instance == null)
        {
            return;
        }

        PauseSystem_UMFOSS.Instance.Resume();
    }

    public void QuitApplication()
    {
        Application.Quit();
    }

    private void OnGamePaused(GamePausedEvent evt)
    {
        SetLastEvent($"GamePausedEvent (prev: {evt.previousTimeScale:0.###})");
    }

    private void OnGameResumed(GameResumedEvent evt)
    {
        SetLastEvent($"GameResumedEvent (restored: {evt.restoredTimeScale:0.###})");
    }

    private void OnFocusLost(ApplicationFocusLostEvent _)
    {
        SetLastEvent("ApplicationFocusLostEvent");
    }

    private void OnFocusGained(ApplicationFocusGainedEvent _)
    {
        SetLastEvent("ApplicationFocusGainedEvent");
    }

    private void RefreshUi()
    {
        PauseSystem_UMFOSS pauseSystem = PauseSystem_UMFOSS.Instance;

        if (timeScaleText != null)
        {
            timeScaleText.text = $"Time.timeScale: {Time.timeScale:0.###}";
        }

        if (isPausedText != null)
        {
            isPausedText.text = $"IsPaused: {(pauseSystem != null && pauseSystem.IsPaused)}";
        }

        if (storedTimeScaleText != null)
        {
            float stored = pauseSystem != null ? pauseSystem.GetPausedTimeScale() : 1f;
            storedTimeScaleText.text = $"Stored timeScale: {stored:0.###}";
        }

        if (audioPauseText != null)
        {
            audioPauseText.text = $"AudioListener.pause: {AudioListener.pause}";
        }
    }

    private void SetLastEvent(string value)
    {
        if (lastEventText != null)
        {
            lastEventText.text = $"Last Event: {value}";
        }
    }
}
