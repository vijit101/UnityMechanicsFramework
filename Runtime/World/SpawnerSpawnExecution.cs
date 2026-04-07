using System;
using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Utils;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    /// <summary>Shared spawn helpers for all spawner types.</summary>
    public static class SpawnerSpawnExecution
    {
        public static Transform TryFindPlayerTransform(string playerTag)
        {
            if (!string.IsNullOrEmpty(playerTag))
            {
                try
                {
                    var tagged = GameObject.FindGameObjectWithTag(playerTag);
                    if (tagged != null) return tagged.transform;
                }
                catch (UnityException)
                {
                    // Tag may be missing from the project TagManager.
                }
            }

            var named = GameObject.Find("Player");
            return named != null ? named.transform : null;
        }

        public static Vector3 ResolveSpawnPosition(
            SpawnProfile_UMFOSS profile,
            IList<SpawnPoint_UMFOSS> spawnPoints,
            ref int sequentialIndex,
            Transform playerTransform)
        {
            if (spawnPoints == null || spawnPoints.Count == 0) return Vector3.zero;
            switch (profile.spawnPointMode)
            {
                case SpawnPointMode.Random:
                    return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].GetSpawnPosition();
                case SpawnPointMode.Sequential:
                {
                    var p = spawnPoints[sequentialIndex % spawnPoints.Count];
                    sequentialIndex++;
                    return p.GetSpawnPosition();
                }
                case SpawnPointMode.Nearest:
                {
                    if (playerTransform == null)
                        return spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Count)].GetSpawnPosition();
                    var best = spawnPoints[0];
                    var bestD = (best.transform.position - playerTransform.position).sqrMagnitude;
                    for (var i = 1; i < spawnPoints.Count; i++)
                    {
                        var d = (spawnPoints[i].transform.position - playerTransform.position).sqrMagnitude;
                        if (d < bestD)
                        {
                            bestD = d;
                            best = spawnPoints[i];
                        }
                    }

                    return best.GetSpawnPosition();
                }
                case SpawnPointMode.All:
                default:
                    return spawnPoints[0].GetSpawnPosition();
            }
        }

        public static GameObject SpawnFromPool(
            GameObject prefab,
            Vector3 position,
            Action<SpawnerTrackedEntity_UMFOSS> configureTracked)
        {
            if (ObjectPoolManager_UMFOSS.Instance == null || prefab == null) return null;
            var go = ObjectPoolManager_UMFOSS.Instance.Get(prefab, position, Quaternion.identity);
            var track = go.GetComponent<SpawnerTrackedEntity_UMFOSS>();
            if (track == null) track = go.AddComponent<SpawnerTrackedEntity_UMFOSS>();
            configureTracked?.Invoke(track);
            return go;
        }
    }
}
