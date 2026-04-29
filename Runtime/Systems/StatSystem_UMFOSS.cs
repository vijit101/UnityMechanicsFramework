using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    // =========================================================================
    //  STAT SYSTEM — GameplayMechanicsUMFOSS.Systems
    //
    //  Architecture:
    //    StatType_UMFOSS      (enum)          — which stat
    //    ModifierType_UMFOSS  (enum)          — Flat / PercentAdd / PercentMultiply
    //    StatModifier_UMFOSS  (plain class)   — one modifier: value, type, order, source
    //    Stat_UMFOSS          (plain class)   — one stat: base value + modifier list + dirty cache
    //    StatSheet_UMFOSS     (MonoBehaviour) — lives in StatSheet_UMFOSS.cs (separate file)
    //    StatEventBus         (static class)  — lightweight publish/subscribe (self-contained)
    //    Event structs                        — StatChangedEvent, ModifierAddedEvent, etc.
    //
    //  Stacking order (industry standard — flat THEN percentage):
    //    FinalValue = (BaseValue + ΣFlat) × (1 + ΣPercentAdd) × ΠPercentMultiply
    // =========================================================================


    // ─────────────────────────────────────────────────────────────────────────
    //  StatType_UMFOSS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Enumeration of every addressable stat.  Add new stat types here — no
    /// other code needs to change.
    /// </summary>
    public enum StatType_UMFOSS
    {
        MaxHealth,
        CurrentHealth,
        Damage,
        Defence,
        MoveSpeed,
        AttackSpeed,
        AttackRange,
        CritChance,
        CritMultiplier,
        Mana,
        Stamina,
        Luck,
        Custom  // extend here for game-specific stats
    }


    // ─────────────────────────────────────────────────────────────────────────
    //  ModifierType_UMFOSS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Determines how a <see cref="StatModifier_UMFOSS"/> is applied.
    /// </summary>
    public enum ModifierType_UMFOSS
    {
        /// <summary>Adds a fixed value.  Stacks additively. Applies first.</summary>
        Flat,

        /// <summary>
        /// Adds a percentage of the flat-adjusted base.
        /// Multiple PercentAdd modifiers stack additively: +20% + +20% = +40%, not ×1.44.
        /// </summary>
        PercentAdd,

        /// <summary>
        /// Multiplies the current total.  Stacks multiplicatively.
        /// Reserved for intentional large power spikes (boss mode, legendaries).
        /// </summary>
        PercentMultiply
    }


    // ─────────────────────────────────────────────────────────────────────────
    //  StatModifier_UMFOSS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// A single modifier that can be applied to a <see cref="Stat_UMFOSS"/>.
    /// The <b>source</b> field is the identity key for removal.
    /// </summary>
    public class StatModifier_UMFOSS
    {
        /// <summary>The modifier amount. For PercentAdd/PercentMultiply use decimal: 0.2f = 20%.</summary>
        public readonly float              Value;

        /// <summary>Flat, PercentAdd, or PercentMultiply.</summary>
        public readonly ModifierType_UMFOSS Type;

        /// <summary>Sort order within the same type. Lower runs first. Default 0.</summary>
        public readonly int                Order;

        /// <summary>
        /// The object that applied this modifier. Used for source-based removal via
        /// reference equality — pass the same reference to remove everything it added.
        /// </summary>
        public readonly object             Source;

        public StatModifier_UMFOSS(float value, ModifierType_UMFOSS type, int order = 0, object source = null)
        {
            Value  = value;
            Type   = type;
            Order  = order;
            Source = source;
        }
    }


    // ─────────────────────────────────────────────────────────────────────────
    //  Stat_UMFOSS
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Represents one numeric stat. Plain C# class owned by <see cref="StatSheet_UMFOSS"/>.
    /// Uses a dirty flag: FinalValue only recalculates when modifiers have changed.
    /// </summary>
    public class Stat_UMFOSS
    {
        private const int ROUND_DIGITS = 4;

        /// <summary>The unmodified base value set in the Inspector.</summary>
        public float BaseValue;

        private readonly List<StatModifier_UMFOSS> _modifiers = new List<StatModifier_UMFOSS>();
        private bool  _isDirty          = true;
        private float _cachedFinalValue;

        /// <summary>
        /// The final computed value after all modifiers.
        /// Recalculates only when modifiers change. Safe to call every frame.
        /// </summary>
        public float FinalValue
        {
            get
            {
                if (_isDirty)
                {
                    _cachedFinalValue = CalculateFinalValue();
                    _isDirty          = false;
                }
                return _cachedFinalValue;
            }
        }

        public Stat_UMFOSS(float baseValue)
        {
            BaseValue = baseValue;
        }

        public void AddModifier(StatModifier_UMFOSS modifier)
        {
            _modifiers.Add(modifier);
            _isDirty = true;
        }

        public bool RemoveModifier(StatModifier_UMFOSS modifier)
        {
            bool removed = _modifiers.Remove(modifier);
            if (removed) _isDirty = true;
            return removed;
        }

        public bool RemoveAllModifiersFromSource(object source)
        {
            int count = _modifiers.RemoveAll(m => ReferenceEquals(m.Source, source));
            if (count > 0) { _isDirty = true; return true; }
            return false;
        }

        public void RemoveAllModifiers()
        {
            if (_modifiers.Count == 0) return;
            _modifiers.Clear();
            _isDirty = true;
        }

        public float GetBaseValue() => BaseValue;

        public List<StatModifier_UMFOSS> GetModifiers() => new List<StatModifier_UMFOSS>(_modifiers);

        /// <summary>
        /// FinalValue = (BaseValue + ΣFlat) × (1 + ΣPercentAdd) × ΠPercentMultiply
        /// </summary>
        private float CalculateFinalValue()
        {
            float flat            = BaseValue;
            float percentAdd      = 0f;
            float percentMultiply = 1f;

            foreach (var mod in _modifiers.OrderBy(m => m.Type).ThenBy(m => m.Order))
            {
                switch (mod.Type)
                {
                    case ModifierType_UMFOSS.Flat:            flat            += mod.Value; break;
                    case ModifierType_UMFOSS.PercentAdd:      percentAdd      += mod.Value; break;
                    case ModifierType_UMFOSS.PercentMultiply: percentMultiply *= mod.Value; break;
                }
            }

            return (float)Math.Round(flat * (1 + percentAdd) * percentMultiply, ROUND_DIGITS);
        }
    }


    // ─────────────────────────────────────────────────────────────────────────
    //  Event Structs
    // ─────────────────────────────────────────────────────────────────────────

    public struct StatChangedEvent
    {
        public StatType_UMFOSS   StatType;
        public float             OldValue;
        public float             NewValue;
        public StatSheet_UMFOSS  Sheet;
    }

    public struct ModifierAddedEvent
    {
        public StatType_UMFOSS      StatType;
        public StatModifier_UMFOSS  Modifier;
        public StatSheet_UMFOSS     Sheet;
    }

    public struct ModifierRemovedEvent
    {
        public StatType_UMFOSS      StatType;
        public StatModifier_UMFOSS  Modifier;
        public StatSheet_UMFOSS     Sheet;
    }

    public struct StatModifierExpiredEvent
    {
        public StatType_UMFOSS      StatType;
        public StatModifier_UMFOSS  Modifier;
        public StatSheet_UMFOSS     Sheet;
    }

    public struct AllModifiersClearedEvent
    {
        public StatSheet_UMFOSS Sheet;
    }


    // ─────────────────────────────────────────────────────────────────────────
    //  StatEventBus
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Minimal self-contained type-safe event bus.
    /// </summary>
    public static class StatEventBus
    {
        private static readonly Dictionary<Type, List<object>> _handlers
            = new Dictionary<Type, List<object>>();

        public static void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<object>();
            _handlers[type].Add(handler);
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        public static void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (!_handlers.TryGetValue(type, out var list)) return;
            foreach (var handler in list.ToList())
                ((Action<T>)handler)?.Invoke(eventData);
        }
    }
}
