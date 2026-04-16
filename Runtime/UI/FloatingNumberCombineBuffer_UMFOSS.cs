using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.UI
{
    /// <summary>
    /// Buffers rapid numeric hits so they can be emitted as one combined popup after a short delay.
    /// </summary>
    public sealed class FloatingNumberCombineBuffer_UMFOSS
    {
        public readonly struct CombinedNumber
        {
            public CombinedNumber(int targetInstanceId, NumberType type, float amount, Vector3 position)
            {
                TargetInstanceId = targetInstanceId;
                Type = type;
                Amount = amount;
                Position = position;
            }

            public int TargetInstanceId { get; }
            public NumberType Type { get; }
            public float Amount { get; }
            public Vector3 Position { get; }
        }

        private readonly struct CombineKey : IEquatable<CombineKey>
        {
            public CombineKey(int targetInstanceId, NumberType type)
            {
                TargetInstanceId = targetInstanceId;
                Type = type;
            }

            public int TargetInstanceId { get; }
            public NumberType Type { get; }

            public bool Equals(CombineKey other)
            {
                return TargetInstanceId == other.TargetInstanceId && Type == other.Type;
            }

            public override bool Equals(object obj)
            {
                return obj is CombineKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (TargetInstanceId * 397) ^ (int)Type;
                }
            }
        }

        private struct PendingCombine
        {
            public float totalAmount;
            public Vector3 position;
            public float expiryTime;
        }

        private readonly Dictionary<CombineKey, PendingCombine> pendingCombines = new Dictionary<CombineKey, PendingCombine>();

        public int Count => pendingCombines.Count;

        public void Add(int targetInstanceId, NumberType type, float amount, Vector3 position, float currentTime, float combineWindow)
        {
            CombineKey key = new CombineKey(targetInstanceId, type);
            if (pendingCombines.TryGetValue(key, out PendingCombine pendingCombine))
            {
                pendingCombine.totalAmount += amount;
                pendingCombine.position = position;
                pendingCombine.expiryTime = currentTime + combineWindow;
                pendingCombines[key] = pendingCombine;
                return;
            }

            pendingCombines[key] = new PendingCombine
            {
                totalAmount = amount,
                position = position,
                expiryTime = currentTime + combineWindow
            };
        }

        public void CollectExpired(float currentTime, List<CombinedNumber> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (pendingCombines.Count == 0)
            {
                return;
            }

            List<CombineKey> expiredKeys = null;
            foreach (KeyValuePair<CombineKey, PendingCombine> entry in pendingCombines)
            {
                if (entry.Value.expiryTime > currentTime)
                {
                    continue;
                }

                if (expiredKeys == null)
                {
                    expiredKeys = new List<CombineKey>();
                }

                expiredKeys.Add(entry.Key);
                results.Add(new CombinedNumber(entry.Key.TargetInstanceId, entry.Key.Type, entry.Value.totalAmount, entry.Value.position));
            }

            if (expiredKeys == null)
            {
                return;
            }

            for (int index = 0; index < expiredKeys.Count; index++)
            {
                pendingCombines.Remove(expiredKeys[index]);
            }
        }

        public void FlushAll(List<CombinedNumber> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (KeyValuePair<CombineKey, PendingCombine> entry in pendingCombines)
            {
                results.Add(new CombinedNumber(entry.Key.TargetInstanceId, entry.Key.Type, entry.Value.totalAmount, entry.Value.position));
            }

            pendingCombines.Clear();
        }

        public void Clear()
        {
            pendingCombines.Clear();
        }
    }
}
