using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Builds demo quest ScriptableObjects at runtime and registers them with <see cref="QuestManager_UMFOSS"/>.
    /// Execution order ensures registration runs before <see cref="QuestManager_UMFOSS"/> Start (auto-start quests).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public sealed class QuestSampleRuntimeSetup : MonoBehaviour
    {
        public const string ClearTheCampQuestId = "ClearTheCamp";

        public const string MerchantsRequestQuestId = "MerchantsRequest";

        public const string EventEnemyDied = "EnemyDiedEvent";

        public const string EventItemAdded = "ItemAddedEvent";

        public const string EventInteract = "InteractEvent";

        public const string EventZoneEntered = "ZoneEnteredEvent";

        /// <summary>Set after <see cref="Start"/> runs; used by quest board and retry UI.</summary>
        public static QuestData_UMFOSS ClearTheCampQuest { get; private set; }

        public static QuestData_UMFOSS MerchantsRequestQuest { get; private set; }

        public static QuestData_UMFOSS ExplorerQuest { get; private set; }

        private void Start()
        {
            var manager = QuestManager_UMFOSS.Instance;
            if (manager == null)
            {
                Debug.LogError("QuestSampleRuntimeSetup: QuestManager_UMFOSS missing in scene.");
                return;
            }

            var ironSword = ScriptableObject.CreateInstance<ItemData_UMFOSS>();
            ironSword.name = "IronSword";
            ironSword.itemID = "IronSword";
            ironSword.displayName = "Iron Sword";

            // --- Objectives: Clear The Camp ---
            var killGoblins = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            killGoblins.name = "Obj_KillGoblins";
            killGoblins.objectiveID = "KillGoblins";
            killGoblins.displayText = "Kill 3 Goblins";
            killGoblins.type = ObjectiveType.KillEnemy;
            killGoblins.requiredCount = 3;
            killGoblins.eventTypeKey = EventEnemyDied;
            killGoblins.filterKey = "enemyType";
            killGoblins.filterValue = "Goblin";

            var collectLoot = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            collectLoot.name = "Obj_GoblinLoot";
            collectLoot.objectiveID = "GoblinLoot";
            collectLoot.displayText = "Collect dropped loot";
            collectLoot.type = ObjectiveType.CollectItem;
            collectLoot.requiredCount = 1;
            collectLoot.eventTypeKey = EventItemAdded;
            collectLoot.filterKey = "itemName";
            collectLoot.filterValue = "GoblinLoot";

            var optionalTrinket = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            optionalTrinket.name = "Obj_BonusTrinket";
            optionalTrinket.objectiveID = "BonusTrinket";
            optionalTrinket.displayText = "Bonus: find a trinket (optional)";
            optionalTrinket.type = ObjectiveType.CollectItem;
            optionalTrinket.requiredCount = 1;
            optionalTrinket.eventTypeKey = EventItemAdded;
            optionalTrinket.filterKey = "itemName";
            optionalTrinket.filterValue = "BonusTrinket";
            optionalTrinket.isOptional = true;

            var q1 = ScriptableObject.CreateInstance<QuestData_UMFOSS>();
            q1.name = "ClearTheCamp";
            q1.questID = ClearTheCampQuestId;
            q1.title = "Clear The Camp";
            q1.description = "Eliminate goblins and collect loot.";
            q1.category = QuestCategory.Main;
            q1.objectives = new[] { killGoblins, collectLoot, optionalTrinket };
            q1.failOnDeath = true;
            q1.experienceReward = 50;
            q1.currencyReward = 10;

            // --- Merchant ---
            var talkMerchant = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            talkMerchant.name = "Obj_TalkMerchant";
            talkMerchant.objectiveID = "TalkMerchant";
            talkMerchant.displayText = "Talk to the Merchant";
            talkMerchant.type = ObjectiveType.InteractWith;
            talkMerchant.requiredCount = 1;
            talkMerchant.eventTypeKey = EventInteract;
            talkMerchant.filterKey = "merchantPhase";
            talkMerchant.filterValue = "Talk";

            var ironOre = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            ironOre.name = "Obj_IronOre";
            ironOre.objectiveID = "IronOre";
            ironOre.displayText = "Collect 2 Iron Ore";
            ironOre.type = ObjectiveType.CollectItem;
            ironOre.requiredCount = 2;
            ironOre.eventTypeKey = EventItemAdded;
            ironOre.filterKey = "itemName";
            ironOre.filterValue = "IronOre";

            var returnMerchant = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            returnMerchant.name = "Obj_ReturnMerchant";
            returnMerchant.objectiveID = "ReturnMerchant";
            returnMerchant.displayText = "Return to the Merchant";
            returnMerchant.type = ObjectiveType.InteractWith;
            returnMerchant.requiredCount = 1;
            returnMerchant.eventTypeKey = EventInteract;
            returnMerchant.filterKey = "merchantPhase";
            returnMerchant.filterValue = "Return";

            var q2 = ScriptableObject.CreateInstance<QuestData_UMFOSS>();
            q2.name = "MerchantsRequest";
            q2.questID = MerchantsRequestQuestId;
            q2.title = "The Merchant's Request";
            q2.description = "Speak, gather ore, return.";
            q2.category = QuestCategory.Side;
            q2.objectives = new[] { talkMerchant, ironOre, returnMerchant };
            q2.itemRewards = new[] { ironSword };

            // --- Explorer (hidden + auto-start) ---
            var east = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            east.name = "Obj_EastGate";
            east.objectiveID = "EastGate";
            east.displayText = "Reach the East Gate";
            east.type = ObjectiveType.ReachLocation;
            east.requiredCount = 1;
            east.eventTypeKey = EventZoneEntered;
            east.filterKey = "zoneID";
            east.filterValue = "EastGate";
            east.isHidden = true;

            var north = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            north.name = "Obj_NorthTower";
            north.objectiveID = "NorthTower";
            north.displayText = "Reach the North Tower";
            north.type = ObjectiveType.ReachLocation;
            north.requiredCount = 1;
            north.eventTypeKey = EventZoneEntered;
            north.filterKey = "zoneID";
            north.filterValue = "NorthTower";
            north.isHidden = true;

            var q3 = ScriptableObject.CreateInstance<QuestData_UMFOSS>();
            q3.name = "Explorer";
            q3.questID = "Explorer";
            q3.title = "Explorer";
            q3.description = "Hidden exploration bonus.";
            q3.category = QuestCategory.Hidden;
            q3.objectives = new[] { east, north };
            q3.experienceReward = 100;
            q3.autoStart = false;

            foreach (var q in new QuestData_UMFOSS[] { q1, q2, q3 })
            {
                manager.RegisterQuestData(q);
            }

            ClearTheCampQuest = q1;
            MerchantsRequestQuest = q2;
            ExplorerQuest = q3;

            // Quests start only from in-world interactions (Quest Board, Merchant).
        }
    }
}
