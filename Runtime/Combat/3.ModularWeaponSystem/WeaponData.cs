// Author: Aditya Jaiswal, Atharv S. Jain
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Plain data snapshot of a weapon's identity and stats. Struct (value type)
    /// so it can be passed through events without GC allocations.
    /// </summary>
    [System.Serializable]
    public struct WeaponData
    {
        public string name;
        public Sprite skin;
        public float damage;
        public float range;
        public float weight;
    }
}
