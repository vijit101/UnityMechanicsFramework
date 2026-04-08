using System;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Sample-only save blob: quest manager state plus player position and consumed world entities.
    /// </summary>
    [Serializable]
    public sealed class QuestSampleFullSave
    {
        public QuestSaveData_UMFOSS quest;

        public float playerX;

        public float playerY;

        public float playerZ;

        public string[] consumedIds = Array.Empty<string>();
    }
}
