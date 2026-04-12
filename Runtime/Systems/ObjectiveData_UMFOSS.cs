using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    [CreateAssetMenu(fileName = "NewObjective", menuName = "UMFOSS/Quest/ObjectiveData")]
    public class ObjectiveData_UMFOSS : ScriptableObject
    {
        [Header("Identity")]
        public string objectiveID;

        public string displayText;
        public Sprite icon;

        [Header("Completion")]
        public ObjectiveType type;

        public int requiredCount = 1;

        [Header("Event Matching")]
        [Tooltip("EventBus payload EventType, e.g. EnemyDiedEvent")]
        public string eventTypeKey;

        [Tooltip("Property key in GameEventPayload.Properties, e.g. enemyType")]
        public string filterKey;

        [Tooltip("Required property value; leave empty to match any value when filterKey is set, or any event when filterKey is empty")]
        public string filterValue;

        [Header("Optional")]
        public bool isOptional;

        public bool isHidden;

        public string hintText;
    }
}
