using UnityEngine;
using UnityEngine.UI;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Samples.TimerUtility
{
    /// <summary>
    /// Demo 2: Countdown Display (Round Timer)
    /// A 30-second round timer that updates a UI Text label on every Tick.
    /// Fires a "Round Over!" message when the timer completes.
    /// Attach to a GameObject in the scene. Assign the Text and TimerUtility references.
    /// </summary>
    public class CountdownDisplayDemo : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Text               countdownText;
        [SerializeField] private Text               statusText;
        [SerializeField] private TimerUtility_UMFOSS roundTimer;

        private void Start()
        {
            roundTimer.OnTimerStart    += OnRoundStart;
            roundTimer.OnTimerTick     += OnRoundTick;
            roundTimer.OnTimerComplete += OnRoundEnd;

            // Auto-start the round timer
            roundTimer.Start();
        }

        private void OnDestroy()
        {
            roundTimer.OnTimerStart    -= OnRoundStart;
            roundTimer.OnTimerTick     -= OnRoundTick;
            roundTimer.OnTimerComplete -= OnRoundEnd;
        }

        // Update is used only for smooth display between ticks
        private void Update()
        {
            if (roundTimer.IsRunning())
                countdownText.text = $"{roundTimer.GetTimeRemaining():F1}s";
        }

        private void OnRoundStart()
        {
            statusText.text    = "Round in progress...";
            countdownText.text = $"{roundTimer.GetTimeRemaining():F1}s";
        }

        private void OnRoundTick(float timeRemaining)
        {
            // Tick fires every 1 second — update the display with a whole number
            countdownText.text = $"{Mathf.CeilToInt(timeRemaining)}s";
        }

        private void OnRoundEnd()
        {
            countdownText.text = "0s";
            statusText.text    = "ROUND OVER!";
        }
    }
}
