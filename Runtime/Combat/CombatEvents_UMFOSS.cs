using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    public enum DamagePresentation
    {
        Damage = 0,
        DamageTaken = 1,
        PoisonDamage = 2
    }

    public readonly struct DamageTakenEvent
    {
        public DamageTakenEvent(Transform target, float amount, DamagePresentation presentation)
        {
            Target = target;
            Amount = amount;
            Presentation = presentation;
        }

        public Transform Target { get; }
        public float Amount { get; }
        public DamagePresentation Presentation { get; }
    }

    public readonly struct HealedEvent
    {
        public HealedEvent(Transform target, float amount)
        {
            Target = target;
            Amount = amount;
        }

        public Transform Target { get; }
        public float Amount { get; }
    }

    public readonly struct CriticalHitEvent
    {
        public CriticalHitEvent(Transform target, float amount)
        {
            Target = target;
            Amount = amount;
        }

        public Transform Target { get; }
        public float Amount { get; }
    }

    public readonly struct ShieldBlockEvent
    {
        public ShieldBlockEvent(Transform target, float amount)
        {
            Target = target;
            Amount = amount;
        }

        public Transform Target { get; }
        public float Amount { get; }
    }

    public readonly struct MissEvent
    {
        public MissEvent(Transform target)
        {
            Target = target;
        }

        public Transform Target { get; }
    }

    public readonly struct ExperienceGainedEvent
    {
        public ExperienceGainedEvent(Transform target, float amount)
        {
            Target = target;
            Amount = amount;
        }

        public Transform Target { get; }
        public float Amount { get; }
    }
}
