using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Stand-in combat: press attack near a goblin to publish kill events (quest-agnostic).
    /// </summary>
    public sealed class QuestSamplePlayerCombat : MonoBehaviour
    {
        [SerializeField]
        private float attackRange = 2.5f;

        [SerializeField]
        private string enemyType = "Goblin";

        [SerializeField]
        private KeyCode attackKey = KeyCode.F;

        private void Update()
        {
            if (!QuestSamplePlayerLifecycle.IsActionAllowed)
            {
                return;
            }

            if (!Input.GetKeyDown(attackKey))
            {
                return;
            }

            var hits = Physics.OverlapSphere(transform.position, attackRange, ~0, QueryTriggerInteraction.Collide);
            QuestSampleEnemy nearest = null;
            var best = float.MaxValue;
            foreach (var c in hits)
            {
                var e = c.GetComponentInParent<QuestSampleEnemy>();
                if (e == null || !e.IsAlive)
                {
                    continue;
                }

                var d = (e.transform.position - transform.position).sqrMagnitude;
                if (d < best)
                {
                    best = d;
                    nearest = e;
                }
            }

            nearest?.ApplyHitFromPlayer(enemyType);
        }
    }
}
