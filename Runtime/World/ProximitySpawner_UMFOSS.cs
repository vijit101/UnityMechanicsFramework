using System.Collections;
using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Utils;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public class ProximitySpawner_UMFOSS : MonoBehaviour
    {
        [Header("Proximity Configuration")]
        [SerializeField] SpawnProfile_UMFOSS profile;
        [SerializeField] List<SpawnPoint_UMFOSS> spawnPoints;
        [SerializeField] float triggerRadius = 5f;
        [SerializeField] bool isOneShot = true;
        [SerializeField] bool requireLineOfSight;
        [SerializeField] float cooldown;
        [SerializeField] LayerMask playerLayer = ~0;
        [SerializeField] LayerMask obstacleLayers;

        [Header("Player")]
        [SerializeField] string playerTag = "Player";

        bool _paused;
        bool _disabled;
        bool _hasFired;
        float _cooldownUntil;
        int _sequentialIndex;
        int _activeCount;
        Coroutine _burstRoutine;

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

        void Update()
        {
            if (_paused || _disabled || profile == null) return;
            if (Time.time < _cooldownUntil) return;
            if (_burstRoutine != null) return;

            Collider2D playerCol = null;
            foreach (var h in Physics2D.OverlapCircleAll(transform.position, triggerRadius, playerLayer))
            {
                if (h == null) continue;
                if (h.name == "Player" ||
                    (!string.IsNullOrEmpty(playerTag) && h.CompareTag(playerTag)))
                {
                    playerCol = h;
                    break;
                }
            }

            if (playerCol == null) return;
            if (requireLineOfSight && !HasLineOfSight(playerCol.transform.position)) return;

            _burstRoutine = StartCoroutine(SpawnBurstRoutine());
        }

        bool HasLineOfSight(Vector3 targetPos)
        {
            var origin = transform.position;
            var dir = (Vector2)(targetPos - origin);
            var dist = dir.magnitude;
            if (dist < 0.01f) return true;
            var hit = Physics2D.Raycast(origin, dir.normalized, dist, obstacleLayers);
            return hit.collider == null;
        }

        IEnumerator SpawnBurstRoutine()
        {
            var scaleKey = 0f;
            var countScale = profile.EvaluateCountScale(scaleKey);
            var delayScale = profile.EvaluateDelayScale(scaleKey);
            var playerTf = SpawnerSpawnExecution.TryFindPlayerTransform(playerTag);
            var spawnedTotal = 0;

            if (profile.entries != null)
            {
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

                                if (SpawnOne(entry.prefab, pt.GetSpawnPosition())) spawnedTotal++;
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
                                ref _sequentialIndex, playerTf);
                            if (SpawnOne(entry.prefab, pos)) spawnedTotal++;
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

            EventBus.Publish(new OnProximitySpawnTriggeredEvent
            {
                TriggerPosition = transform.position,
                SpawnCount = spawnedTotal
            });
            EventBus.Publish(new OnSpawnCountChangedEvent
            {
                ActiveCount = _activeCount,
                MaxCount = profile.maxSimultaneous
            });

            if (isOneShot)
            {
                _hasFired = true;
                _disabled = true;
            }
            else
                _cooldownUntil = Time.time + Mathf.Max(0f, cooldown);

            _burstRoutine = null;
        }

        bool SpawnOne(GameObject prefab, Vector3 pos)
        {
            var spawned = SpawnerSpawnExecution.SpawnFromPool(prefab, pos, track =>
            {
                track.Configure(OnTrackedEnded, 0);
            });
            if (spawned == null) return false;
            _activeCount++;
            return true;
        }

        void OnTrackedEnded(GameObject go)
        {
            _activeCount = Mathf.Max(0, _activeCount - 1);
            EventBus.Publish(new OnSpawnCountChangedEvent
            {
                ActiveCount = _activeCount,
                MaxCount = profile != null ? profile.maxSimultaneous : 0
            });
            if (ObjectPoolManager_UMFOSS.Instance != null)
                ObjectPoolManager_UMFOSS.Instance.Release(go);
        }

        public void Enable()
        {
            _disabled = false;
            _hasFired = false;
            EventBus.Publish(new OnProximitySpawnerResetEvent());
        }

        public void Disable()
        {
            _disabled = true;
        }

        public void ForceSpawn()
        {
            if (profile == null || _burstRoutine != null) return;
            _burstRoutine = StartCoroutine(SpawnBurstRoutine());
        }

        public bool HasFired() => _hasFired;

        /// <summary>Code-driven setup (e.g. demo/bootstrap) without Inspector references.</summary>
        public void ApplyRuntimeConfiguration(
            SpawnProfile_UMFOSS profileValue,
            List<SpawnPoint_UMFOSS> points,
            float radius,
            bool oneShot,
            bool los,
            float cooldownSeconds,
            LayerMask players,
            LayerMask obstacles,
            string playerTagValue)
        {
            profile = profileValue;
            spawnPoints = points;
            triggerRadius = radius;
            isOneShot = oneShot;
            requireLineOfSight = los;
            cooldown = cooldownSeconds;
            playerLayer = players;
            obstacleLayers = obstacles;
            playerTag = playerTagValue;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0.5f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }
    }
}
