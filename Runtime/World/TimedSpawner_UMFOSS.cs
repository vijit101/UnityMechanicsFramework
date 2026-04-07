using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Utils;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public class TimedSpawner_UMFOSS : MonoBehaviour
    {
        [Header("Timed Configuration")]
        [SerializeField] SpawnProfile_UMFOSS profile;
        [SerializeField] List<SpawnPoint_UMFOSS> spawnPoints;
        [SerializeField] float spawnInterval = 3f;
        [SerializeField] int maxActive = 5;
        [SerializeField] bool spawnOnStart = true;
        [SerializeField] bool respawnOnDeath = true;

        [Header("Player")]
        [SerializeField] string playerTag = "Player";

        int _activeCount;
        int _repeatTimerId;
        int _sequentialIndex;
        bool _paused;
        bool _spawning;
        float _nextSpawnAllowedTime;

        void OnEnable()
        {
            EventBus.Subscribe<GamePausedEvent>(OnPause);
        }

        void OnDisable()
        {
            EventBus.Unsubscribe<GamePausedEvent>(OnPause);
            StopSpawning();
        }

        void Start()
        {
            if (spawnOnStart) StartSpawning();
        }

        void OnPause(GamePausedEvent e)
        {
            _paused = e.IsPaused;
        }

        public void StartSpawning()
        {
            if (_spawning || profile == null) return;
            if (TimerUtility_UMFOSS.Instance == null)
            {
                var go = new GameObject(nameof(TimerUtility_UMFOSS));
                go.AddComponent<TimerUtility_UMFOSS>();
            }

            _spawning = true;
            _nextSpawnAllowedTime = Time.time;
            EventBus.Publish(new OnSpawnerStartedEvent { Spawner = gameObject });
            _repeatTimerId = TimerUtility_UMFOSS.Instance.ScheduleRepeating(spawnInterval, OnTimedTick);
        }

        public void StopSpawning()
        {
            if (!_spawning) return;
            if (TimerUtility_UMFOSS.Instance != null && _repeatTimerId != 0)
                TimerUtility_UMFOSS.Instance.Cancel(_repeatTimerId);
            _repeatTimerId = 0;
            _spawning = false;
            EventBus.Publish(new OnSpawnerStoppedEvent { Spawner = gameObject });
        }

        public void SetInterval(float seconds)
        {
            spawnInterval = Mathf.Max(0.01f, seconds);
            if (_spawning)
            {
                StopSpawning();
                StartSpawning();
            }
        }

        public void SetMaxActive(int count)
        {
            maxActive = Mathf.Max(0, count);
        }

        public int GetActiveCount() => _activeCount;

        /// <summary>Code-driven setup (e.g. demo/bootstrap) without Inspector references.</summary>
        public void ApplyRuntimeConfiguration(
            SpawnProfile_UMFOSS profileValue,
            List<SpawnPoint_UMFOSS> points,
            float interval,
            int max,
            bool onStart,
            bool respawn,
            string playerTagValue)
        {
            profile = profileValue;
            spawnPoints = points;
            spawnInterval = interval;
            maxActive = max;
            spawnOnStart = onStart;
            respawnOnDeath = respawn;
            playerTag = playerTagValue;
        }

        void OnTimedTick()
        {
            if (_paused || profile == null) return;
            TrySpawnOne(false);
        }

        void TrySpawnOne(bool fromDeathRespawn)
        {
            if (profile == null) return;
            var cap = Mathf.Min(maxActive, profile.maxSimultaneous);
            if (_activeCount >= cap)
            {
                EventBus.Publish(new OnTimedSpawnCapReachedEvent { MaxActive = cap });
                return;
            }

            if (Time.time < _nextSpawnAllowedTime && !fromDeathRespawn) return;

            var entry = profile.SelectWeightedEntry();
            if (entry == null || entry.prefab == null) return;

            var playerTf = SpawnerSpawnExecution.TryFindPlayerTransform(playerTag);
            var pos = SpawnerSpawnExecution.ResolveSpawnPosition(profile, spawnPoints, ref _sequentialIndex, playerTf);
            var waveDisplay = 0;
            var spawned = SpawnerSpawnExecution.SpawnFromPool(entry.prefab, pos, track =>
            {
                track.Configure(go => OnEntityEnded(go, cap), waveDisplay);
            });

            if (spawned == null) return;

            _activeCount++;
            _nextSpawnAllowedTime = Time.time + profile.respawnCooldown;
            EventBus.Publish(new OnTimedSpawnTriggeredEvent
            {
                SpawnedObj = spawned,
                Position = pos
            });
            EventBus.Publish(new OnSpawnCountChangedEvent { ActiveCount = _activeCount, MaxCount = cap });
        }

        void OnEntityEnded(GameObject go, int cap)
        {
            _activeCount = Mathf.Max(0, _activeCount - 1);
            EventBus.Publish(new OnSpawnCountChangedEvent { ActiveCount = _activeCount, MaxCount = cap });
            if (ObjectPoolManager_UMFOSS.Instance != null)
                ObjectPoolManager_UMFOSS.Instance.Release(go);

            if (!respawnOnDeath || profile == null) return;
            if (TimerUtility_UMFOSS.Instance == null) return;
            TimerUtility_UMFOSS.Instance.ScheduleOnce(spawnInterval, () =>
            {
                if (this != null && enabled) TrySpawnOne(true);
            });
        }
    }
}
