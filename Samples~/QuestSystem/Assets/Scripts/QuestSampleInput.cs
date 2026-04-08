using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Sample input: death (P), save/load. Movement and interact are blocked while dead via <see cref="QuestSamplePlayerLifecycle"/>.
    /// </summary>
    public sealed class QuestSampleInput : MonoBehaviour
    {
        private QuestSampleSaveCoordinator _save;

        private void Awake()
        {
            _save = GetComponent<QuestSampleSaveCoordinator>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                var life = QuestSamplePlayerLifecycle.Instance;
                if (life != null && life.IsAlive)
                {
                    life.Die();
                }
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                if (QuestSamplePlayerLifecycle.IsActionAllowed)
                {
                    _save?.Save();
                }
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                _save?.Load();
            }
        }
    }
}
