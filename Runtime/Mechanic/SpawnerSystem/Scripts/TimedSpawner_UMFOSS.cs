using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public class TimedSpawner_UMFOSS : MonoBehaviour
    {
        [Header("Timed Configuration")]
        [SerializeField] private SpawnProfile_UMFOSS profile;
        [SerializeField] private List<SpawnPoint_UMFOSS> spawnPoints;
        [SerializeField] private float spawnInterval = 3f;
        [SerializeField] private int maxActive = 5;
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool respawnOnDeath = true;

        private int activeCount = 0;
        private bool isSpawning = false;
        private bool isPaused = false;
        private int sequentialIndex = 0;
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private Coroutine spawnCoroutine;

        // --- Public Properties ---

        public int ActiveCount => activeCount;
        public bool IsSpawning => isSpawning;

        // --- Unity Lifecycle ---

        private void Start()
        {
            if (spawnOnStart)
                StartSpawning();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<OnSpawnedObjectDied>(OnObjectDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnSpawnedObjectDied>(OnObjectDied);
        }

        // --- Public API ---

        public void Configure(SpawnProfile_UMFOSS profile, List<SpawnPoint_UMFOSS> spawnPoints,
            float interval = 3f, int maxActive = 5, bool spawnOnStart = true, bool respawnOnDeath = true)
        {
            this.profile = profile;
            this.spawnPoints = spawnPoints;
            this.spawnInterval = Mathf.Max(0.1f, interval);
            this.maxActive = Mathf.Max(1, maxActive);
            this.spawnOnStart = spawnOnStart;
            this.respawnOnDeath = respawnOnDeath;
        }

        public void StartSpawning()
        {
            if (isSpawning) return;
            isSpawning = true;

            if (spawnCoroutine != null)
                StopCoroutine(spawnCoroutine);
            spawnCoroutine = StartCoroutine(SpawnLoop());
            EventBus.Publish(new OnSpawnerStarted { spawner = gameObject });
        }

        public void StopSpawning()
        {
            isSpawning = false;
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
            EventBus.Publish(new OnSpawnerStopped { spawner = gameObject });
        }

        public void SetInterval(float seconds)
        {
            spawnInterval = Mathf.Max(0.1f, seconds);
        }

        public void SetMaxActive(int count)
        {
            maxActive = Mathf.Max(1, count);
        }

        public int GetActiveCount() => activeCount;

        // --- Private Methods ---

        private IEnumerator SpawnLoop()
        {
            while (isSpawning)
            {
                if (!isPaused && activeCount < maxActive)
                {
                    SpawnOne();

                    if (activeCount >= maxActive)
                    {
                        EventBus.Publish(new OnTimedSpawnCapReached
                        {
                            maxActive = maxActive
                        });
                    }
                }

                yield return WaitUnpaused(spawnInterval);
            }
        }

        private void SpawnOne()
        {
            if (profile == null || profile.entries == null || profile.entries.Length == 0)
                return;

            var entry = SpawnHelper_UMFOSS.SelectWeightedRandom(profile.entries);
            Vector3 pos = SpawnHelper_UMFOSS.GetSpawnPosition(
                profile.spawnPointMode, spawnPoints, ref sequentialIndex, Vector3.zero);

            GameObject obj = Instantiate(entry.prefab);
            obj.transform.position = pos;
            obj.SetActive(true);

            spawnedObjects.Add(obj);
            activeCount++;

            EventBus.Publish(new OnTimedSpawnTriggered
            {
                spawnedObj = obj,
                position = pos
            });
            EventBus.Publish(new OnSpawnCountChanged
            {
                activeCount = activeCount,
                maxCount = maxActive
            });
        }

        private void OnObjectDied(OnSpawnedObjectDied e)
        {
            if (!spawnedObjects.Contains(e.obj)) return;

            spawnedObjects.Remove(e.obj);
            activeCount = Mathf.Max(0, activeCount - 1);

            EventBus.Publish(new OnSpawnCountChanged
            {
                activeCount = activeCount,
                maxCount = maxActive
            });

            if (respawnOnDeath && isSpawning)
                StartCoroutine(RespawnAfterDelay());
        }

        private IEnumerator RespawnAfterDelay()
        {
            yield return WaitUnpaused(spawnInterval);
            if (isSpawning && activeCount < maxActive)
                SpawnOne();
        }

        private IEnumerator WaitUnpaused(float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                if (!isPaused)
                    elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
