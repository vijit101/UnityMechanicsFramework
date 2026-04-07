using UnityEngine;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Samples.TimerUtility
{
    /// <summary>
    /// Demo 3: Looping Spawn Timer
    /// Uses TimerUtility_UMFOSS.CreateLooping() to spawn a prefab every 3 seconds.
    /// Stops automatically after 5 spawn cycles using OnAllLoopsComplete.
    /// Demonstrates the static factory pattern — no inspector timer component needed.
    /// Attach to an empty GameObject in the scene. Assign the spawnPrefab and spawnPoint.
    /// </summary>
    public class LoopingSpawnDemo : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject spawnPrefab;
        [SerializeField] private Transform  spawnPoint;
        [SerializeField] private float      spawnInterval  = 3f;
        [SerializeField] private int        maxSpawnCount  = 5;

        [Header("UI")]
        [SerializeField] private UnityEngine.UI.Text spawnCountLabel;

        private TimerUtility_UMFOSS spawnTimer;
        private int                 spawnedCount = 0;

        private void Start()
        {
            UpdateLabel();

            // Create a looping timer from code — no inspector component needed
            spawnTimer = TimerUtility_UMFOSS.CreateLooping(
                interval: spawnInterval,
                onTick:   SpawnObject
            );

            // Wire LoopComplete to track iteration count and stop at maxSpawnCount
            spawnTimer.OnLoopComplete += OnSpawnCycle;
        }

        private void OnDestroy()
        {
            if (spawnTimer != null)
                spawnTimer.OnLoopComplete -= OnSpawnCycle;
        }

        private void SpawnObject()
        {
            if (spawnedCount >= maxSpawnCount) return;

            Vector3 pos = spawnPoint != null
                ? spawnPoint.position
                : transform.position + Vector3.up;

            Instantiate(spawnPrefab, pos, Quaternion.identity);
            spawnedCount++;
            UpdateLabel();

            // Stop after reaching max count
            if (spawnedCount >= maxSpawnCount)
                spawnTimer.Stop();
        }

        private void OnSpawnCycle(int loopIndex)
        {
            // loopIndex is zero-based — log for demo clarity
            Debug.Log($"[LoopingSpawnDemo] Spawn cycle {loopIndex + 1}/{maxSpawnCount}");
        }

        private void UpdateLabel()
        {
            if (spawnCountLabel != null)
                spawnCountLabel.text = $"Spawned: {spawnedCount} / {maxSpawnCount}";
        }
    }
}
