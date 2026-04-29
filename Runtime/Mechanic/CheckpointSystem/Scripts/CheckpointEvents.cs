using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    public struct OnCheckpointActivated 
    { 
        public Checkpoint_UMFOSS checkpoint; 
        public GameObject activator; 
    }

    public struct OnCheckpointDeactivated 
    { 
        public Checkpoint_UMFOSS checkpoint; 
    }

    public struct OnRespawnStarted 
    { 
        public Vector3 deathPosition; 
    }

    public struct OnRespawnComplete 
    { 
        public Vector3 respawnPosition; 
    }

    public struct OnAllCheckpointsCleared 
    { 
    }
}
