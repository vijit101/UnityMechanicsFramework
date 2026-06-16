using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Systems
{
    public class CheckpointManager_UMFOSS : MonoBehaviour
    {
        public static CheckpointManager_UMFOSS Instance { get; private set; }
        [Header("Respawn Settings")]
        [SerializeField] private float respawnDelay = 0f;
        [SerializeField] private float respawnHealthPercent = 1f;
        [SerializeField] private bool keepInventoryOnDeath = true;

        [SerializeField] private Checkpoint_UMFOSS startingCheckpoint;

        private Checkpoint_UMFOSS activeCheckpoint;
        private List<Checkpoint_UMFOSS> allCheckpoints = new List<Checkpoint_UMFOSS>();

        private GameObject player;
        private bool isRespawning;

        // Subscriptions
        private System.Action<DeathEvent> onDeathSubscription;
        private System.Action<OnCheckpointActivated> onCheckpointActivatedSubscription;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
            
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }
        }

        private void OnEnable()
        {
            onDeathSubscription = OnPlayerDeath;
            onCheckpointActivatedSubscription = OnCheckpointActivatedCallback;
            EventBus.Subscribe(onDeathSubscription);
            EventBus.Subscribe(onCheckpointActivatedSubscription);
        }

        private void OnDisable()
        {
            if (onDeathSubscription != null) EventBus.Unsubscribe(onDeathSubscription);
            if (onCheckpointActivatedSubscription != null) EventBus.Unsubscribe(onCheckpointActivatedSubscription);
        }

        public void RegisterPlayer(GameObject playerGO)
        {
            player = playerGO;
        }

        public void RegisterCheckpoint(Checkpoint_UMFOSS checkpoint)
        {
            if (!allCheckpoints.Contains(checkpoint))
            {
                allCheckpoints.Add(checkpoint);
            }
        }

        public void SetActiveCheckpoint(Checkpoint_UMFOSS checkpoint)
        {
            if (activeCheckpoint != null && activeCheckpoint != checkpoint)
            {
                activeCheckpoint.Deactivate();
            }
            activeCheckpoint = checkpoint;
        }

        private void OnCheckpointActivatedCallback(OnCheckpointActivated e)
        {
            SetActiveCheckpoint(e.checkpoint);
        }

        public void TriggerRespawn()
        {
            if (isRespawning) return;
            isRespawning = true;
            StartCoroutine(RespawnSequence());
        }

        public Checkpoint_UMFOSS GetActiveCheckpoint()
        {
            return activeCheckpoint;
        }

        public Vector3 GetRespawnPosition()
        {
            if (activeCheckpoint != null) return activeCheckpoint.RespawnPoint.position;
            if (startingCheckpoint != null) return startingCheckpoint.RespawnPoint.position;
            
            Debug.LogWarning("No active or starting checkpoint set! Respawning at Vector3.zero.");
            return Vector3.zero;
        }

        private void OnPlayerDeath(DeathEvent e)
        {
            Debug.Log("[CheckpointManager] Received DeathEvent for: " + (e.source != null ? e.source.name : "null"));
            if (e.source != null && e.source.CompareTag("Player"))
            {
                TriggerRespawn();
            }
        }

        private IEnumerator RespawnSequence()
        {
            Vector3 deathPos = player != null ? player.transform.position : Vector3.zero;
            EventBus.Publish(new OnRespawnStarted { deathPosition = deathPos });

            if (respawnDelay > 0)
            {
                yield return new WaitForSeconds(respawnDelay);
            }

            if (player != null)
            {
                player.transform.position = GetRespawnPosition();

                // Reset physics velocity so they don't immediately plummet back down
                var rb2d = player.GetComponent<Rigidbody2D>();
                if (rb2d != null) rb2d.velocity = Vector2.zero;

                var health = player.GetComponent<HealthSystem_UMFOSS>();
                if (health != null)
                {
                    health.Heal(health.GetMaxHealth() * respawnHealthPercent);
                }

                if (!keepInventoryOnDeath)
                {
                    var inventory = player.GetComponent<InventorySystem_UMFOSS>();
                    if (inventory != null)
                    {
                        inventory.ClearInventory();
                    }
                }
            }

            EventBus.Publish(new OnRespawnComplete { respawnPosition = GetRespawnPosition() });
            isRespawning = false;
        }

        public void ResetAllCheckpoints()
        {
            foreach (var checkpoint in allCheckpoints)
            {
                if (checkpoint != null) checkpoint.Deactivate();
            }

            activeCheckpoint = startingCheckpoint;
            EventBus.Publish(new OnAllCheckpointsCleared());
        }
    }
}
