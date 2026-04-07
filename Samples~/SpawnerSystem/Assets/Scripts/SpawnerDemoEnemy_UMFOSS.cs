using GameplayMechanicsUMFOSS.World;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.SpawnerSystem
{
    /// <summary>Minimal pooled enemy for the spawner demo — press Kill or collide to despawn.</summary>
    public class SpawnerDemoEnemy_UMFOSS : MonoBehaviour
    {
        public void Kill()
        {
            var t = GetComponent<SpawnerTrackedEntity_UMFOSS>();
            t?.NotifyLifecycleEnded();
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.collider.name == "Player")
                Kill();
        }
    }
}
