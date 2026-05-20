using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public class WaveSpawner_UMFOSS : MonoBehaviour
    {
        [Header("Wave Configuration")]
        [SerializeField] private SpawnProfile_UMFOSS[] waveProfiles;
        [SerializeField] private List<SpawnPoint_UMFOSS> spawnPoints;
        [SerializeField] private float timeBetweenWaves = 5f;
        [SerializeField] private bool loopWaves = false;

        [Header("Clear Condition")]
        [SerializeField] private WaveClearCondition clearCondition = WaveClearCondition.AllDead;
        [SerializeField] private float waveDuration = 30f;

        private int currentWave = 0;
        private int activeCount = 0;
        private bool isWaveActive = false;
        private bool isPaused = false;
        private int sequentialIndex = 0;
        private float waveStartTime;
        private List<GameObject> spawnedObjects = new List<GameObject>();
        private Coroutine waveCoroutine;

        // --- Public Properties ---

        public int CurrentWave => currentWave;
        public int ActiveCount => activeCount;
        public bool IsWaveActive => isWaveActive;
        public int TotalWaves => waveProfiles != null ? waveProfiles.Length : 0;

        // --- Unity Lifecycle ---

        private void OnEnable()
        {
            EventBus.Subscribe<OnSpawnedObjectDied>(OnObjectDied);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnSpawnedObjectDied>(OnObjectDied);
        }

        // --- Public API ---

        public void StartWaves()
        {
            currentWave = 0;
            if (waveCoroutine != null)
                StopCoroutine(waveCoroutine);
            waveCoroutine = StartCoroutine(RunWaves());
            EventBus.Publish(new OnSpawnerStarted { spawner = gameObject });
        }

        public void PauseWaves()
        {
            isPaused = true;
        }

        public void ResumeWaves()
        {
            isPaused = false;
        }

        public void ClearCurrentWave()
        {
            if (clearCondition == WaveClearCondition.Manual)
                isWaveActive = false;
        }

        public void SkipToWave(int waveIndex)
        {
            if (waveIndex >= 0 && waveIndex < waveProfiles.Length)
                currentWave = waveIndex;
        }

        public int GetActiveCount() => activeCount;

        // --- Private Methods ---

        private IEnumerator RunWaves()
        {
            do
            {
                for (; currentWave < waveProfiles.Length; currentWave++)
                {
                    yield return StartCoroutine(RunSingleWave(currentWave));
                    if (currentWave < waveProfiles.Length - 1 || loopWaves)
                        yield return WaitUnpaused(timeBetweenWaves);
                }

                if (loopWaves)
                    currentWave = 0;

            } while (loopWaves);

            EventBus.Publish(new OnAllWavesComplete { totalWaves = waveProfiles.Length });
            EventBus.Publish(new OnSpawnerStopped { spawner = gameObject });
        }

        private IEnumerator RunSingleWave(int waveIndex)
        {
            isWaveActive = true;
            waveStartTime = Time.time;
            spawnedObjects.Clear();
            activeCount = 0;

            SpawnProfile_UMFOSS profile = waveProfiles[waveIndex];
            float progress = (float)waveIndex / Mathf.Max(1, waveProfiles.Length - 1);

            EventBus.Publish(new OnWaveStarted
            {
                waveNumber = waveIndex + 1,
                totalWaves = waveProfiles.Length
            });

            foreach (var entry in profile.entries)
            {
                int count = SpawnHelper_UMFOSS.GetScaledCount(
                    entry, profile.countScaleCurve, progress);

                for (int i = 0; i < count; i++)
                {
                    if (activeCount >= profile.maxSimultaneous)
                        break;

                    SpawnOneObject(entry, profile);

                    if (entry.spawnDelay > 0f)
                        yield return WaitUnpaused(entry.spawnDelay);
                }
            }

            switch (clearCondition)
            {
                case WaveClearCondition.AllDead:
                    while (activeCount > 0)
                        yield return null;
                    break;
                case WaveClearCondition.TimedEnd:
                    yield return WaitUnpaused(waveDuration);
                    break;
                case WaveClearCondition.Manual:
                    while (isWaveActive)
                        yield return null;
                    break;
            }

            isWaveActive = false;
            EventBus.Publish(new OnWaveCleared
            {
                waveNumber = waveIndex + 1,
                timeTaken = Time.time - waveStartTime
            });
        }

        private void SpawnOneObject(
            SpawnProfile_UMFOSS.SpawnEntry entry,
            SpawnProfile_UMFOSS profile)
        {
            Vector3 pos = SpawnHelper_UMFOSS.GetSpawnPosition(
                profile.spawnPointMode, spawnPoints, ref sequentialIndex, Vector3.zero);

            GameObject obj = Instantiate(entry.prefab);
            obj.transform.position = pos;
            obj.SetActive(true);

            spawnedObjects.Add(obj);
            activeCount++;

            EventBus.Publish(new OnSpawnCountChanged
            {
                activeCount = activeCount,
                maxCount = profile.maxSimultaneous
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
                maxCount = waveProfiles != null && currentWave < waveProfiles.Length
                    ? waveProfiles[currentWave].maxSimultaneous : 0
            });
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
