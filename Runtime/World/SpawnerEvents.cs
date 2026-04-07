using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public struct OnWaveStartedEvent
    {
        public int WaveNumber;
        public int TotalWaves;
    }

    public struct OnWaveClearedEvent
    {
        public int WaveNumber;
        public float TimeTaken;
    }

    public struct OnAllWavesCompleteEvent
    {
        public int TotalWaves;
    }

    public struct OnSpawnedObjectDiedEvent
    {
        public GameObject Obj;
        public int WaveNumber;
        public int RemainingCount;
    }

    public struct OnTimedSpawnTriggeredEvent
    {
        public GameObject SpawnedObj;
        public Vector3 Position;
    }

    public struct OnTimedSpawnCapReachedEvent
    {
        public int MaxActive;
    }

    public struct OnProximitySpawnTriggeredEvent
    {
        public Vector3 TriggerPosition;
        public int SpawnCount;
    }

    public struct OnProximitySpawnerResetEvent { }

    public struct OnSpawnerStartedEvent
    {
        public GameObject Spawner;
    }

    public struct OnSpawnerStoppedEvent
    {
        public GameObject Spawner;
    }

    public struct OnSpawnCountChangedEvent
    {
        public int ActiveCount;
        public int MaxCount;
    }
}
