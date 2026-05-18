using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>Circular buffer that records the last N state transitions.</summary>
    public class StateHistory_UMFOSS
    {
        public struct StateHistoryEntry
        {
            public string stateName;
            public float  timeEntered;
            public float  duration;
        }

        private readonly int capacity;
        private readonly Queue<StateHistoryEntry> history;

        public StateHistory_UMFOSS(int capacity = 10)
        {
            this.capacity = capacity;
            history       = new Queue<StateHistoryEntry>(capacity);
        }

        /// <summary>Records a new state entry. Drops the oldest if at capacity.</summary>
        public void Record(IState_UMFOSS state)
        {
            if (history.Count >= capacity)
                history.Dequeue();

            history.Enqueue(new StateHistoryEntry
            {
                stateName   = state.GetType().Name,
                timeEntered = Time.time,
                duration    = 0f
            });
        }

        /// <summary>Sets duration on the most recently recorded entry when that state exits.</summary>
        public void MarkCurrentExited()
        {
            if (history.Count == 0) return;

            var entries = history.ToArray();
            entries[entries.Length - 1].duration = Time.time - entries[entries.Length - 1].timeEntered;

            history.Clear();
            foreach (var e in entries)
                history.Enqueue(e);
        }

        /// <summary>Returns all recorded entries oldest-first.</summary>
        public IEnumerable<StateHistoryEntry> GetHistory() => history;

        /// <summary>Returns the most recently recorded entry.</summary>
        public StateHistoryEntry GetLatest() => history.Last();
    }
}
