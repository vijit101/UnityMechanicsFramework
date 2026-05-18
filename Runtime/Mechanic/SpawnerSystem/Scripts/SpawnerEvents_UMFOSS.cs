using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    // --- Enums ---

    public enum SpawnShape { Point, Circle, Rectangle }

    public enum SpawnPointMode { Random, Sequential, Nearest, All }

    public enum WaveClearCondition { AllDead, TimedEnd, Manual }

    // --- Wave Spawner Events ---

    public struct OnWaveStarted
    {
        public int waveNumber;
        public int totalWaves;
    }

    public struct OnWaveCleared
    {
        public int waveNumber;
        public float timeTaken;
    }

    public struct OnAllWavesComplete
    {
        public int totalWaves;
    }

    public struct OnSpawnedObjectDied
    {
        public GameObject obj;
        public int waveNumber;
        public int remainingCount;
    }

    // --- Timed Spawner Events ---

    public struct OnTimedSpawnTriggered
    {
        public GameObject spawnedObj;
        public Vector3 position;
    }

    public struct OnTimedSpawnCapReached
    {
        public int maxActive;
    }

    // --- Proximity Spawner Events ---

    public struct OnProximitySpawnTriggered
    {
        public Vector3 triggerPosition;
        public int spawnCount;
    }

    public struct OnProximitySpawnerReset { }

    // --- Shared Events ---

    public struct OnSpawnerStarted
    {
        public GameObject spawner;
    }

    public struct OnSpawnerStopped
    {
        public GameObject spawner;
    }

    public struct OnSpawnCountChanged
    {
        public int activeCount;
        public int maxCount;
    }
}
