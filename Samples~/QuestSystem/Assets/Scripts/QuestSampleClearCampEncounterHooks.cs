using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// When Clear The Camp fails (e.g. death), restores camp enemies and pickups for the next attempt.
    /// </summary>
    public sealed class QuestSampleClearCampEncounterHooks : MonoBehaviour
    {
        private void OnEnable()
        {
            EventBus.Subscribe<QuestFailedEvent_UMFOSS>(OnQuestFailed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<QuestFailedEvent_UMFOSS>(OnQuestFailed);
        }

        private void OnQuestFailed(QuestFailedEvent_UMFOSS e)
        {
            if (e?.Quest?.Data != null &&
                e.Quest.Data.questID == QuestSampleRuntimeSetup.ClearTheCampQuestId)
            {
                QuestSampleClearCampEncounter.Reset();
            }
        }
    }
}
