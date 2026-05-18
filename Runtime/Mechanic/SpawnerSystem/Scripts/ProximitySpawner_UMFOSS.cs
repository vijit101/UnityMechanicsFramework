using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public class ProximitySpawner_UMFOSS : MonoBehaviour
    {
        [Header("Proximity Configuration")]
        [SerializeField] private SpawnProfile_UMFOSS profile;
        [SerializeField] private List<SpawnPoint_UMFOSS> spawnPoints;
        [SerializeField] private float triggerRadius = 5f;
        [SerializeField] private bool isOneShot = true;
        [SerializeField] private bool requireLineOfSight = false;
        [SerializeField] private float cooldown = 0f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Detection")]
        [SerializeField] private float checkInterval = 0.2f;

        private bool hasFired = false;
        private bool isEnabled = true;
        private bool isPaused = false;
        private float lastTriggerTime = -999f;
        private int sequentialIndex = 0;
        private List<GameObject> spawnedObjects = new List<GameObject>();

        // --- Public Properties ---

        public bool HasFiredValue => hasFired;
        public float TriggerRadius => triggerRadius;

        // --- Unity Lifecycle ---

        private void OnEnable()
        {
            EventBus.Subscribe<OnSpawnedObjectDied>(OnObjectDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnSpawnedObjectDied>(OnObjectDied);
        }

        private void Start()
        {
            StartCoroutine(ProximityCheckLoop());
        }

        // --- Public API ---

        public void Enable()
        {
            isEnabled = true;
            hasFired = false;
        }

        public void Disable()
        {
            isEnabled = false;
        }

        public void ForceSpawn()
        {
            DoSpawn(transform.position);
        }

        public bool HasFired() => hasFired;

        // --- Private Methods ---

        private IEnumerator ProximityCheckLoop()
        {
            while (true)
            {
                if (isEnabled && !isPaused)
                {
                    if (!(isOneShot && hasFired)
                        && Time.time - lastTriggerTime >= cooldown)
                    {
                        Collider2D hit = Physics2D.OverlapCircle(
                            transform.position, triggerRadius, playerLayer);

                        if (hit != null)
                        {
                            bool canSpawn = true;

                            if (requireLineOfSight)
                            {
                                Vector2 direction = (Vector2)(hit.transform.position
                                    - transform.position);
                                RaycastHit2D ray = Physics2D.Raycast(
                                    transform.position, direction, triggerRadius);
                                if (ray.collider != hit)
                                    canSpawn = false;
                            }

                            if (canSpawn)
                            {
                                DoSpawn(hit.transform.position);
                                lastTriggerTime = Time.time;

                                if (isOneShot)
                                    hasFired = true;
                            }
                        }
                    }
                }

                yield return new WaitForSeconds(checkInterval);
            }
        }

        private void DoSpawn(Vector3 playerPosition)
        {
            if (profile == null || profile.entries == null) return;

            int totalSpawned = 0;

            foreach (var entry in profile.entries)
            {
                int count = Random.Range(entry.minCount, entry.maxCount + 1);

                for (int i = 0; i < count; i++)
                {
                    if (spawnedObjects.Count >= profile.maxSimultaneous)
                        break;

                    Vector3 pos = SpawnHelper_UMFOSS.GetSpawnPosition(
                        profile.spawnPointMode, spawnPoints,
                        ref sequentialIndex, playerPosition);

                    GameObject obj = Instantiate(entry.prefab);
                    obj.transform.position = pos;
                    obj.SetActive(true);

                    spawnedObjects.Add(obj);
                    totalSpawned++;
                }
            }

            EventBus.Publish(new OnProximitySpawnTriggered
            {
                triggerPosition = transform.position,
                spawnCount = totalSpawned
            });
            EventBus.Publish(new OnSpawnerStarted { spawner = gameObject });
        }

        private void OnObjectDied(OnSpawnedObjectDied e)
        {
            if (spawnedObjects.Contains(e.obj))
                spawnedObjects.Remove(e.obj);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = hasFired
                ? new Color(0.5f, 0.5f, 0.5f, 0.2f)
                : new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}
