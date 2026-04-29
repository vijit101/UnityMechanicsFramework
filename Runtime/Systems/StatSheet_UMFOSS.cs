using System.Collections.Generic;
using UnityEngine;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Systems
{
    // =========================================================================
    //  StatSheet_UMFOSS — MonoBehaviour
    //
    //  Owns a complete set of Stat_UMFOSS instances for one entity.
    //  All stats are initialised from Inspector base values on Awake.
    //
    //  IMPORTANT: This file is named StatSheet_UMFOSS.cs to match the class
    //  name — Unity requires filename == MonoBehaviour class name.
    // =========================================================================

    /// <summary>
    /// A MonoBehaviour that owns a complete set of <see cref="Stat_UMFOSS"/>
    /// instances for one entity (player, enemy, item, etc.).
    ///
    /// <para>Add this component to any GameObject. Set base values in the
    /// Inspector. Use <see cref="AddModifier"/> and
    /// <see cref="RemoveAllModifiersFromSource"/> to manage modifiers at runtime.
    /// Changes are communicated via <see cref="StatEventBus"/>.</para>
    /// </summary>
    public class StatSheet_UMFOSS : MonoBehaviour
    {
        // ─── Serialized Base Values ───────────────────────────────────────────

        [Header("Base Stats")]
        [SerializeField] private float baseMaxHealth     = 100f;
        [SerializeField] private float baseCurrentHealth = 100f;
        [SerializeField] private float baseDamage        = 10f;
        [SerializeField] private float baseDefence       = 0f;
        [SerializeField] private float baseMoveSpeed     = 5f;
        [SerializeField] private float baseAttackSpeed   = 1f;
        [SerializeField] private float baseAttackRange   = 2f;
        [SerializeField] private float baseCritChance    = 0.05f;
        [SerializeField] private float baseCritMult      = 1.5f;
        [SerializeField] private float baseMana          = 100f;
        [SerializeField] private float baseStamina       = 100f;
        [SerializeField] private float baseLuck          = 0f;

        // ─── Private Fields ───────────────────────────────────────────────────

        private Dictionary<StatType_UMFOSS, Stat_UMFOSS> _stats;

        // ─── Unity Lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            InitialiseStats();
        }

        // ─── Public Access ────────────────────────────────────────────────────

        /// <summary>Returns the <see cref="Stat_UMFOSS"/> for the given type.</summary>
        public Stat_UMFOSS GetStat(StatType_UMFOSS type)
        {
            if (_stats.TryGetValue(type, out var stat)) return stat;
            Debug.LogWarning($"[StatSheet] Stat {type} not found.");
            return null;
        }

        /// <summary>Shorthand for GetStat(type).FinalValue.</summary>
        public float GetValue(StatType_UMFOSS type)
        {
            return GetStat(type)?.FinalValue ?? 0f;
        }

        // ─── Modifier Management ──────────────────────────────────────────────

        /// <summary>Adds a modifier to a stat and fires change events.</summary>
        public void AddModifier(StatType_UMFOSS type, StatModifier_UMFOSS modifier)
        {
            var stat = GetStat(type);
            if (stat == null) return;

            float oldValue = stat.FinalValue;
            stat.AddModifier(modifier);
            float newValue = stat.FinalValue;

            StatEventBus.Publish(new ModifierAddedEvent
                { StatType = type, Modifier = modifier, Sheet = this });

            if (!Mathf.Approximately(oldValue, newValue))
                StatEventBus.Publish(new StatChangedEvent
                    { StatType = type, OldValue = oldValue, NewValue = newValue, Sheet = this });
        }

        /// <summary>Removes a specific modifier from a stat by reference.</summary>
        public void RemoveModifier(StatType_UMFOSS type, StatModifier_UMFOSS modifier)
        {
            var stat = GetStat(type);
            if (stat == null) return;

            float oldValue = stat.FinalValue;
            if (!stat.RemoveModifier(modifier)) return;
            float newValue = stat.FinalValue;

            StatEventBus.Publish(new ModifierRemovedEvent
                { StatType = type, Modifier = modifier, Sheet = this });

            if (!Mathf.Approximately(oldValue, newValue))
                StatEventBus.Publish(new StatChangedEvent
                    { StatType = type, OldValue = oldValue, NewValue = newValue, Sheet = this });
        }

        /// <summary>
        /// Removes all modifiers from EVERY stat whose Source matches
        /// <paramref name="source"/> by reference equality.
        /// One call removes everything a sword/potion/effect applied.
        /// </summary>
        public void RemoveAllModifiersFromSource(object source)
        {
            foreach (var kvp in _stats)
            {
                float oldValue = kvp.Value.FinalValue;
                if (!kvp.Value.RemoveAllModifiersFromSource(source)) continue;
                float newValue = kvp.Value.FinalValue;
                if (!Mathf.Approximately(oldValue, newValue))
                    StatEventBus.Publish(new StatChangedEvent
                        { StatType = kvp.Key, OldValue = oldValue, NewValue = newValue, Sheet = this });
            }
        }

        /// <summary>Removes all modifiers from all stats. Base values unchanged.</summary>
        public void ResetToBase()
        {
            foreach (var stat in _stats.Values)
                stat.RemoveAllModifiers();

            StatEventBus.Publish(new AllModifiersClearedEvent { Sheet = this });
        }

        // ─── Timed Modifier ───────────────────────────────────────────────────

        /// <summary>
        /// Adds a modifier that automatically removes itself after
        /// <paramref name="duration"/> seconds via <see cref="TimerUtility_UMFOSS"/>.
        /// No coroutines. No manual tracking.
        /// </summary>
        public void AddTimedModifier(StatType_UMFOSS type, StatModifier_UMFOSS modifier, float duration)
        {
            AddModifier(type, modifier);

            var timer = TimerUtility_UMFOSS.Create(duration, () =>
            {
                RemoveModifier(type, modifier);
                StatEventBus.Publish(new StatModifierExpiredEvent
                    { StatType = type, Modifier = modifier, Sheet = this });
            });

            timer.Start();
        }

        // ─── Snapshot ─────────────────────────────────────────────────────────

        /// <summary>
        /// Returns final values of all stats as a plain dictionary.
        /// Usable directly by a save system — no coupling to this class.
        /// </summary>
        public Dictionary<StatType_UMFOSS, float> GetSnapshot()
        {
            var snapshot = new Dictionary<StatType_UMFOSS, float>(_stats.Count);
            foreach (var kvp in _stats)
                snapshot[kvp.Key] = kvp.Value.FinalValue;
            return snapshot;
        }

        // ─── Private Methods ──────────────────────────────────────────────────

        private void InitialiseStats()
        {
            _stats = new Dictionary<StatType_UMFOSS, Stat_UMFOSS>
            {
                { StatType_UMFOSS.MaxHealth,      new Stat_UMFOSS(baseMaxHealth)     },
                { StatType_UMFOSS.CurrentHealth,  new Stat_UMFOSS(baseCurrentHealth) },
                { StatType_UMFOSS.Damage,         new Stat_UMFOSS(baseDamage)        },
                { StatType_UMFOSS.Defence,        new Stat_UMFOSS(baseDefence)       },
                { StatType_UMFOSS.MoveSpeed,      new Stat_UMFOSS(baseMoveSpeed)     },
                { StatType_UMFOSS.AttackSpeed,    new Stat_UMFOSS(baseAttackSpeed)   },
                { StatType_UMFOSS.AttackRange,    new Stat_UMFOSS(baseAttackRange)   },
                { StatType_UMFOSS.CritChance,     new Stat_UMFOSS(baseCritChance)    },
                { StatType_UMFOSS.CritMultiplier, new Stat_UMFOSS(baseCritMult)      },
                { StatType_UMFOSS.Mana,           new Stat_UMFOSS(baseMana)          },
                { StatType_UMFOSS.Stamina,        new Stat_UMFOSS(baseStamina)       },
                { StatType_UMFOSS.Luck,           new Stat_UMFOSS(baseLuck)          },
                { StatType_UMFOSS.Custom,         new Stat_UMFOSS(0f)                }
            };
        }
    }
}
