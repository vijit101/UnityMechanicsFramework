using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// In-world interactable: press E in range to reset the camp encounter and start
    /// <see cref="QuestSampleRuntimeSetup.ClearTheCampQuest"/> when <see cref="QuestManager_UMFOSS.CanStartQuest"/> allows it.
    /// </summary>
    public sealed class QuestSampleQuestStartPoint : MonoBehaviour
    {
        [SerializeField]
        private KeyCode interactKey = KeyCode.E;

        private bool _playerInRange;

        private void OnTriggerEnter(Collider other)
        {
            if (other != null && other.GetComponentInParent<CharacterController>() != null)
            {
                _playerInRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other != null && other.GetComponentInParent<CharacterController>() != null)
            {
                _playerInRange = false;
            }
        }

        private void Update()
        {
            if (!QuestSamplePlayerLifecycle.IsActionAllowed)
            {
                return;
            }

            if (!_playerInRange || !Input.GetKeyDown(interactKey))
            {
                return;
            }

            var q = QuestSampleRuntimeSetup.ClearTheCampQuest;
            var m = QuestManager_UMFOSS.Instance;
            if (q == null || m == null)
            {
                return;
            }

            if (!m.CanStartQuest(q))
            {
                return;
            }

            QuestSampleClearCampEncounter.Reset();
            m.StartQuest(q);
        }
    }
}
