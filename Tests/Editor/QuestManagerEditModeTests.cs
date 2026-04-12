using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Systems;
using NUnit.Framework;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Tests.Editor
{
    public class QuestManagerEditModeTests
    {
        [SetUp]
        public void SetUp()
        {
            EventBus.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            EventBus.ClearAll();
        }

        [Test]
        public void Filter_EmptyFilterKey_MatchesAnyPropertyPayload()
        {
            var go = new GameObject("QM_Test");
            var manager = go.AddComponent<QuestManager_UMFOSS>();
            var obj = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            obj.objectiveID = "o1";
            obj.eventTypeKey = "TestEvent";
            obj.filterKey = "";
            obj.filterValue = "";
            obj.requiredCount = 1;

            var quest = ScriptableObject.CreateInstance<QuestData_UMFOSS>();
            quest.questID = "Q1";
            quest.objectives = new[] { obj };

            manager.RegisterQuestData(quest);
            Assert.IsTrue(manager.StartQuest(quest));

            EventBus.Publish(new GameEventPayload("TestEvent", new Dictionary<string, string> { { "any", "x" } }));
            Assert.IsTrue(manager.IsQuestComplete(quest));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Filter_EmptyFilterValue_MatchesAnyValueForKey()
        {
            var go = new GameObject("QM_Test2");
            var manager = go.AddComponent<QuestManager_UMFOSS>();
            var obj = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            obj.objectiveID = "o1";
            obj.eventTypeKey = "Evt";
            obj.filterKey = "k";
            obj.filterValue = "";
            obj.requiredCount = 1;

            var quest = ScriptableObject.CreateInstance<QuestData_UMFOSS>();
            quest.questID = "Q2";
            quest.objectives = new[] { obj };

            manager.RegisterQuestData(quest);
            manager.StartQuest(quest);

            EventBus.Publish(new GameEventPayload("Evt", new Dictionary<string, string> { { "k", "anything" } }));
            Assert.IsTrue(manager.IsQuestComplete(quest));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void OptionalObjective_DoesNotBlockCompletion()
        {
            var go = new GameObject("QM_Test3");
            var manager = go.AddComponent<QuestManager_UMFOSS>();

            var main = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            main.objectiveID = "m";
            main.eventTypeKey = "A";
            main.requiredCount = 1;

            var opt = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            opt.objectiveID = "opt";
            opt.eventTypeKey = "B";
            opt.requiredCount = 1;
            opt.isOptional = true;

            var quest = ScriptableObject.CreateInstance<QuestData_UMFOSS>();
            quest.questID = "QOpt";
            quest.objectives = new[] { main, opt };

            manager.RegisterQuestData(quest);
            manager.StartQuest(quest);

            EventBus.Publish(new GameEventPayload("A", new Dictionary<string, string>()));
            Assert.IsTrue(manager.IsQuestComplete(quest));
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Save_RoundTrip_RestoresCounts()
        {
            var go = new GameObject("QM_Test4");
            var manager = go.AddComponent<QuestManager_UMFOSS>();

            var o1 = ScriptableObject.CreateInstance<ObjectiveData_UMFOSS>();
            o1.objectiveID = "x";
            o1.eventTypeKey = "K";
            o1.requiredCount = 3;

            var quest = ScriptableObject.CreateInstance<QuestData_UMFOSS>();
            quest.questID = "SaveQ";
            quest.objectives = new[] { o1 };

            manager.RegisterQuestData(quest);
            manager.StartQuest(quest);

            EventBus.Publish(new GameEventPayload("K", new Dictionary<string, string>()));

            var state = manager.CaptureState();
            Object.DestroyImmediate(go);

            var go2 = new GameObject("QM_Test5");
            var manager2 = go2.AddComponent<QuestManager_UMFOSS>();
            manager2.RegisterQuestData(quest);
            manager2.RestoreState(state);

            var inst = manager2.GetQuestInstance(quest);
            Assert.IsNotNull(inst);
            Assert.AreEqual(1, inst.Objectives[0].CurrentCount);

            Object.DestroyImmediate(go2);
        }
    }
}
