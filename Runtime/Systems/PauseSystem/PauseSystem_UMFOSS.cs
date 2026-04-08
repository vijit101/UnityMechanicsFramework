using GameplayMechanicsUMFOSS.Core;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    public class PauseSystem_UMFOSS : MonoSingletongeneric<PauseSystem_UMFOSS>
    {
        [Header("Pause Settings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private bool toggleOnFocusLoss = true;

        [Header("Audio")]
        [SerializeField] private bool pauseAudio = true;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = false;

        private float storedTimeScale = 1f;
        private bool isPaused;

        public bool IsPaused => isPaused;

        public void Pause()
        {
            if (isPaused)
            {
                return;
            }

            storedTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            isPaused = true;

            if (pauseAudio)
            {
                AudioListener.pause = true;
            }

            if (logStateChanges)
            {
                Debug.Log($"[PauseSystem] Paused. Stored timeScale: {storedTimeScale}");
            }

            EventBus.Publish(new GamePausedEvent { previousTimeScale = storedTimeScale });
        }

        public void Resume()
        {
            if (!isPaused)
            {
                return;
            }

            Time.timeScale = storedTimeScale;
            isPaused = false;

            if (pauseAudio)
            {
                AudioListener.pause = false;
            }

            if (logStateChanges)
            {
                Debug.Log($"[PauseSystem] Resumed. Restored timeScale: {storedTimeScale}");
            }

            EventBus.Publish(new GameResumedEvent { restoredTimeScale = storedTimeScale });
        }

        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
                return;
            }

            Pause();
        }

        public float GetPausedTimeScale()
        {
            return storedTimeScale;
        }

        private void Update()
        {
            if (Input.GetKeyDown(pauseKey))
            {
                TogglePause();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!toggleOnFocusLoss)
            {
                return;
            }

            if (!hasFocus)
            {
                EventBus.Publish(new ApplicationFocusLostEvent());
                if (!isPaused)
                {
                    Pause();
                }
                return;
            }

            EventBus.Publish(new ApplicationFocusGainedEvent());
        }
    }
}
