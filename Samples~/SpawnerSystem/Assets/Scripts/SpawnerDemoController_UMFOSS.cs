using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Utils;
using GameplayMechanicsUMFOSS.World;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.SpawnerSystem
{
    /// <summary>
    /// Bootstraps pools, profiles, and all three spawner types for <c>DemoScene</c> with zero Inspector wiring.
    /// </summary>
    public class SpawnerDemoController_UMFOSS : MonoBehaviour
    {
        const string PlayerTagName = "Player";

        GameObject _enemyPrefab;
        WaveSpawner_UMFOSS _wave;
        TimedSpawner_UMFOSS _timed;
        ProximitySpawner_UMFOSS _prox;
        bool _paused;

        SpawnProfile_UMFOSS _wave1;
        SpawnProfile_UMFOSS _wave2;
        SpawnProfile_UMFOSS _wave3;
        SpawnProfile_UMFOSS _timedProfile;
        SpawnProfile_UMFOSS _proxProfile;

        static Sprite CreateWhiteSprite()
        {
            var t = Texture2D.whiteTexture;
            return Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 32f);
        }

        void Awake()
        {
            if (FindObjectOfType<ObjectPoolManager_UMFOSS>() == null)
                new GameObject(nameof(ObjectPoolManager_UMFOSS)).AddComponent<ObjectPoolManager_UMFOSS>();
            if (FindObjectOfType<TimerUtility_UMFOSS>() == null)
                new GameObject(nameof(TimerUtility_UMFOSS)).AddComponent<TimerUtility_UMFOSS>();

            _enemyPrefab = BuildEnemyPrefab();
            ObjectPoolManager_UMFOSS.Instance.WarmPool(_enemyPrefab, 48);

            var cam = Camera.main;
            if (cam != null)
            {
                cam.orthographic = true;
                cam.orthographicSize = 9f;
                cam.transform.position = new Vector3(0f, 0f, -10f);
                cam.backgroundColor = new Color(0.12f, 0.12f, 0.16f);
            }

            BuildGround();
            BuildPlayer();

            _wave1 = MakeProfile(3, 3, 0.15f);
            _wave2 = MakeProfile(4, 5, 0.12f);
            _wave3 = MakeProfile(6, 8, 0.08f);
            _timedProfile = MakeWeightedPatrolProfile();
            _proxProfile = MakeProfile(4, 4, 0.05f);

            var wavePoints = new List<SpawnPoint_UMFOSS>
            {
                CreateSpawnPoint(new Vector3(-9f, 2f, 0f)),
                CreateSpawnPoint(new Vector3(-11f, -1f, 0f))
            };
            var timedPoints = new List<SpawnPoint_UMFOSS> { CreateSpawnPoint(new Vector3(0f, 1.5f, 0f)) };
            var proxPoints = new List<SpawnPoint_UMFOSS> { CreateSpawnPoint(new Vector3(9f, 1f, 0f)) };

            var waveGo = new GameObject("WaveSpawner");
            waveGo.transform.position = new Vector3(-9f, 0f, 0f);
            _wave = waveGo.AddComponent<WaveSpawner_UMFOSS>();
            _wave.ApplyRuntimeConfiguration(
                new[] { _wave1, _wave2, _wave3 },
                wavePoints,
                betweenWaves: 4f,
                loop: false,
                clear: WaveClearCondition.AllDead,
                timedDur: 30f,
                playerTagValue: PlayerTagName);

            var timedGo = new GameObject("TimedSpawner");
            timedGo.transform.position = new Vector3(0f, 0f, 0f);
            _timed = timedGo.AddComponent<TimedSpawner_UMFOSS>();
            _timed.ApplyRuntimeConfiguration(
                _timedProfile,
                timedPoints,
                interval: 4f,
                max: 3,
                onStart: true,
                respawn: true,
                playerTagValue: PlayerTagName);

            var proxGo = new GameObject("ProximitySpawner");
            proxGo.transform.position = new Vector3(9f, 0f, 0f);
            _prox = proxGo.AddComponent<ProximitySpawner_UMFOSS>();
            _prox.ApplyRuntimeConfiguration(
                _proxProfile,
                proxPoints,
                radius: 4f,
                oneShot: true,
                los: false,
                cooldownSeconds: 5f,
                players: ~0,
                obstacles: 0,
                playerTagValue: PlayerTagName);
        }

        void Start()
        {
            _wave.StartWaves();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                _paused = !_paused;
                EventBus.Publish(new GamePausedEvent { IsPaused = _paused });
            }

            if (Input.GetKeyDown(KeyCode.K)) KillAllSpawned();
        }

        void KillAllSpawned()
        {
            foreach (var t in FindObjectsOfType<SpawnerTrackedEntity_UMFOSS>())
                t.NotifyLifecycleEnded();
        }

        void OnGUI()
        {
            const int w = 420;
            GUI.Box(new Rect(8, 8, w, 130), "Spawner System Demo (Issue #25)");
            GUI.Label(new Rect(16, 32, w - 16, 22),
                $"WASD — move player   |   P — pause ({(_paused ? "paused" : "running")})   |   K — kill all spawned");
            if (_wave != null)
                GUI.Label(new Rect(16, 58, w - 16, 22),
                    $"Wave: current={_wave.GetCurrentWave()}  active={_wave.GetActiveCount()}  waveActive={_wave.IsWaveActive()}");
            if (_timed != null)
                GUI.Label(new Rect(16, 82, w - 16, 22), $"Timed: active={_timed.GetActiveCount()}");
            if (_prox != null)
                GUI.Label(new Rect(16, 106, w - 16, 22),
                    $"Proximity: hasFired={_prox.HasFired()}  (enter right zone)");
        }

        static GameObject BuildEnemyPrefab()
        {
            var go = new GameObject("EnemyPrefab");
            go.SetActive(false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSprite();
            sr.color = new Color(0.95f, 0.35f, 0.35f);
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(0.5f, 0.5f);
            go.AddComponent<SpawnerDemoEnemy_UMFOSS>();
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            return go;
        }

        void BuildGround()
        {
            var ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, -4.5f, 0f);
            var sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSprite();
            sr.color = new Color(0.22f, 0.24f, 0.28f);
            ground.transform.localScale = new Vector3(24f, 1.2f, 1f);
            var box = ground.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 1f);
        }

        void BuildPlayer()
        {
            var p = new GameObject("Player");
            p.transform.position = new Vector3(-4f, -3f, 0f);
            var sr = p.AddComponent<SpriteRenderer>();
            sr.sprite = CreateWhiteSprite();
            sr.color = new Color(0.35f, 0.85f, 1f);
            p.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            var rb = p.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            var box = p.AddComponent<BoxCollider2D>();
            box.size = new Vector2(0.45f, 0.45f);
            p.AddComponent<SpawnerDemoPlayer_UMFOSS>();
        }

        SpawnPoint_UMFOSS CreateSpawnPoint(Vector3 worldPos)
        {
            var go = new GameObject("SpawnPoint");
            go.transform.position = worldPos;
            return go.AddComponent<SpawnPoint_UMFOSS>();
        }

        SpawnProfile_UMFOSS MakeProfile(int min, int max, float delay)
        {
            var p = ScriptableObject.CreateInstance<SpawnProfile_UMFOSS>();
            p.entries = new[]
            {
                new SpawnProfile_UMFOSS.SpawnEntry
                {
                    prefab = _enemyPrefab,
                    minCount = min,
                    maxCount = max,
                    weight = 1f,
                    spawnDelay = delay
                }
            };
            p.spawnPointMode = SpawnPointMode.Random;
            p.maxSimultaneous = 12;
            p.respawnCooldown = 0f;
            p.countScaleCurve = AnimationCurve.Constant(0f, 1f, 1f);
            p.delayScaleCurve = AnimationCurve.Constant(0f, 1f, 1f);
            return p;
        }

        SpawnProfile_UMFOSS MakeWeightedPatrolProfile()
        {
            var p = ScriptableObject.CreateInstance<SpawnProfile_UMFOSS>();
            p.entries = new[]
            {
                new SpawnProfile_UMFOSS.SpawnEntry
                {
                    prefab = _enemyPrefab,
                    minCount = 1,
                    maxCount = 1,
                    weight = 5f,
                    spawnDelay = 0f
                },
                new SpawnProfile_UMFOSS.SpawnEntry
                {
                    prefab = _enemyPrefab,
                    minCount = 1,
                    maxCount = 1,
                    weight = 2f,
                    spawnDelay = 0f
                }
            };
            p.spawnPointMode = SpawnPointMode.Random;
            p.maxSimultaneous = 6;
            p.countScaleCurve = AnimationCurve.Constant(0f, 1f, 1f);
            p.delayScaleCurve = AnimationCurve.Constant(0f, 1f, 1f);
            return p;
        }
    }
}
