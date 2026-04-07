using System.Collections;
using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Utils;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public class WaveSpawner_UMFOSS : MonoBehaviour
    {
        [Header("Wave Configuration")]
        [SerializeField] SpawnProfile_UMFOSS[] waveProfiles;
        [SerializeField] List<SpawnPoint_UMFOSS> spawnPoints;
        [SerializeField] float timeBetweenWaves = 5f;
        [SerializeField] bool loopWaves;

        [Header("Clear Condition")]
        [SerializeField] WaveClearCondition clearCondition;
        [SerializeField] float timedWaveDuration = 30f;

        [Header("Player")]
        [SerializeField] string playerTag = "Player";

        int _currentWave;
        int _activeCount;
        bool _isWaveActive;
        bool _paused;
        bool _manualClearRequested;
        Coroutine _waveRoutine;
        float _waveStartedAt;
        int _sequentialSpawnIndex;

        void OnEnable()
        {
            EventBus.Subscribe<GamePausedEvent>(OnPause);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<GamePausedEvent>(OnPause);
        }

        void OnPause(GamePausedEvent e)
        {
            _paused = e.IsPaused;
        }

        /// <summary>Begins from wave 1 (internal index 0).</summary>
        public void StartWaves()
        {
            if (_waveRoutine != null) StopCoroutine(_waveRoutine);
            _currentWave = 0;
            _sequentialSpawnIndex = 0;
            _waveRoutine = StartCoroutine(WaveLoop());
        }

        public void PauseWaves()
        {
            _paused = true;
        }

        public void ResumeWaves()
        {
            _paused = false;
        }

        /// <summary>Used with <see cref="WaveClearCondition.Manual"/> to advance.</summary>
        public void ClearCurrentWave()
        {
            _manualClearRequested = true;
        }

        /// <param name="oneBasedWaveIndex">Wave number starting at 1 (first wave = 1).</param>
        public void SkipToWave(int oneBasedWaveIndex)
        {
            if (waveProfiles == null || waveProfiles.Length == 0) return;
            if (_waveRoutine != null) StopCoroutine(_waveRoutine);
            _currentWave = Mathf.Clamp(oneBasedWaveIndex - 1, 0, waveProfiles.Length - 1);
            _sequentialSpawnIndex = 0;
            _waveRoutine = StartCoroutine(WaveLoop());
        }

        public int GetCurrentWave() => _currentWave + 1;

        public int GetActiveCount() => _activeCount;

        public bool IsWaveActive() => _isWaveActive;

        /// <summary>Code-driven setup (e.g. demo/bootstrap) without Inspector references.</summary>
        public void ApplyRuntimeConfiguration(
            SpawnProfile_UMFOSS[] waves,
            List<SpawnPoint_UMFOSS> points,
            float betweenWaves,
            bool loop,
            WaveClearCondition clear,
            float timedDur,
            string playerTagValue)
        {
            waveProfiles = waves;
            spawnPoints = points;
            timeBetweenWaves = betweenWaves;
            loopWaves = loop;
            clearCondition = clear;
            timedWaveDuration = timedDur;
            playerTag = playerTagValue;
        }

        IEnumerator WaveLoop()
        {
            EventBus.Publish(new OnSpawnerStartedEvent { Spawner = gameObject });
            if (waveProfiles == null || waveProfiles.Length == 0)
            {
                EventBus.Publish(new OnSpawnerStoppedEvent { Spawner = gameObject });
                yield break;
            }

            while (true)
            {
                while (_paused) yield return null;

                if (_currentWave >= waveProfiles.Length)
                {
                    if (!loopWaves) break;
                    _currentWave = 0;
                }

                var profile = waveProfiles[_currentWave];
                if (profile == null)
                {
                    _currentWave++;
                    continue;
                }

                _isWaveActive = true;
                _manualClearRequested = false;
                _sequentialSpawnIndex = 0;
                _waveStartedAt = Time.time;
                var totalWaves = waveProfiles.Length;
                EventBus.Publish(new OnWaveStartedEvent
                {
                    WaveNumber = _currentWave + 1,
                    TotalWaves = totalWaves
                });

                yield return SpawnWaveProfile(profile);

                yield return WaitForClear(profile);

                var taken = Time.time - _waveStartedAt;
                _isWaveActive = false;
                EventBus.Publish(new OnWaveClearedEvent
                {
                    WaveNumber = _currentWave + 1,
                    TimeTaken = taken
                });

                var t = 0f;
                while (t < timeBetweenWaves)
                {
                    while (_paused) yield return null;
                    t += Time.deltaTime;
                    yield return null;
                }

                _currentWave++;
            }

            EventBus.Publish(new OnAllWavesCompleteEvent { TotalWaves = waveProfiles.Length });
            EventBus.Publish(new OnSpawnerStoppedEvent { Spawner = gameObject });
        }

        IEnumerator WaitForClear(SpawnProfile_UMFOSS profile)
        {
            switch (clearCondition)
            {
                case WaveClearCondition.AllDead:
                    while (_activeCount > 0)
                    {
                        while (_paused) yield return null;
                        yield return null;
                    }

                    break;
                case WaveClearCondition.TimedEnd:
                {
                    var t = 0f;
                    while (t < timedWaveDuration)
                    {
                        while (_paused) yield return null;
                        t += Time.deltaTime;
                        yield return null;
                    }

                    break;
                }
                case WaveClearCondition.Manual:
                    while (!_manualClearRequested)
                    {
                        while (_paused) yield return null;
                        yield return null;
                    }

                    _manualClearRequested = false;
                    break;
            }
        }

        IEnumerator SpawnWaveProfile(SpawnProfile_UMFOSS profile)
        {
            var scaleKey = _currentWave;
            var countScale = profile.EvaluateCountScale(scaleKey);
            var delayScale = profile.EvaluateDelayScale(scaleKey);
            var playerTf = SpawnerSpawnExecution.TryFindPlayerTransform(playerTag);
            if (profile.entries == null) yield break;

            foreach (var entry in profile.entries)
            {
                if (entry == null || entry.prefab == null) continue;
                var raw = Random.Range(entry.minCount, entry.maxCount + 1) * countScale;
                var count = Mathf.Max(0, Mathf.RoundToInt(raw));
                for (var i = 0; i < count; i++)
                {
                    while (_activeCount >= profile.maxSimultaneous)
                    {
                        while (_paused) yield return null;
                        yield return null;
                    }

                    if (profile.spawnPointMode == SpawnPointMode.All)
                    {
                        if (spawnPoints == null) yield break;
                        foreach (var pt in spawnPoints)
                        {
                            if (pt == null) continue;
                            while (_activeCount >= profile.maxSimultaneous)
                            {
                                while (_paused) yield return null;
                                yield return null;
                            }

                            while (!SpawnOne(profile, entry.prefab, pt.GetSpawnPosition()))
                            {
                                while (_paused) yield return null;
                                yield return null;
                            }
                            var dAll = entry.spawnDelay * (1f / delayScale);
                            if (dAll > 0f)
                            {
                                var w = 0f;
                                while (w < dAll)
                                {
                                    while (_paused) yield return null;
                                    w += Time.deltaTime;
                                    yield return null;
                                }
                            }
                        }
                    }
                    else
                    {
                        var pos = SpawnerSpawnExecution.ResolveSpawnPosition(profile, spawnPoints,
                            ref _sequentialSpawnIndex, playerTf);
                        while (!SpawnOne(profile, entry.prefab, pos))
                        {
                            while (_paused) yield return null;
                            yield return null;
                        }
                        var d = entry.spawnDelay * (1f / delayScale);
                        if (d > 0f)
                        {
                            var w = 0f;
                            while (w < d)
                            {
                                while (_paused) yield return null;
                                w += Time.deltaTime;
                                yield return null;
                            }
                        }
                    }
                }
            }
        }

        bool SpawnOne(SpawnProfile_UMFOSS profile, GameObject prefab, Vector3 pos)
        {
            var waveNum = _currentWave + 1;
            var spawned = SpawnerSpawnExecution.SpawnFromPool(prefab, pos, track =>
            {
                track.Configure(go => OnTrackedDeath(profile, go, waveNum), waveNum);
            });
            if (spawned == null) return false;
            _activeCount++;
            EventBus.Publish(new OnSpawnCountChangedEvent
            {
                ActiveCount = _activeCount,
                MaxCount = profile.maxSimultaneous
            });
            return true;
        }

        void OnTrackedDeath(SpawnProfile_UMFOSS profile, GameObject go, int waveNum)
        {
            _activeCount = Mathf.Max(0, _activeCount - 1);
            EventBus.Publish(new OnSpawnedObjectDiedEvent
            {
                Obj = go,
                WaveNumber = waveNum,
                RemainingCount = _activeCount
            });
            EventBus.Publish(new OnSpawnCountChangedEvent
            {
                ActiveCount = _activeCount,
                MaxCount = profile.maxSimultaneous
            });
            if (ObjectPoolManager_UMFOSS.Instance != null)
                ObjectPoolManager_UMFOSS.Instance.Release(go);
        }
    }
}
