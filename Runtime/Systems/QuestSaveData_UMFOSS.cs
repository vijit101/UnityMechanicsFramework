using System;

namespace GameplayMechanicsUMFOSS.Systems
{
    [Serializable]
    public class QuestSaveData_UMFOSS
    {
        public QuestSaveEntry_UMFOSS[] activeQuests = Array.Empty<QuestSaveEntry_UMFOSS>();

        public string[] completedQuestIDs = Array.Empty<string>();
    }

    [Serializable]
    public class QuestSaveEntry_UMFOSS
    {
        public string questID;

        public QuestState state;

        public ObjectiveSaveEntry_UMFOSS[] objectives = Array.Empty<ObjectiveSaveEntry_UMFOSS>();
    }

    [Serializable]
    public class ObjectiveSaveEntry_UMFOSS
    {
        public string objectiveID;

        public int currentCount;

        public bool isRevealed;
    }
}
