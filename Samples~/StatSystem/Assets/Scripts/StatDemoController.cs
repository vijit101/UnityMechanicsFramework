using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.StatSystem
{
    // =========================================================================
    //  StatDemoController
    //  Drives the StatSystem demo scene.
    //
    //  Attach this MonoBehaviour to a single "DemoController" GameObject.
    //  Wire up the Inspector references (see the Setup Guide) and press Play.
    //
    //  What this shows:
    //    • Live FinalValue display updating as modifiers are applied/removed
    //    • Correct flat-before-percentage stacking order
    //    • PercentMultiply as a separate multiplicative layer
    //    • Source-based removal — one call strips everything a "source" added
    //    • Self-expiring timed modifier with visible countdown
    //    • ResetToBase restoring exact original values
    // =========================================================================

    public class StatDemoController : MonoBehaviour
    {
        // ─── Inspector References — Stats ─────────────────────────────────────

        [Header("Stat Sheet")]
        [SerializeField] private StatSheet_UMFOSS playerStatSheet;

        // ─── Inspector References — UI Labels ─────────────────────────────────

        [Header("Damage Display")]
        [SerializeField] private TMP_Text labelBaseDamage;
        [SerializeField] private TMP_Text labelFinalDamage;
        [SerializeField] private TMP_Text labelActiveModifiers;

        [Header("Speed Display")]
        [SerializeField] private TMP_Text labelBaseMoveSpeed;
        [SerializeField] private TMP_Text labelFinalMoveSpeed;
        [SerializeField] private TMP_Text labelSpeedDebuffTimer;

        [Header("Defence Display")]
        [SerializeField] private TMP_Text labelBaseDefence;
        [SerializeField] private TMP_Text labelFinalDefence;

        // ─── Inspector References — Buttons ──────────────────────────────────

        [Header("Buttons")]
        [SerializeField] private Button btnAddFlatDamage;
        [SerializeField] private Button btnAddPercentDamage;
        [SerializeField] private Button btnAddMultiplyDamage;
        [SerializeField] private Button btnRemoveSword;
        [SerializeField] private Button btnSpeedDebuff;
        [SerializeField] private Button btnResetAll;

        // ─── Private State ────────────────────────────────────────────────────

        // "sword" is the source object for flat damage modifier
        private readonly object _swordSource   = new object();
        // "potion" is the source for percent-add damage modifier
        private readonly object _potionSource  = new object();
        // "relic" is the source for percent-multiply damage modifier
        private readonly object _relicSource   = new object();

        private const float SWORD_FLAT_BONUS        = 15f;
        private const float POTION_PERCENT_ADD      = 0.20f;   // +20%
        private const float RELIC_PERCENT_MULTIPLY  = 1.5f;    // ×1.5
        private const float SPEED_DEBUFF_PERCENT    = -0.30f;  // -30%
        private const float SPEED_DEBUFF_DURATION   = 5f;      // seconds

        private bool  _swordEquipped;
        private bool  _potionActive;
        private bool  _relicActive;

        // Tracks whether a timed speed debuff is running
        private float _speedDebuffEndTime = -1f;

        // Active modifiers stringbuilder cache
        private readonly System.Text.StringBuilder _sb = new System.Text.StringBuilder();

        // ─── Unity Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            // Subscribe to stat change events for debug logging
            StatEventBus.Subscribe<StatChangedEvent>(OnStatChanged);
            StatEventBus.Subscribe<StatModifierExpiredEvent>(OnModifierExpired);
        }

        private void OnDestroy()
        {
            StatEventBus.Unsubscribe<StatChangedEvent>(OnStatChanged);
            StatEventBus.Unsubscribe<StatModifierExpiredEvent>(OnModifierExpired);
        }

        private void Start()
        {
            // Wire buttons
            btnAddFlatDamage.onClick.AddListener(OnAddFlatDamage);
            btnAddPercentDamage.onClick.AddListener(OnAddPercentDamage);
            btnAddMultiplyDamage.onClick.AddListener(OnAddMultiplyDamage);
            btnRemoveSword.onClick.AddListener(OnRemoveSword);
            btnSpeedDebuff.onClick.AddListener(OnApplySpeedDebuff);
            btnResetAll.onClick.AddListener(OnResetAll);

            RefreshUI();
        }

        private void Update()
        {
            RefreshUI();
            UpdateDebuffTimer();
        }

        // ─── Button Handlers ──────────────────────────────────────────────────

        /// <summary>Equip sword → flat +15 damage (if not already equipped).</summary>
        private void OnAddFlatDamage()
        {
            if (_swordEquipped)
            {
                Debug.Log("[Demo] Sword already equipped.");
                return;
            }
            var mod = new StatModifier_UMFOSS(SWORD_FLAT_BONUS, ModifierType_UMFOSS.Flat, order: 0, source: _swordSource);
            playerStatSheet.AddModifier(StatType_UMFOSS.Damage, mod);
            _swordEquipped = true;
        }

        /// <summary>Apply potion → PercentAdd +20% damage (if not already active).</summary>
        private void OnAddPercentDamage()
        {
            if (_potionActive)
            {
                Debug.Log("[Demo] Potion already active.");
                return;
            }
            var mod = new StatModifier_UMFOSS(POTION_PERCENT_ADD, ModifierType_UMFOSS.PercentAdd, order: 0, source: _potionSource);
            playerStatSheet.AddModifier(StatType_UMFOSS.Damage, mod);
            _potionActive = true;
        }

        /// <summary>Equip relic → PercentMultiply ×1.5 damage (if not already active).</summary>
        private void OnAddMultiplyDamage()
        {
            if (_relicActive)
            {
                Debug.Log("[Demo] Relic already active.");
                return;
            }
            var mod = new StatModifier_UMFOSS(RELIC_PERCENT_MULTIPLY, ModifierType_UMFOSS.PercentMultiply, order: 0, source: _relicSource);
            playerStatSheet.AddModifier(StatType_UMFOSS.Damage, mod);
            _relicActive = true;
        }

        /// <summary>
        /// Unequip sword → removes ALL modifiers the sword source added, across
        /// every stat, in a single call.
        /// </summary>
        private void OnRemoveSword()
        {
            playerStatSheet.RemoveAllModifiersFromSource(_swordSource);
            _swordEquipped = false;
        }

        /// <summary>
        /// Applies a -30% MoveSpeed debuff for 5 seconds.
        /// The modifier removes itself via TimerUtility_UMFOSS — no coroutines.
        /// </summary>
        private void OnApplySpeedDebuff()
        {
            if (Time.time < _speedDebuffEndTime)
            {
                Debug.Log("[Demo] Speed debuff already active.");
                return;
            }

            var debuffSource = new object(); // fresh source per application
            var mod = new StatModifier_UMFOSS(SPEED_DEBUFF_PERCENT, ModifierType_UMFOSS.PercentAdd,
                                              order: 0, source: debuffSource);
            playerStatSheet.AddTimedModifier(StatType_UMFOSS.MoveSpeed, mod, SPEED_DEBUFF_DURATION);
            _speedDebuffEndTime = Time.time + SPEED_DEBUFF_DURATION;
        }

        /// <summary>Removes all modifiers from all stats and resets to base values.</summary>
        private void OnResetAll()
        {
            playerStatSheet.ResetToBase();
            _swordEquipped = false;
            _potionActive  = false;
            _relicActive   = false;
            _speedDebuffEndTime = -1f;
        }

        // ─── UI Refresh ───────────────────────────────────────────────────────

        private void RefreshUI()
        {
            // Damage
            var damageStat = playerStatSheet.GetStat(StatType_UMFOSS.Damage);
            if (damageStat != null)
            {
                if (labelBaseDamage  != null) labelBaseDamage.text  = $"Base Damage  : {damageStat.BaseValue:F2}";
                if (labelFinalDamage != null) labelFinalDamage.text = $"Final Damage : <b>{damageStat.FinalValue:F2}</b>";
                if (labelActiveModifiers != null)
                    labelActiveModifiers.text = BuildModifierList(StatType_UMFOSS.Damage, damageStat.GetModifiers());
            }

            // Move Speed
            var speedStat = playerStatSheet.GetStat(StatType_UMFOSS.MoveSpeed);
            if (speedStat != null)
            {
                if (labelBaseMoveSpeed  != null) labelBaseMoveSpeed.text  = $"Base Speed   : {speedStat.BaseValue:F2}";
                if (labelFinalMoveSpeed != null) labelFinalMoveSpeed.text = $"Final Speed  : <b>{speedStat.FinalValue:F2}</b>";
            }

            // Defence
            var defenceStat = playerStatSheet.GetStat(StatType_UMFOSS.Defence);
            if (defenceStat != null)
            {
                if (labelBaseDefence  != null) labelBaseDefence.text  = $"Base Defence : {defenceStat.BaseValue:F2}";
                if (labelFinalDefence != null) labelFinalDefence.text = $"Final Defence: <b>{defenceStat.FinalValue:F2}</b>";
            }
        }

        private void UpdateDebuffTimer()
        {
            if (labelSpeedDebuffTimer == null) return;

            float remaining = _speedDebuffEndTime - Time.time;
            if (remaining > 0)
                labelSpeedDebuffTimer.text = $"Speed Debuff : <color=red>{remaining:F1}s remaining</color>";
            else
                labelSpeedDebuffTimer.text = "Speed Debuff : <color=green>none</color>";
        }

        private string BuildModifierList(StatType_UMFOSS type, List<StatModifier_UMFOSS> mods)
        {
            _sb.Clear();
            _sb.AppendLine($"Active Modifiers ({type}):");
            if (mods.Count == 0)
            {
                _sb.AppendLine("  <i>none</i>");
                return _sb.ToString();
            }
            foreach (var m in mods)
            {
                string typeName   = m.Type.ToString();
                string sourceLabel;
                if      (ReferenceEquals(m.Source, _swordSource))  sourceLabel = "Sword";
                else if (ReferenceEquals(m.Source, _potionSource)) sourceLabel = "Potion";
                else if (ReferenceEquals(m.Source, _relicSource))  sourceLabel = "Relic";
                else                                               sourceLabel = "Debuff";

                string sign = m.Value >= 0 ? "+" : "";
                string valueStr = m.Type == ModifierType_UMFOSS.PercentMultiply
                    ? $"×{m.Value}"
                    : $"{sign}{m.Value * (m.Type == ModifierType_UMFOSS.Flat ? 1f : 100f):F0}{(m.Type == ModifierType_UMFOSS.Flat ? "" : "%")}";

                _sb.AppendLine($"  [{typeName}] {valueStr}   (src: {sourceLabel})");
            }
            return _sb.ToString();
        }

        // ─── Event Handlers ───────────────────────────────────────────────────

        private void OnStatChanged(StatChangedEvent e)
        {
            Debug.Log($"[StatSystem] {e.StatType} changed: {e.OldValue:F4} → {e.NewValue:F4}");
        }

        private void OnModifierExpired(StatModifierExpiredEvent e)
        {
            Debug.Log($"[StatSystem] Timed modifier on {e.StatType} expired.");
            if (e.StatType == StatType_UMFOSS.MoveSpeed)
                _speedDebuffEndTime = -1f;
        }
    }
}
