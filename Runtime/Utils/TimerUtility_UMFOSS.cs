using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// A fully modular, event-driven timer utility.
    /// Attach to any GameObject, configure in the Inspector, subscribe to events — done.
    /// Eliminates scattered float timer -= Time.deltaTime patterns across your project.
    /// </summary>
    public class TimerUtility_UMFOSS : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        // CONSTANTS
        // ─────────────────────────────────────────────
        private const float MIN_DURATION    = 0.001f;
        private const float MIN_TICK        = 0.001f;
        private const int   INFINITE_LOOPS  = -1;
        private const float PROGRESS_MIN    = 0f;
        private const float PROGRESS_MAX    = 1f;

        // ─────────────────────────────────────────────
        // SERIALIZED INSPECTOR FIELDS
        // ─────────────────────────────────────────────

        [Header("Timer Settings")]
        [Tooltip("Total countdown duration in seconds.")]
        [SerializeField] private float duration = 5f;

        [Tooltip("If true, timer starts automatically when Awake() is called.")]
        [SerializeField] private bool autoStart = false;

        [Tooltip("If true, timer restarts automatically each time it completes.")]
        [SerializeField] private bool loop = false;

        [Tooltip("Number of times to loop before stopping. -1 = infinite.")]
        [SerializeField] private int loopCount = INFINITE_LOOPS;

        [Header("Tick Settings")]
        [Tooltip("If true, OnTimerTick fires regularly while the timer runs.")]
        [SerializeField] private bool enableTicks = false;

        [Tooltip("Interval in seconds between each OnTimerTick event.")]
        [SerializeField] private float tickInterval = 1f;

        [Header("Scale Settings")]
        [Tooltip("If true, the timer uses Time.unscaledDeltaTime and ignores Time.timeScale. Useful for pause menus.")]
        [SerializeField] private bool useUnscaledTime = false;

        /// <summary>
        /// Get or set whether this timer uses unscaled time.
        /// When true, the timer ignores Time.timeScale — it keeps running even when the game is paused.
        /// Can be set from code after AddComponent, not just from the Inspector.
        /// </summary>
        public bool UseUnscaledTime
        {
            get => useUnscaledTime;
            set => useUnscaledTime = value;
        }


        // ─────────────────────────────────────────────
        // PRIVATE STATE FIELDS
        // ─────────────────────────────────────────────

        private float elapsed        = 0f;   // seconds that have passed since Start()
        private float tickAccumulator = 0f;  // seconds accumulated toward next tick
        private int   completedLoops  = 0;   // how many loops have finished so far

        private bool running   = false;
        private bool paused    = false;
        private bool completed = false;

        // ─────────────────────────────────────────────
        // EVENTS
        // ─────────────────────────────────────────────

        /// <summary>Fires when Start() is called.</summary>
        public UnityAction OnTimerStart;

        /// <summary>Fires the moment the timer reaches zero.</summary>
        public UnityAction OnTimerComplete;

        /// <summary>Fires every TickInterval seconds while running. Carries seconds remaining.</summary>
        public UnityAction<float> OnTimerTick;

        /// <summary>Fires when Pause() is called.</summary>
        public UnityAction OnTimerPaused;

        /// <summary>Fires when Resume() is called.</summary>
        public UnityAction OnTimerResumed;

        /// <summary>Fires when Stop() is called.</summary>
        public UnityAction OnTimerStopped;

        /// <summary>Fires at the end of each loop cycle. loopIndex is zero-based.</summary>
        public UnityAction<int> OnLoopComplete;

        /// <summary>Fires after the final loop when LoopCount is reached.</summary>
        public UnityAction OnAllLoopsComplete;

        // ─────────────────────────────────────────────
        // UNITY LIFECYCLE
        // ─────────────────────────────────────────────

        private void Awake()
        {
            if (autoStart)
                Start();
        }

        private void Update()
        {
            if (!running || paused) return;

            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += delta;

            // ── Tick system ──────────────────────────
            if (enableTicks)
            {
                tickAccumulator += delta;
                while (tickAccumulator >= tickInterval)
                {
                    tickAccumulator -= tickInterval;
                    OnTimerTick?.Invoke(GetTimeRemaining());
                }
            }

            // ── Completion check ─────────────────────
            if (elapsed >= duration)
            {
                HandleCompletion();
            }
        }

        // ─────────────────────────────────────────────
        // PUBLIC METHODS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Starts or restarts the timer from the full Duration.
        /// Safe to call at any time — resets internal state cleanly.
        /// </summary>
        public new void Start()
        {
            elapsed          = 0f;
            tickAccumulator  = 0f;
            completedLoops   = 0;
            running          = true;
            paused           = false;
            completed        = false;

            OnTimerStart?.Invoke();
        }

        /// <summary>
        /// Freezes the timer at its current elapsed value.
        /// Does nothing if not currently running or already paused.
        /// </summary>
        public void Pause()
        {
            if (!running || paused) return;
            paused = true;
            OnTimerPaused?.Invoke();
        }

        /// <summary>
        /// Continues the timer from where Pause() stopped.
        /// Does nothing if not paused.
        /// </summary>
        public void Resume()
        {
            if (!paused) return;
            paused = false;
            OnTimerResumed?.Invoke();
        }

        /// <summary>
        /// Halts and resets the timer to its full Duration.
        /// Does NOT fire OnTimerComplete — use this for cancellation.
        /// </summary>
        public void Stop()
        {
            running          = false;
            paused           = false;
            completed        = false;
            elapsed          = 0f;
            tickAccumulator  = 0f;
            completedLoops   = 0;

            OnTimerStopped?.Invoke();
        }

        /// <summary>
        /// Resets elapsed time to zero without stopping.
        /// If the timer was running it continues from the beginning.
        /// </summary>
        public void Reset()
        {
            elapsed         = 0f;
            tickAccumulator = 0f;
            completed       = false;
        }

        /// <summary>
        /// Updates the timer duration at runtime.
        /// </summary>
        /// <param name="newDuration">The new duration in seconds.</param>
        /// <param name="resetTimer">If true, elapsed time is reset and timer restarts from the new duration. If false, the timer continues and completes at the new threshold.</param>
        public void SetDuration(float newDuration, bool resetTimer)
        {
            duration = Mathf.Max(newDuration, MIN_DURATION);
            if (resetTimer)
            {
                elapsed         = 0f;
                tickAccumulator = 0f;
                completed       = false;
            }
        }

        // ─────────────────────────────────────────────
        // READ-ONLY ACCESSORS
        // ─────────────────────────────────────────────

        /// <summary>Returns the number of seconds remaining before completion.</summary>
        public float GetTimeRemaining() => Mathf.Max(duration - elapsed, 0f);

        /// <summary>Returns the number of seconds elapsed since the last Start() call.</summary>
        public float GetTimeElapsed() => elapsed;

        /// <summary>
        /// Returns a 0.0–1.0 value representing how far through the duration the timer is.
        /// 0 = just started, 1 = complete. Use this directly with any progress bar or fill image.
        /// </summary>
        public float GetProgress() => Mathf.Clamp(elapsed / Mathf.Max(duration, MIN_DURATION), PROGRESS_MIN, PROGRESS_MAX);

        /// <summary>Returns true if the timer is actively counting down.</summary>
        public bool IsRunning() => running && !paused;

        /// <summary>Returns true if the timer has been paused mid-countdown.</summary>
        public bool IsPaused() => paused;

        /// <summary>Returns true if the timer has reached zero and is not looping.</summary>
        public bool IsComplete() => completed;

        // ─────────────────────────────────────────────
        // STATIC FACTORY METHODS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Creates and starts a one-shot timer entirely from code.
        /// Internally creates a hidden GameObject that destroys itself on completion.
        /// The caller never manages a GameObject.
        /// </summary>
        /// <param name="duration">How long the timer runs in seconds.</param>
        /// <param name="onComplete">Optional callback fired when the timer completes.</param>
        /// <returns>The TimerUtility_UMFOSS instance for further configuration or event subscription.</returns>
        public static TimerUtility_UMFOSS Create(float duration, Action onComplete = null)
        {
            var go    = new GameObject("[Timer] One-Shot");
            var timer = go.AddComponent<TimerUtility_UMFOSS>();

            timer.duration      = Mathf.Max(duration, MIN_DURATION);
            timer.loop          = false;
            timer.autoStart     = false;

            if (onComplete != null)
                timer.OnTimerComplete += () => onComplete();

            // Self-destroy when done
            timer.OnTimerComplete += () => Destroy(go);

            timer.Start();
            return timer;
        }

        /// <summary>
        /// Creates and starts a looping timer entirely from code.
        /// Fires onTick on every loop cycle. Runs infinitely until Stop() is called.
        /// Internally creates a hidden, self-managing GameObject.
        /// </summary>
        /// <param name="interval">Seconds between each loop cycle.</param>
        /// <param name="onTick">Callback fired on every completed loop. Use this like a repeating event.</param>
        /// <returns>The TimerUtility_UMFOSS instance. Call Stop() on it to halt the looping timer.</returns>
        public static TimerUtility_UMFOSS CreateLooping(float interval, Action onTick = null)
        {
            var go    = new GameObject("[Timer] Looping");
            var timer = go.AddComponent<TimerUtility_UMFOSS>();

            timer.duration      = Mathf.Max(interval, MIN_DURATION);
            timer.loop          = true;
            timer.loopCount     = INFINITE_LOOPS;
            timer.autoStart     = false;

            if (onTick != null)
                timer.OnLoopComplete += (_) => onTick();

            timer.Start();
            return timer;
        }

        // ─────────────────────────────────────────────
        // PRIVATE METHODS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Called internally when elapsed time reaches or exceeds Duration.
        /// Handles loop restart logic, loop counting, and final completion.
        /// </summary>
        private void HandleCompletion()
        {
            // Fire the final tick if ticks are enabled and we haven't fired this boundary yet
            if (enableTicks)
                OnTimerTick?.Invoke(0f);

            OnTimerComplete?.Invoke();

            if (loop)
            {
                OnLoopComplete?.Invoke(completedLoops);
                completedLoops++;

                bool infiniteLoop  = loopCount == INFINITE_LOOPS;
                bool loopsLeft     = completedLoops < loopCount;

                if (infiniteLoop || loopsLeft)
                {
                    // Restart — carry over any overshoot so intervals stay accurate
                    elapsed         = elapsed - duration;
                    tickAccumulator = 0f;
                    completed       = false;
                }
                else
                {
                    // All loops finished
                    OnAllLoopsComplete?.Invoke();
                    running   = false;
                    completed = true;
                }
            }
            else
            {
                running   = false;
                completed = true;
            }
        }
    }
}
