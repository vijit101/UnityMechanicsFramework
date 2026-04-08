using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    [CreateAssetMenu(fileName = "NewQuest", menuName = "UMFOSS/Quest/QuestData")]
    public class QuestData_UMFOSS : ScriptableObject
    {
        [Header("Identity")]
        public string questID;

        public string title;
        public string description;
        public Sprite icon;
        public QuestCategory category;

        [Header("Objectives")]
        public ObjectiveData_UMFOSS[] objectives;

        [Header("Prerequisites")]
        public QuestData_UMFOSS[] requiredQuests;

        [Tooltip("0 = no level requirement")]
        public int requiredLevel;

        [Header("Rewards")]
        public int experienceReward;

        public int currencyReward;
        public ItemData_UMFOSS[] itemRewards;
        public QuestData_UMFOSS[] unlockedQuests;

        [Header("Settings")]
        public bool isRepeatable;

        public bool autoStart;

        public bool failOnDeath;
    }
}
