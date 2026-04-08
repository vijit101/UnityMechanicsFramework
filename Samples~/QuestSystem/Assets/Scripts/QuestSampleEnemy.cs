using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Goblin stand-in: publishes <see cref="QuestSampleRuntimeSetup.EventEnemyDied"/> payload and removes self from the world.
    /// </summary>
    public sealed class QuestSampleEnemy : MonoBehaviour
    {
        [SerializeField]
        private string enemyType = "Goblin";

        [SerializeField]
        private string saveId;

        public bool IsAlive { get; private set; } = true;

        private QuestSampleWorldRegistry _registry;

        private void Start()
        {
            _registry = QuestSampleWorldRegistry.Instance;
            if (_registry != null && !string.IsNullOrEmpty(saveId))
            {
                _registry.Register(saveId, gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_registry != null && !string.IsNullOrEmpty(saveId))
            {
                _registry.Unregister(saveId);
            }
        }

        public void Configure(string id, string typeLabel)
        {
            saveId = id;
            enemyType = typeLabel;
        }

        public void ApplyHitFromPlayer(string typeFromPlayer)
        {
            if (!IsAlive)
            {
                return;
            }

            IsAlive = false;
            QuestSampleGameEventHelper.Publish(QuestSampleRuntimeSetup.EventEnemyDied,
                new Dictionary<string, string> { { "enemyType", typeFromPlayer } });
            if (_registry != null && !string.IsNullOrEmpty(saveId))
            {
                _registry.Consume(saveId);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        /// <summary>Sample encounter retry: revive this enemy and restore registry consumption state.</summary>
        public void ResetForEncounter()
        {
            IsAlive = true;
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
