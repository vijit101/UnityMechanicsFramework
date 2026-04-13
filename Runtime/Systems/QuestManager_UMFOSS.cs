using System;
using System.Collections.Generic;
using System.Linq;
using GameplayMechanicsUMFOSS.Core;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Data-driven quest manager. Subscribes only to <see cref="GameEventPayload"/> for objective matching.
    /// </summary>
    public class QuestManager_UMFOSS : MonoSingletongeneric<QuestManager_UMFOSS>, ISaveable_UMFOSS
    {
        public const string PlayerDeathEventType = "PlayerDeathEvent";

        public const string PlayerLevelChangedEventType = "PlayerLevelChangedEvent";

        [Header("Content")]
        [SerializeField]
        [Tooltip("All quests available in this build (used for save restore and lookups). Also loads Resources/Quests.")]
        private QuestData_UMFOSS[] knownQuests;

        [Header("Runtime")]
        [SerializeField]
        private int currentPlayerLevel = 1;

        private readonly Dictionary<string, QuestInstance_UMFOSS> _activeById = new Dictionary<string, QuestInstance_UMFOSS>();

        private readonly List<QuestInstance_UMFOSS> _activeQuests = new List<QuestInstance_UMFOSS>();

        private readonly List<QuestInstance_UMFOSS> _completedQuests = new List<QuestInstance_UMFOSS>();

        private readonly HashSet<string> _completedIds = new HashSet<string>();

        private readonly Dictionary<string, HashSet<QuestInstance_UMFOSS>> _objectiveListeners =
            new Dictionary<string, HashSet<QuestInstance_UMFOSS>>();

        private Dictionary<string, QuestData_UMFOSS> _questLookup;

        /// <summary>Current player level for <see cref="QuestData_UMFOSS.requiredLevel"/> checks.</summary>
        public int CurrentPlayerLevel => currentPlayerLevel;

        protected override void Awake()
        {
            base.Awake();
            BuildQuestLookup();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEventPayload>(OnGameEventPayload);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEventPayload>(OnGameEventPayload);
        }

        private void Start()
        {
            TryAutoStartQuests();
        }

        /// <summary>Sets player level for prerequisite checks (also updated via <see cref="PlayerLevelChangedEventType"/> payloads).</summary>
        public void SetCurrentPlayerLevel(int level)
        {
            currentPlayerLevel = Mathf.Max(0, level);
        }

        /// <summary>
        /// Samples/tests: register runtime-created or additional <see cref="QuestData_UMFOSS"/> assets.
        /// </summary>
        public void RegisterQuestData(QuestData_UMFOSS data)
        {
            if (data == null || string.IsNullOrEmpty(data.questID))
            {
                return;
            }

            if (_questLookup == null)
            {
                _questLookup = new Dictionary<string, QuestData_UMFOSS>();
            }

            _questLookup[data.questID] = data;
        }

        private void BuildQuestLookup()
        {
            _questLookup = new Dictionary<string, QuestData_UMFOSS>();
            if (knownQuests != null)
            {
                foreach (var q in knownQuests)
                {
                    if (q == null || string.IsNullOrEmpty(q.questID)) continue;
                    _questLookup[q.questID] = q;
                }
            }

            var loaded = Resources.LoadAll<QuestData_UMFOSS>("Quests");
            foreach (var q in loaded)
            {
                if (q == null || string.IsNullOrEmpty(q.questID)) continue;
                if (!_questLookup.ContainsKey(q.questID))
                {
                    _questLookup[q.questID] = q;
                }
            }
        }

        private void OnGameEventPayload(GameEventPayload payload)
        {
            if (string.IsNullOrEmpty(payload.EventType))
            {
                return;
            }

            if (payload.EventType == PlayerLevelChangedEventType && payload.Properties != null &&
                payload.Properties.TryGetValue("level", out var levelStr) &&
                int.TryParse(levelStr, out var lvl))
            {
                currentPlayerLevel = lvl;
            }

            if (payload.EventType == PlayerDeathEventType)
            {
                HandlePlayerDeath(payload);
            }

            if (!_objectiveListeners.TryGetValue(payload.EventType, out var quests))
            {
                return;
            }

            foreach (var quest in quests.ToArray())
            {
                if (quest.State != QuestState.Active)
                {
                    continue;
                }

                ProcessPayloadForQuest(quest, payload);
            }
        }

        private void HandlePlayerDeath(GameEventPayload payload)
        {
            var isPlayer = payload.Properties != null &&
                           payload.Properties.TryGetValue("isPlayer", out var v) &&
                           (v == "true" || v == "True" || v == "1");

            if (!isPlayer)
            {
                return;
            }

            foreach (var quest in _activeQuests.ToArray())
            {
                if (!quest.Data.failOnDeath)
                {
                    continue;
                }

                FailQuestInternal(quest, "Player died");
            }
        }

        private void ProcessPayloadForQuest(QuestInstance_UMFOSS quest, GameEventPayload payload)
        {
            foreach (var objective in quest.Objectives)
            {
                if (objective.Data.eventTypeKey != payload.EventType)
                {
                    continue;
                }

                if (objective.IsComplete())
                {
                    continue;
                }

                if (!MatchesFilter(objective, payload))
                {
                    continue;
                }

                if (objective.Data.isHidden && !objective.IsRevealed)
                {
                    objective.IsRevealed = true;
                    EventBus.Publish(new ObjectiveStartedEvent_UMFOSS
                    {
                        Quest = quest,
                        Objective = objective
                    });
                }

                objective.Increment();
                EventBus.Publish(new ObjectiveProgressEvent_UMFOSS
                {
                    Quest = quest,
                    Objective = objective,
                    NewCount = objective.CurrentCount
                });

                if (objective.IsComplete())
                {
                    EventBus.Publish(new ObjectiveCompletedEvent_UMFOSS
                    {
                        Quest = quest,
                        Objective = objective
                    });

                    if (quest.IsComplete())
                    {
                        CompleteQuestInternal(quest);
                    }
                }
            }
        }

        private static bool MatchesFilter(ObjectiveInstance_UMFOSS objective, GameEventPayload payload)
        {
            var data = objective.Data;
            var props = payload.Properties ?? new Dictionary<string, string>();

            if (string.IsNullOrEmpty(data.filterKey))
            {
                return true;
            }

            if (!props.TryGetValue(data.filterKey, out var val))
            {
                return false;
            }

            if (string.IsNullOrEmpty(data.filterValue))
            {
                return true;
            }

            return val == data.filterValue;
        }

        /// <summary>Starts a quest if prerequisites are met.</summary>
        public bool StartQuest(QuestData_UMFOSS quest)
        {
            if (quest == null)
            {
                PublishStartFailed(null, "Null quest");
                return false;
            }

            if (_activeById.ContainsKey(quest.questID))
            {
                PublishStartFailed(quest, "Already active");
                return false;
            }

            if (_completedIds.Contains(quest.questID) && !quest.isRepeatable)
            {
                PublishStartFailed(quest, "Already completed and not repeatable");
                return false;
            }

            if (quest.requiredLevel > 0 && currentPlayerLevel < quest.requiredLevel)
            {
                PublishStartFailed(quest, "Level too low");
                return false;
            }

            if (quest.requiredQuests != null)
            {
                foreach (var req in quest.requiredQuests)
                {
                    if (req == null) continue;
                    if (!_completedIds.Contains(req.questID))
                    {
                        PublishStartFailed(quest, "Prerequisites not met");
                        return false;
                    }
                }
            }

            StartQuestInternal(quest);
            return true;
        }

        private void PublishStartFailed(QuestData_UMFOSS quest, string reason)
        {
            EventBus.Publish(new QuestStartFailedEvent_UMFOSS
            {
                Quest = quest,
                Reason = reason
            });
        }

        private void StartQuestInternal(QuestData_UMFOSS data)
        {
            var objectives = new List<ObjectiveInstance_UMFOSS>();
            if (data.objectives != null)
            {
                foreach (var od in data.objectives)
                {
                    if (od == null) continue;
                    objectives.Add(new ObjectiveInstance_UMFOSS(od));
                }
            }

            var instance = new QuestInstance_UMFOSS(data, objectives)
            {
                State = QuestState.Active,
                StartTime = Time.realtimeSinceStartup
            };

            _activeById[data.questID] = instance;
            _activeQuests.Add(instance);

            RegisterObjectiveListeners(instance);

            foreach (var obj in instance.Objectives)
            {
                if (!obj.Data.isHidden)
                {
                    obj.IsRevealed = true;
                    EventBus.Publish(new ObjectiveStartedEvent_UMFOSS
                    {
                        Quest = instance,
                        Objective = obj
                    });
                }
            }

            EventBus.Publish(new QuestStartedEvent_UMFOSS { Quest = instance });
        }

        private void RegisterObjectiveListeners(QuestInstance_UMFOSS quest)
        {
            foreach (var obj in quest.Objectives)
            {
                var key = obj.Data.eventTypeKey;
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (!_objectiveListeners.TryGetValue(key, out var set))
                {
                    set = new HashSet<QuestInstance_UMFOSS>();
                    _objectiveListeners[key] = set;
                }

                set.Add(quest);
            }
        }

        private void UnregisterObjectiveListeners(QuestInstance_UMFOSS quest)
        {
            foreach (var kv in _objectiveListeners.ToList())
            {
                kv.Value.Remove(quest);
                if (kv.Value.Count == 0)
                {
                    _objectiveListeners.Remove(kv.Key);
                }
            }
        }

        public bool AbandonQuest(QuestData_UMFOSS quest)
        {
            if (quest == null || !_activeById.TryGetValue(quest.questID, out var instance))
            {
                return false;
            }

            UnregisterObjectiveListeners(instance);
            instance.State = QuestState.Abandoned;
            _activeById.Remove(quest.questID);
            _activeQuests.Remove(instance);
            EventBus.Publish(new QuestAbandonedEvent_UMFOSS { Quest = instance });
            return true;
        }

        public bool FailQuest(QuestData_UMFOSS quest)
        {
            if (quest == null || !_activeById.TryGetValue(quest.questID, out var instance))
            {
                return false;
            }

            return FailQuestInternal(instance, "Manual fail");
        }

        private bool FailQuestInternal(QuestInstance_UMFOSS instance, string reason)
        {
            UnregisterObjectiveListeners(instance);
            instance.State = QuestState.Failed;
            _activeById.Remove(instance.Data.questID);
            _activeQuests.Remove(instance);
            EventBus.Publish(new QuestFailedEvent_UMFOSS { Quest = instance, Reason = reason });
            return true;
        }

        private void CompleteQuestInternal(QuestInstance_UMFOSS quest)
        {
            UnregisterObjectiveListeners(quest);
            quest.State = QuestState.Completed;
            quest.CompletionTime = Time.realtimeSinceStartup;
            _activeById.Remove(quest.Data.questID);
            _activeQuests.Remove(quest);

            if (!quest.Data.isRepeatable)
            {
                _completedIds.Add(quest.Data.questID);
                _completedQuests.Add(quest);
            }

            EventBus.Publish(new QuestCompletedEvent_UMFOSS { Quest = quest });

            var items = quest.Data.itemRewards != null
                ? quest.Data.itemRewards.Where(i => i != null).ToArray()
                : Array.Empty<ItemData_UMFOSS>();

            EventBus.Publish(new QuestRewardGrantedEvent_UMFOSS
            {
                Quest = quest,
                Experience = quest.Data.experienceReward,
                Currency = quest.Data.currencyReward,
                Items = items
            });

            if (quest.Data.unlockedQuests != null)
            {
                foreach (var u in quest.Data.unlockedQuests)
                {
                    if (u == null) continue;
                    EventBus.Publish(new QuestUnlockedEvent_UMFOSS { Quest = u });
                }
            }

            TryAutoStartQuests();
        }

        private void TryAutoStartQuests()
        {
            if (_questLookup == null)
            {
                BuildQuestLookup();
            }

            foreach (var q in _questLookup.Values)
            {
                if (q == null || !q.autoStart)
                {
                    continue;
                }

                if (_activeById.ContainsKey(q.questID))
                {
                    continue;
                }

                if (_completedIds.Contains(q.questID) && !q.isRepeatable)
                {
                    continue;
                }

                if (!CanStartQuest(q))
                {
                    continue;
                }

                StartQuestInternal(q);
            }
        }

        public bool IsQuestActive(QuestData_UMFOSS quest)
        {
            return quest != null && _activeById.ContainsKey(quest.questID);
        }

        public bool IsQuestComplete(QuestData_UMFOSS quest)
        {
            return quest != null && _completedIds.Contains(quest.questID);
        }

        public bool CanStartQuest(QuestData_UMFOSS quest)
        {
            if (quest == null) return false;
            if (_activeById.ContainsKey(quest.questID)) return false;
            if (_completedIds.Contains(quest.questID) && !quest.isRepeatable) return false;
            if (quest.requiredLevel > 0 && currentPlayerLevel < quest.requiredLevel) return false;
            if (quest.requiredQuests != null)
            {
                foreach (var req in quest.requiredQuests)
                {
                    if (req == null) continue;
                    if (!_completedIds.Contains(req.questID)) return false;
                }
            }

            return true;
        }

        public QuestInstance_UMFOSS GetQuestInstance(QuestData_UMFOSS quest)
        {
            if (quest == null) return null;
            return _activeById.TryGetValue(quest.questID, out var q) ? q : null;
        }

        public List<QuestInstance_UMFOSS> GetAllActiveQuests()
        {
            return new List<QuestInstance_UMFOSS>(_activeQuests);
        }

        public List<QuestInstance_UMFOSS> GetQuestsByCategory(QuestCategory category)
        {
            return _activeQuests.Where(q => q.Data.category == category).ToList();
        }

        public float GetQuestProgress(QuestData_UMFOSS quest)
        {
            var inst = GetQuestInstance(quest);
            return inst == null ? 0f : inst.GetProgress();
        }

        public int GetObjectiveCount(QuestData_UMFOSS quest)
        {
            var inst = GetQuestInstance(quest);
            if (inst == null) return 0;
            return inst.Objectives.Count(o => !o.Data.isOptional);
        }

        public int GetCompletedObjectiveCount(QuestData_UMFOSS quest)
        {
            var inst = GetQuestInstance(quest);
            if (inst == null) return 0;
            return inst.Objectives.Count(o => !o.Data.isOptional && o.IsComplete());
        }

        public object CaptureState()
        {
            var completed = new string[_completedIds.Count];
            var i = 0;
            foreach (var id in _completedIds)
            {
                completed[i++] = id;
            }

            var active = new QuestSaveEntry_UMFOSS[_activeQuests.Count];
            for (var qi = 0; qi < _activeQuests.Count; qi++)
            {
                var q = _activeQuests[qi];
                var objs = new ObjectiveSaveEntry_UMFOSS[q.Objectives.Count];
                for (var oi = 0; oi < q.Objectives.Count; oi++)
                {
                    var o = q.Objectives[oi];
                    objs[oi] = new ObjectiveSaveEntry_UMFOSS
                    {
                        objectiveID = o.Data.objectiveID,
                        currentCount = o.CurrentCount,
                        isRevealed = o.IsRevealed
                    };
                }

                active[qi] = new QuestSaveEntry_UMFOSS
                {
                    questID = q.Data.questID,
                    state = q.State,
                    objectives = objs
                };
            }

            return new QuestSaveData_UMFOSS
            {
                activeQuests = active,
                completedQuestIDs = completed
            };
        }

        public void RestoreState(object state)
        {
            if (state is QuestSaveData_UMFOSS data)
            {
                RestoreFromSave(data);
            }
        }

        private void RestoreFromSave(QuestSaveData_UMFOSS data)
        {
            foreach (var q in _activeQuests.ToArray())
            {
                UnregisterObjectiveListeners(q);
            }

            _activeQuests.Clear();
            _activeById.Clear();
            _completedIds.Clear();
            _completedQuests.Clear();

            if (data.completedQuestIDs != null)
            {
                foreach (var id in data.completedQuestIDs)
                {
                    _completedIds.Add(id);
                }
            }

            BuildQuestLookup();

            if (data.activeQuests == null)
            {
                return;
            }

            foreach (var entry in data.activeQuests)
            {
                if (!_questLookup.TryGetValue(entry.questID, out var questData))
                {
                    Debug.LogWarning($"Quest restore: missing QuestData for id {entry.questID}");
                    continue;
                }

                var objectives = new List<ObjectiveInstance_UMFOSS>();
                if (questData.objectives != null)
                {
                    foreach (var od in questData.objectives)
                    {
                        if (od == null) continue;
                        var oi = new ObjectiveInstance_UMFOSS(od);
                        ObjectiveSaveEntry_UMFOSS saved = null;
                        if (entry.objectives != null)
                        {
                            saved = entry.objectives.FirstOrDefault(s => s.objectiveID == od.objectiveID);
                        }

                        if (saved != null)
                        {
                            oi.SetCount(saved.currentCount);
                            oi.IsRevealed = saved.isRevealed;
                        }

                        objectives.Add(oi);
                    }
                }

                var instance = new QuestInstance_UMFOSS(questData, objectives)
                {
                    State = entry.state,
                    StartTime = Time.realtimeSinceStartup
                };

                _activeById[questData.questID] = instance;
                _activeQuests.Add(instance);
                RegisterObjectiveListeners(instance);
            }
        }
    }
}
