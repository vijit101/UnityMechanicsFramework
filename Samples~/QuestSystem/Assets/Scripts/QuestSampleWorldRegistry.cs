using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Tracks consumable world entities (enemies, pickups) for save/load alongside quest progress.
    /// </summary>
    public sealed class QuestSampleWorldRegistry : MonoBehaviour
    {
        private static QuestSampleWorldRegistry _instance;

        private readonly Dictionary<string, GameObject> _idToObject = new Dictionary<string, GameObject>();

        private readonly HashSet<string> _consumed = new HashSet<string>();

        public static QuestSampleWorldRegistry Instance => _instance;

        private void Awake()
        {
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public void Register(string id, GameObject target)
        {
            if (string.IsNullOrEmpty(id) || target == null)
            {
                return;
            }

            _idToObject[id] = target;
            if (_consumed.Contains(id))
            {
                target.SetActive(false);
            }
        }

        public void Unregister(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            _idToObject.Remove(id);
        }

        public void Consume(string id)
        {
            if (string.IsNullOrEmpty(id) || !_idToObject.TryGetValue(id, out var go) || go == null)
            {
                return;
            }

            _consumed.Add(id);
            go.SetActive(false);
        }

        /// <summary>Restores a consumed id for encounter retries (sample-only; save/load still uses <see cref="LoadConsumedSnapshot"/>).</summary>
        public void Unconsume(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            _consumed.Remove(id);
            if (_idToObject.TryGetValue(id, out var go) && go != null)
            {
                go.SetActive(true);
            }
        }

        public string[] GetConsumedSnapshot()
        {
            return _consumed.ToArray();
        }

        public void LoadConsumedSnapshot(string[] ids)
        {
            _consumed.Clear();
            if (ids == null)
            {
                return;
            }

            foreach (var id in ids)
            {
                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                _consumed.Add(id);
                if (_idToObject.TryGetValue(id, out var go) && go != null)
                {
                    go.SetActive(false);
                }
            }
        }
    }
}
