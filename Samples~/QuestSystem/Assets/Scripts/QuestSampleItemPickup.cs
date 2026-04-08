using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Walk into pickup to collect; publishes <see cref="QuestSampleRuntimeSetup.EventItemAdded"/>.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class QuestSampleItemPickup : MonoBehaviour
    {
        [SerializeField]
        private string itemName;

        [SerializeField]
        private string saveId;

        private QuestSampleWorldRegistry _registry;

        private bool _collected;

        private void Start()
        {
            _registry = QuestSampleWorldRegistry.Instance;
            if (_registry != null && !string.IsNullOrEmpty(saveId))
            {
                _registry.Register(saveId, gameObject);
            }

            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void OnDestroy()
        {
            if (_registry != null && !string.IsNullOrEmpty(saveId))
            {
                _registry.Unregister(saveId);
            }
        }

        public void Configure(string id, string item)
        {
            saveId = id;
            itemName = item;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!QuestSamplePlayerLifecycle.IsActionAllowed)
            {
                return;
            }

            if (_collected || other == null || other.GetComponentInParent<CharacterController>() == null)
            {
                return;
            }

            Collect();
        }

        private void Collect()
        {
            _collected = true;
            QuestSampleGameEventHelper.Publish(QuestSampleRuntimeSetup.EventItemAdded,
                new Dictionary<string, string> { { "itemName", itemName } });
            if (_registry != null && !string.IsNullOrEmpty(saveId))
            {
                _registry.Consume(saveId);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>Sample retry: allow collecting again and restore registry state.</summary>
        public void ResetPickup()
        {
            _collected = false;
            if (_registry != null && !string.IsNullOrEmpty(saveId))
            {
                _registry.Unconsume(saveId);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }
}
