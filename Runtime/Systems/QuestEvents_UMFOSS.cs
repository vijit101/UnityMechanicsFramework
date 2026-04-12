namespace GameplayMechanicsUMFOSS.Systems
{
    public class QuestStartedEvent_UMFOSS
    {
        public QuestInstance_UMFOSS Quest { get; set; }
    }

    public class QuestStartFailedEvent_UMFOSS
    {
        public QuestData_UMFOSS Quest { get; set; }

        public string Reason { get; set; }
    }

    public class QuestCompletedEvent_UMFOSS
    {
        public QuestInstance_UMFOSS Quest { get; set; }
    }

    public class QuestFailedEvent_UMFOSS
    {
        public QuestInstance_UMFOSS Quest { get; set; }

        public string Reason { get; set; }
    }

    public class QuestAbandonedEvent_UMFOSS
    {
        public QuestInstance_UMFOSS Quest { get; set; }
    }

    public class ObjectiveStartedEvent_UMFOSS
    {
        public QuestInstance_UMFOSS Quest { get; set; }

        public ObjectiveInstance_UMFOSS Objective { get; set; }
    }

    public class ObjectiveProgressEvent_UMFOSS
    {
        public QuestInstance_UMFOSS Quest { get; set; }

        public ObjectiveInstance_UMFOSS Objective { get; set; }

        public int NewCount { get; set; }
    }

    public class ObjectiveCompletedEvent_UMFOSS
    {
        public QuestInstance_UMFOSS Quest { get; set; }

        public ObjectiveInstance_UMFOSS Objective { get; set; }
    }

    public class QuestRewardGrantedEvent_UMFOSS
    {
        public QuestInstance_UMFOSS Quest { get; set; }

        public int Experience { get; set; }

        public int Currency { get; set; }

        public ItemData_UMFOSS[] Items { get; set; }
    }

    public class QuestUnlockedEvent_UMFOSS
    {
        public QuestData_UMFOSS Quest { get; set; }
    }
}
