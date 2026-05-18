using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    [CreateAssetMenu(fileName = "NewSpawnProfile", menuName = "UMFOSS/World/SpawnProfile")]
    public class SpawnProfile_UMFOSS : ScriptableObject
    {
        [System.Serializable]
        public class SpawnEntry
        {
            public GameObject prefab;
            public int minCount = 1;
            public int maxCount = 1;
            [Range(0.1f, 100f)]
            public float weight = 1f;
            public float spawnDelay = 0f;
        }

        [Header("Spawn Entries")]
        public SpawnEntry[] entries;

        [Header("Spawn Points")]
        public SpawnPointMode spawnPointMode = SpawnPointMode.Random;

        [Header("Limits")]
        public int maxSimultaneous = 10;
        public float respawnCooldown = 0f;

        [Header("Difficulty Scaling")]
        public AnimationCurve countScaleCurve = AnimationCurve.Constant(0f, 1f, 1f);
        public AnimationCurve delayScaleCurve = AnimationCurve.Constant(0f, 1f, 1f);
    }
}
