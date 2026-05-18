using System;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    /// <summary>
    /// Attached to pooled spawn instances so spawners can track lifecycle without polling for null.
    /// Call <see cref="NotifyLifecycleEnded"/> when the entity is defeated or despawned.
    /// </summary>
    public sealed class SpawnerTrackedEntity_UMFOSS : MonoBehaviour
    {
        Action<GameObject> _handler;
        public int WaveNumber { get; private set; }

        public void Configure(Action<GameObject> onEnded, int waveNumber)
        {
            _handler = onEnded;
            WaveNumber = waveNumber;
        }

        public void NotifyLifecycleEnded()
        {
            _handler?.Invoke(gameObject);
        }
    }
}
