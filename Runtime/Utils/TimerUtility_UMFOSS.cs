using System;
using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Core;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// Pause-aware timers driven from one <see cref="Update"/> loop (no per-spawner countdown fields).
    /// </summary>
    public sealed class TimerUtility_UMFOSS : MonoSingletonGeneric<TimerUtility_UMFOSS>
    {
        readonly List<TimerEntry> _timers = new List<TimerEntry>(16);
        int _nextId = 1;
        bool _paused;

        struct TimerEntry
        {
            public int Id;
            public float Remaining;
            public float RepeatInterval;
            public bool Repeating;
            public Action Callback;
        }

        protected override void Awake()
        {
            base.Awake();
            EventBus.Subscribe<GamePausedEvent>(e => { _paused = e.IsPaused; });
        }

        void Update()
        {
            if (_paused) return;
            var dt = Time.deltaTime;
            for (var i = _timers.Count - 1; i >= 0; i--)
            {
                var t = _timers[i];
                t.Remaining -= dt;
                if (t.Remaining > 0f)
                {
                    _timers[i] = t;
                    continue;
                }

                t.Callback?.Invoke();
                if (t.Repeating)
                {
                    t.Remaining = t.RepeatInterval;
                    _timers[i] = t;
                }
                else
                    _timers.RemoveAt(i);
            }
        }

        /// <summary>Schedules a one-shot callback after <paramref name="delaySeconds"/>.</summary>
        public int ScheduleOnce(float delaySeconds, Action callback)
        {
            if (callback == null) return 0;
            var id = _nextId++;
            _timers.Add(new TimerEntry
            {
                Id = id,
                Remaining = Mathf.Max(0.0001f, delaySeconds),
                RepeatInterval = 0f,
                Repeating = false,
                Callback = callback
            });
            return id;
        }

        /// <summary>Schedules a repeating callback every <paramref name="intervalSeconds"/>.</summary>
        public int ScheduleRepeating(float intervalSeconds, Action callback)
        {
            if (callback == null) return 0;
            var id = _nextId++;
            var iv = Mathf.Max(0.0001f, intervalSeconds);
            _timers.Add(new TimerEntry
            {
                Id = id,
                Remaining = iv,
                RepeatInterval = iv,
                Repeating = true,
                Callback = callback
            });
            return id;
        }

        /// <summary>Cancels a previously returned timer id, if still pending.</summary>
        public void Cancel(int timerId)
        {
            if (timerId <= 0) return;
            for (var i = _timers.Count - 1; i >= 0; i--)
            {
                if (_timers[i].Id == timerId)
                    _timers.RemoveAt(i);
            }
        }
    }
}
