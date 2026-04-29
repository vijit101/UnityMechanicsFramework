using System;
using System.Collections;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    // ─────────────────────────────────────────────────────────────────────────
    //  TimerUtility_UMFOSS
    //  A lightweight, coroutine-backed timer that fires a callback after a
    //  specified duration.  Plain C# classes (like Stat_UMFOSS) can use it
    //  because the hidden TimerRunner MonoBehaviour owns the coroutine.
    //
    //  Usage:
    //      var timer = TimerUtility_UMFOSS.Create(5f, () => Debug.Log("done"));
    //      timer.Start();
    //      timer.Cancel(); // optional early cancel
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A fire-and-forget timer.  Create with <see cref="Create"/>, then call
    /// <see cref="Start"/> to begin the countdown.
    /// </summary>
    public class TimerUtility_UMFOSS
    {
        // ─── Fields ───────────────────────────────────────────────────────────

        private readonly float    _duration;
        private readonly Action   _onComplete;
        private Coroutine         _coroutine;
        private bool              _isCancelled;

        // ─── Constructor (private – use Create) ───────────────────────────────

        private TimerUtility_UMFOSS(float duration, Action onComplete)
        {
            _duration   = duration;
            _onComplete = onComplete;
        }

        // ─── Factory ──────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a new timer.  Call <see cref="Start"/> to begin.
        /// </summary>
        /// <param name="duration">Seconds to wait before firing <paramref name="onComplete"/>.</param>
        /// <param name="onComplete">Callback invoked when the timer expires.</param>
        public static TimerUtility_UMFOSS Create(float duration, Action onComplete)
        {
            return new TimerUtility_UMFOSS(duration, onComplete);
        }

        // ─── Public Methods ───────────────────────────────────────────────────

        /// <summary>
        /// Starts the countdown.  Safe to call only once per timer instance.
        /// </summary>
        public void Start()
        {
            _isCancelled = false;
            _coroutine   = TimerRunner.Instance.Run(CountdownRoutine());
        }

        /// <summary>
        /// Cancels the timer before it fires.  The <c>onComplete</c> callback
        /// will <b>not</b> be invoked.
        /// </summary>
        public void Cancel()
        {
            _isCancelled = true;
            if (_coroutine != null)
            {
                TimerRunner.Instance.StopCoroutine(_coroutine);
                _coroutine = null;
            }
        }

        // ─── Private Methods ──────────────────────────────────────────────────

        private IEnumerator CountdownRoutine()
        {
            yield return new WaitForSeconds(_duration);
            if (!_isCancelled)
                _onComplete?.Invoke();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TimerRunner — hidden MonoBehaviour that owns coroutines
        //  Automatically creates itself in the scene on first use.
        // ─────────────────────────────────────────────────────────────────────

        private class TimerRunner : MonoBehaviour
        {
            private static TimerRunner _instance;

            internal static TimerRunner Instance
            {
                get
                {
                    if (_instance != null) return _instance;

                    var go = new GameObject("[TimerRunner_UMFOSS]")
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<TimerRunner>();
                    return _instance;
                }
            }

            internal Coroutine Run(IEnumerator routine)
            {
                return StartCoroutine(routine);
            }
        }
    }
}
