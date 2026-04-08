using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Package entry point for the Quest / Objective system. Add alongside <see cref="QuestManager_UMFOSS"/> on a bootstrap GameObject.
    /// </summary>
    [RequireComponent(typeof(QuestManager_UMFOSS))]
    [DisallowMultipleComponent]
    public sealed class QuestSystem_UMFOSS : MonoBehaviour
    {
        /// <summary>Runtime quest manager singleton.</summary>
        public QuestManager_UMFOSS Manager => GetComponent<QuestManager_UMFOSS>();
    }
}
