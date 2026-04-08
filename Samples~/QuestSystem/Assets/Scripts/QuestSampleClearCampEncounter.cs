using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Resets goblin camp actors after <see cref="QuestSampleRuntimeSetup.ClearTheCampQuestId"/> fails or before a fresh start from the Quest Board.
    /// </summary>
    public static class QuestSampleClearCampEncounter
    {
        private static readonly string[] GoblinNames = { "Goblin_A", "Goblin_B", "Goblin_C" };

        private static readonly string[] CampPickupNames = { "Loot_Goblin", "Bonus_Trinket" };

        public static void Reset()
        {
            var reg = QuestSampleWorldRegistry.Instance;
            foreach (var name in GoblinNames)
            {
                reg?.Unconsume(name);
            }

            foreach (var name in CampPickupNames)
            {
                reg?.Unconsume(name);
            }

            var enemies = Object.FindObjectsByType<QuestSampleEnemy>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var e in enemies)
            {
                if (e == null)
                {
                    continue;
                }

                foreach (var t in GoblinNames)
                {
                    if (e.gameObject.name == t)
                    {
                        e.ResetForEncounter();
                        break;
                    }
                }
            }

            var pickups =
                Object.FindObjectsByType<QuestSampleItemPickup>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var p in pickups)
            {
                if (p == null)
                {
                    continue;
                }

                foreach (var t in CampPickupNames)
                {
                    if (p.gameObject.name == t)
                    {
                        p.ResetPickup();
                        break;
                    }
                }
            }
        }
    }
}
