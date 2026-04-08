using System.Collections.Generic;
using System.Text;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Quest tracker, <see cref="GameEventPayload"/> log, reward feed, controls, and death overlay.
    /// </summary>
    public sealed class QuestSampleHud : MonoBehaviour
    {
        [SerializeField]
        private Text questTrackerText;

        [SerializeField]
        private Text eventLogText;

        [SerializeField]
        private Text controlsText;

        [SerializeField]
        private Text rewardsText;

        [SerializeField]
        private GameObject deathOverlayRoot;

        [SerializeField]
        private Text deathTitleText;

        [SerializeField]
        private Text deathCountdownText;

        [SerializeField]
        private Button deathRespawnButton;

        [SerializeField]
        private Text retryHintText;

        [SerializeField]
        private int maxLogLines = 14;

        [SerializeField]
        private int maxRewardLines = 6;

        private QuestSamplePlayerLifecycle _lifecycle;

        private readonly List<string> _lines = new List<string>();

        private readonly List<string> _rewardLines = new List<string>();

        private bool _showClearCampRetryHint;

        /// <summary>Called from <see cref="QuestSampleSceneBootstrap"/> when building the demo at runtime.</summary>
        public void Configure(Text tracker, Text log, Text controls, Text rewards)
        {
            questTrackerText = tracker;
            eventLogText = log;
            controlsText = controls;
            rewardsText = rewards;
        }

        public void ConfigureRetryHint(Text hint)
        {
            retryHintText = hint;
            ApplyRetryHintVisibility();
        }

        public void ConfigureDeath(GameObject overlayRoot, Text title, Text countdown, Button respawnButton,
            QuestSamplePlayerLifecycle lifecycle)
        {
            if (_lifecycle != null)
            {
                _lifecycle.Died -= OnLifecycleDied;
                _lifecycle.Respawned -= OnLifecycleRespawned;
            }

            deathOverlayRoot = overlayRoot;
            deathTitleText = title;
            deathCountdownText = countdown;
            deathRespawnButton = respawnButton;
            _lifecycle = lifecycle;
            if (deathRespawnButton != null)
            {
                deathRespawnButton.onClick.RemoveListener(OnRespawnClicked);
                deathRespawnButton.onClick.AddListener(OnRespawnClicked);
            }

            if (deathOverlayRoot != null)
            {
                deathOverlayRoot.SetActive(false);
            }

            if (_lifecycle != null)
            {
                _lifecycle.Died += OnLifecycleDied;
                _lifecycle.Respawned += OnLifecycleRespawned;
            }
        }

        private void OnLifecycleDied()
        {
            ShowDeathOverlay();
        }

        private void OnLifecycleRespawned()
        {
            HideDeathOverlay();
        }

        private void OnDestroy()
        {
            if (_lifecycle != null)
            {
                _lifecycle.Died -= OnLifecycleDied;
                _lifecycle.Respawned -= OnLifecycleRespawned;
            }
        }

        private void OnRespawnClicked()
        {
            _lifecycle?.RequestImmediateRespawn();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GameEventPayload>(OnPayload);
            EventBus.Subscribe<QuestStartedEvent_UMFOSS>(OnQuestStarted);
            EventBus.Subscribe<QuestCompletedEvent_UMFOSS>(OnQuestCompleted);
            EventBus.Subscribe<ObjectiveProgressEvent_UMFOSS>(OnObjectiveProgress);
            EventBus.Subscribe<ObjectiveCompletedEvent_UMFOSS>(OnObjectiveCompletedEv);
            EventBus.Subscribe<QuestFailedEvent_UMFOSS>(OnQuestFailed);
            EventBus.Subscribe<QuestAbandonedEvent_UMFOSS>(OnQuestAbandoned);
            EventBus.Subscribe<QuestRewardGrantedEvent_UMFOSS>(OnQuestRewardGranted);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GameEventPayload>(OnPayload);
            EventBus.Unsubscribe<QuestStartedEvent_UMFOSS>(OnQuestStarted);
            EventBus.Unsubscribe<QuestCompletedEvent_UMFOSS>(OnQuestCompleted);
            EventBus.Unsubscribe<ObjectiveProgressEvent_UMFOSS>(OnObjectiveProgress);
            EventBus.Unsubscribe<ObjectiveCompletedEvent_UMFOSS>(OnObjectiveCompletedEv);
            EventBus.Unsubscribe<QuestFailedEvent_UMFOSS>(OnQuestFailed);
            EventBus.Unsubscribe<QuestAbandonedEvent_UMFOSS>(OnQuestAbandoned);
            EventBus.Unsubscribe<QuestRewardGrantedEvent_UMFOSS>(OnQuestRewardGranted);
        }

        private void Start()
        {
            if (controlsText != null)
            {
                controlsText.text =
                    "WASD move | F attack | E at blue Quest Board: Clear The Camp | E at Merchant: merchant quest | Loot & zones | P death | S save | L load";
            }

            ApplyRetryHintVisibility();
            RefreshTracker();
        }

        private void Update()
        {
            if (_lifecycle != null && !_lifecycle.IsAlive && deathOverlayRoot != null && deathOverlayRoot.activeSelf &&
                deathCountdownText != null)
            {
                deathCountdownText.text =
                    "Respawning in " + _lifecycle.RespawnTimeRemaining.ToString("F1") +
                    " s (or click Respawn)";
            }
        }

        private void OnQuestStarted(QuestStartedEvent_UMFOSS e)
        {
            if (e?.Quest?.Data != null &&
                e.Quest.Data.questID == QuestSampleRuntimeSetup.ClearTheCampQuestId)
            {
                _showClearCampRetryHint = false;
                ApplyRetryHintVisibility();
            }

            RefreshTracker();
        }

        private void OnQuestCompleted(QuestCompletedEvent_UMFOSS _) => RefreshTracker();

        private void OnObjectiveProgress(ObjectiveProgressEvent_UMFOSS _) => RefreshTracker();

        private void OnObjectiveCompletedEv(ObjectiveCompletedEvent_UMFOSS _) => RefreshTracker();

        private void OnQuestFailed(QuestFailedEvent_UMFOSS e)
        {
            if (e?.Quest?.Data != null &&
                e.Quest.Data.questID == QuestSampleRuntimeSetup.ClearTheCampQuestId)
            {
                _showClearCampRetryHint = true;
                ApplyRetryHintVisibility();
            }

            RefreshTracker();
        }

        private void OnQuestAbandoned(QuestAbandonedEvent_UMFOSS _) => RefreshTracker();

        private void OnQuestRewardGranted(QuestRewardGrantedEvent_UMFOSS e)
        {
            if (e?.Quest == null)
            {
                return;
            }

            var title = e.Quest.Data.title;
            var sb = new StringBuilder();
            sb.Append(title).Append(": ");
            if (e.Experience != 0)
            {
                sb.Append("+").Append(e.Experience).Append(" XP ");
            }

            if (e.Currency != 0)
            {
                sb.Append("+").Append(e.Currency).Append(" gold ");
            }

            if (e.Items != null)
            {
                foreach (var it in e.Items)
                {
                    if (it == null)
                    {
                        continue;
                    }

                    sb.Append(it.displayName).Append(" ");
                }
            }

            _rewardLines.Insert(0, sb.ToString().TrimEnd());
            while (_rewardLines.Count > maxRewardLines)
            {
                _rewardLines.RemoveAt(_rewardLines.Count - 1);
            }

            if (rewardsText != null)
            {
                var b = new StringBuilder();
                foreach (var line in _rewardLines)
                {
                    b.AppendLine(line);
                }

                rewardsText.text = b.ToString();
            }
        }

        private void OnPayload(GameEventPayload p)
        {
            if (eventLogText == null)
            {
                return;
            }

            var line = p.EventType + " | ";
            if (p.Properties != null)
            {
                foreach (var kv in p.Properties)
                {
                    line += kv.Key + "=" + kv.Value + " ";
                }
            }

            _lines.Insert(0, line.TrimEnd());
            while (_lines.Count > maxLogLines)
            {
                _lines.RemoveAt(_lines.Count - 1);
            }

            var sb = new StringBuilder();
            foreach (var l in _lines)
            {
                sb.AppendLine(l);
            }

            eventLogText.text = sb.ToString();
        }

        private void RefreshTracker()
        {
            if (questTrackerText == null)
            {
                return;
            }

            var m = QuestManager_UMFOSS.Instance;
            if (m == null)
            {
                questTrackerText.text = "No QuestManager";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("AVAILABLE QUESTS");
            AppendAvailableQuestLine(sb, m, QuestSampleRuntimeSetup.ClearTheCampQuest,
                "Clear The Camp (press E at Quest Board)");
            AppendAvailableQuestLine(sb, m, QuestSampleRuntimeSetup.MerchantsRequestQuest,
                "The Merchant's Request (press E at Merchant)");
            sb.AppendLine();

            sb.AppendLine("ACTIVE");
            var active = m.GetAllActiveQuests();
            if (active == null || active.Count == 0)
            {
                sb.AppendLine("(none)");
            }
            else
            {
                foreach (var q in active)
                {
                    sb.Append(q.Data.title).Append(" (").Append(q.Data.category).AppendLine(")");
                    foreach (var o in q.Objectives)
                    {
                        if (o.Data.isHidden && !o.IsRevealed)
                        {
                            continue;
                        }

                        var prog = o.CurrentCount + "/" + o.Data.requiredCount;
                        var mark = o.IsComplete() ? "[x] " : "[ ] ";
                        sb.Append("  ").Append(mark).Append(o.Data.displayText).Append(" ").AppendLine(prog);
                    }

                    sb.AppendLine();
                }
            }

            questTrackerText.text = sb.ToString();
        }

        private static void AppendAvailableQuestLine(StringBuilder sb, QuestManager_UMFOSS manager,
            QuestData_UMFOSS quest, string startInstruction)
        {
            if (quest == null)
            {
                sb.AppendLine("  (unregistered)");
                return;
            }

            if (manager.IsQuestActive(quest))
            {
                sb.Append("  — ").Append(quest.title).AppendLine(" (in progress — see Active)");
                return;
            }

            if (manager.IsQuestComplete(quest))
            {
                sb.Append("  • ").Append(quest.title).AppendLine(" (completed)");
                return;
            }

            if (manager.CanStartQuest(quest))
            {
                sb.Append("  • ").Append(startInstruction).AppendLine();
                return;
            }

            sb.Append("  • ").Append(quest.title).AppendLine(" (locked)");
        }

        private void ApplyRetryHintVisibility()
        {
            if (retryHintText == null)
            {
                return;
            }

            retryHintText.gameObject.SetActive(_showClearCampRetryHint);
            if (_showClearCampRetryHint)
            {
                retryHintText.text =
                    "Clear The Camp failed. Go to the Quest Board (blue pillar) and press E to retry.";
            }
        }

        public void ShowDeathOverlay()
        {
            if (deathOverlayRoot != null)
            {
                deathOverlayRoot.SetActive(true);
            }

            if (deathTitleText != null)
            {
                deathTitleText.text = "You died";
            }
        }

        public void HideDeathOverlay()
        {
            if (deathOverlayRoot != null)
            {
                deathOverlayRoot.SetActive(false);
            }
        }
    }
}
