using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// NPC stand-in: staged <see cref="QuestSampleRuntimeSetup.EventInteract"/> — start quest + talk phase, then return only after ore is collected.
    /// </summary>
    public sealed class QuestSampleMerchant : MonoBehaviour
    {
        public const string DefaultObjectId = "Merchant";

        [SerializeField]
        private string objectId = DefaultObjectId;

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

            TryInteractMerchant();
        }

        private void TryInteractMerchant()
        {
            var m = QuestManager_UMFOSS.Instance;
            var q = QuestSampleRuntimeSetup.MerchantsRequestQuest;
            if (m == null || q == null)
            {
                return;
            }

            var inst = m.GetQuestInstance(q);
            if (inst == null)
            {
                if (!m.CanStartQuest(q))
                {
                    return;
                }

                if (!m.StartQuest(q))
                {
                    return;
                }

                PublishMerchantPhase("Talk");
                return;
            }

            var talk = FindObjective(inst, "TalkMerchant");
            var ore = FindObjective(inst, "IronOre");
            var ret = FindObjective(inst, "ReturnMerchant");
            if (talk == null || ore == null || ret == null)
            {
                return;
            }

            if (!talk.IsComplete())
            {
                PublishMerchantPhase("Talk");
                return;
            }

            if (!ore.IsComplete())
            {
                return;
            }

            if (!ret.IsComplete())
            {
                PublishMerchantPhase("Return");
            }
        }

        private static ObjectiveInstance_UMFOSS FindObjective(QuestInstance_UMFOSS inst, string objectiveId)
        {
            foreach (var o in inst.Objectives)
            {
                if (o.Data.objectiveID == objectiveId)
                {
                    return o;
                }
            }

            return null;
        }

        private void PublishMerchantPhase(string phase)
        {
            QuestSampleGameEventHelper.Publish(QuestSampleRuntimeSetup.EventInteract,
                new Dictionary<string, string>
                {
                    { "objectID", objectId },
                    { "merchantPhase", phase }
                });
        }
    }
}
