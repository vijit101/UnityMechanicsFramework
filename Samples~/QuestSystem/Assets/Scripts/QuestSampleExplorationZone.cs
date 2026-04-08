using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Trigger volume for exploration objectives (<see cref="QuestSampleRuntimeSetup.EventZoneEntered"/>).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public sealed class QuestSampleExplorationZone : MonoBehaviour
    {
        [SerializeField]
        private string zoneId;

        private bool _entered;

        private void Awake()
        {
            var c = GetComponent<Collider>();
            c.isTrigger = true;
        }

        private void Reset()
        {
            var c = GetComponent<Collider>();
            c.isTrigger = true;
        }

        public void Configure(string id)
        {
            zoneId = id;
        }

        private void Start()
        {
            TryFireIfPlayerAlreadyInside();
        }

        /// <summary>
        /// After load, the player may be restored inside the volume; <see cref="OnTriggerEnter"/> does not repeat.
        /// </summary>
        private void TryFireIfPlayerAlreadyInside()
        {
            if (!QuestSamplePlayerLifecycle.IsActionAllowed)
            {
                return;
            }

            if (_entered || string.IsNullOrEmpty(zoneId))
            {
                return;
            }

            var col = GetComponent<Collider>();
            if (col == null)
            {
                return;
            }

            var player = FindFirstObjectByType<CharacterController>();
            if (player == null)
            {
                return;
            }

            var p = player.transform.position;
            if (!col.bounds.Contains(p))
            {
                return;
            }

            _entered = true;
            QuestSampleGameEventHelper.Publish(QuestSampleRuntimeSetup.EventZoneEntered,
                new Dictionary<string, string> { { "zoneID", zoneId } });
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!QuestSamplePlayerLifecycle.IsActionAllowed)
            {
                return;
            }

            if (_entered || other == null || other.GetComponentInParent<CharacterController>() == null)
            {
                return;
            }

            _entered = true;
            QuestSampleGameEventHelper.Publish(QuestSampleRuntimeSetup.EventZoneEntered,
                new Dictionary<string, string> { { "zoneID", zoneId } });
        }
    }
}
