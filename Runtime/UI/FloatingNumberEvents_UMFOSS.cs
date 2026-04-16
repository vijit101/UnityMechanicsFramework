using UnityEngine;

namespace GameplayMechanicsUMFOSS.UI
{
    public readonly struct FloatingNumberSpawnedEvent
    {
        public FloatingNumberSpawnedEvent(NumberType type, float amount, Vector3 position)
        {
            Type = type;
            Amount = amount;
            Position = position;
        }

        public NumberType Type { get; }
        public float Amount { get; }
        public Vector3 Position { get; }
    }

    public readonly struct FloatingNumberReturnedEvent
    {
        public FloatingNumberReturnedEvent(NumberType type)
        {
            Type = type;
        }

        public NumberType Type { get; }
    }
}
