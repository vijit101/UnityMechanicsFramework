using System.Collections;
using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Sample-only death / respawn: publishes quest death payload, locks gameplay until respawn (button or 5s).
    /// </summary>
    public sealed class QuestSamplePlayerLifecycle : MonoBehaviour
    {
        public static QuestSamplePlayerLifecycle Instance { get; private set; }

        [SerializeField]
        private float respawnDelaySeconds = 5f;

        private CharacterController _controller;

        private Vector3 _spawnPosition;

        private Quaternion _spawnRotation;

        private Coroutine _respawnRoutine;

        public bool IsAlive { get; private set; } = true;

        /// <summary>Seconds until auto-respawn while dead; 0 when alive.</summary>
        public float RespawnTimeRemaining { get; private set; }

        public event System.Action Died;

        public event System.Action Respawned;

        /// <summary>Gameplay scripts should block input when the player is dead.</summary>
        public static bool IsActionAllowed => Instance == null || Instance.IsAlive;

        private void Awake()
        {
            Instance = this;
            _controller = GetComponent<CharacterController>();
            _spawnPosition = transform.position;
            _spawnRotation = transform.rotation;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>Publishes <see cref="QuestManager_UMFOSS.PlayerDeathEventType"/> and starts respawn flow.</summary>
        public void Die()
        {
            if (!IsAlive)
            {
                return;
            }

            IsAlive = false;
            RespawnTimeRemaining = respawnDelaySeconds;
            QuestSampleGameEventHelper.Publish(QuestManager_UMFOSS.PlayerDeathEventType,
                new Dictionary<string, string> { { "isPlayer", "true" } });
            Died?.Invoke();
            if (_respawnRoutine != null)
            {
                StopCoroutine(_respawnRoutine);
            }

            _respawnRoutine = StartCoroutine(RespawnCountdownRoutine());
        }

        public void RequestImmediateRespawn()
        {
            if (!IsAlive)
            {
                if (_respawnRoutine != null)
                {
                    StopCoroutine(_respawnRoutine);
                    _respawnRoutine = null;
                }

                RespawnNow();
            }
        }

        private IEnumerator RespawnCountdownRoutine()
        {
            var end = Time.unscaledTime + respawnDelaySeconds;
            while (Time.unscaledTime < end && !IsAlive)
            {
                RespawnTimeRemaining = Mathf.Max(0f, end - Time.unscaledTime);
                yield return null;
            }

            if (!IsAlive)
            {
                RespawnNow();
            }

            _respawnRoutine = null;
        }

        private void RespawnNow()
        {
            IsAlive = true;
            RespawnTimeRemaining = 0f;
            if (_controller != null)
            {
                _controller.enabled = false;
            }

            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            Physics.SyncTransforms();
            if (_controller != null)
            {
                _controller.enabled = true;
            }

            Respawned?.Invoke();
        }

        /// <summary>Call from bootstrap if you relocate spawn (e.g. after loading save position).</summary>
        public void SetSpawnPoint(Vector3 position, Quaternion rotation)
        {
            _spawnPosition = position;
            _spawnRotation = rotation;
        }

        public void TeleportToSpawnWithoutEvent()
        {
            if (_controller != null)
            {
                _controller.enabled = false;
            }

            transform.SetPositionAndRotation(_spawnPosition, _spawnRotation);
            Physics.SyncTransforms();
            if (_controller != null)
            {
                _controller.enabled = true;
            }
        }
    }
}
