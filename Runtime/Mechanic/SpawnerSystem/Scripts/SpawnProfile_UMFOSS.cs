using System;
using System.Linq;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    [CreateAssetMenu(fileName = "NewSpawnProfile", menuName = "UMFOSS/World/SpawnProfile")]
    public class SpawnProfile_UMFOSS : ScriptableObject
    {
        [Serializable]
        public class SpawnEntry
        {
            [Tooltip("Must be registered with ObjectPoolManager — never Instantiate at gameplay.")]
            public GameObject prefab;

            public int minCount = 1;
            public int maxCount = 1;
            public float weight = 1f;
            public float spawnDelay = 0f;
        }

        [Header("Spawn Entries")]
        public SpawnEntry[] entries;

        [Header("Spawn Points")]
        public SpawnPointMode spawnPointMode;

        [Header("Limits")]
        public int maxSimultaneous = 10;
        public float respawnCooldown = 0f;

        [Header("Difficulty Scaling")]
        [Tooltip("Y value multiplies spawn counts. Flat line at 1 = no scaling.")]
        public AnimationCurve countScaleCurve;

        [Tooltip("Y value scales delay curves (lower delay when curve is higher).")]
        public AnimationCurve delayScaleCurve;

        /// <summary>Weighted random selection — not uniform index pick.</summary>
        public SpawnEntry SelectWeightedEntry()
        {
            if (entries == null || entries.Length == 0) return null;
            var totalWeight = entries.Where(e => e != null && e.prefab != null && e.weight > 0f).Sum(e => e.weight);
            if (totalWeight <= 0f) return null;
            var roll = UnityEngine.Random.Range(0f, totalWeight);
            var cumulative = 0f;
            foreach (var entry in entries)
            {
                if (entry == null || entry.prefab == null || entry.weight <= 0f) continue;
                cumulative += entry.weight;
                if (roll <= cumulative) return entry;
            }

            for (var i = entries.Length - 1; i >= 0; i--)
            {
                if (entries[i] != null && entries[i].prefab != null) return entries[i];
            }

            return null;
        }

        public float EvaluateCountScale(float key)
        {
            if (countScaleCurve == null || countScaleCurve.keys == null || countScaleCurve.length == 0)
                return 1f;
            return Mathf.Max(0.01f, countScaleCurve.Evaluate(key));
        }

        public float EvaluateDelayScale(float key)
        {
            if (delayScaleCurve == null || delayScaleCurve.keys == null || delayScaleCurve.length == 0)
                return 1f;
            return Mathf.Max(0.01f, delayScaleCurve.Evaluate(key));
        }
    }
}
