using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public static class SpawnHelper_UMFOSS
    {
        /// <summary>
        /// Weighted random selection from spawn entries.
        /// Higher weight = higher probability of being selected.
        /// Algorithm: sum all weights, pick random in [0, total], walk entries
        /// accumulating weight until random value is reached.
        /// </summary>
        public static SpawnProfile_UMFOSS.SpawnEntry SelectWeightedRandom(
            SpawnProfile_UMFOSS.SpawnEntry[] entries)
        {
            float totalWeight = 0f;
            for (int i = 0; i < entries.Length; i++)
                totalWeight += entries[i].weight;

            float randomValue = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            for (int i = 0; i < entries.Length; i++)
            {
                cumulative += entries[i].weight;
                if (randomValue <= cumulative)
                    return entries[i];
            }

            return entries[entries.Length - 1];
        }

        /// <summary>
        /// Get a spawn position based on the profile's SpawnPointMode.
        /// </summary>
        public static Vector3 GetSpawnPosition(
            SpawnPointMode mode,
            List<SpawnPoint_UMFOSS> spawnPoints,
            ref int sequentialIndex,
            Vector3 playerPosition)
        {
            if (spawnPoints == null || spawnPoints.Count == 0)
                return Vector3.zero;

            switch (mode)
            {
                case SpawnPointMode.Random:
                    return spawnPoints[Random.Range(0, spawnPoints.Count)]
                        .GetSpawnPosition();

                case SpawnPointMode.Sequential:
                    var point = spawnPoints[sequentialIndex % spawnPoints.Count];
                    sequentialIndex++;
                    return point.GetSpawnPosition();

                case SpawnPointMode.Nearest:
                    SpawnPoint_UMFOSS nearest = spawnPoints[0];
                    float minDist = float.MaxValue;
                    for (int i = 0; i < spawnPoints.Count; i++)
                    {
                        float dist = Vector3.Distance(
                            spawnPoints[i].transform.position, playerPosition);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            nearest = spawnPoints[i];
                        }
                    }
                    return nearest.GetSpawnPosition();

                case SpawnPointMode.All:
                    return spawnPoints[0].GetSpawnPosition();

                default:
                    return spawnPoints[0].GetSpawnPosition();
            }
        }

        /// <summary>
        /// Calculate spawn count with difficulty scaling applied.
        /// Flat curve at 1.0 = no scaling.
        /// </summary>
        public static int GetScaledCount(
            SpawnProfile_UMFOSS.SpawnEntry entry,
            AnimationCurve countScaleCurve,
            float progressNormalized)
        {
            int baseCount = Random.Range(entry.minCount, entry.maxCount + 1);
            float scale = countScaleCurve.Evaluate(progressNormalized);
            return Mathf.Max(1, Mathf.RoundToInt(baseCount * scale));
        }
    }
}
