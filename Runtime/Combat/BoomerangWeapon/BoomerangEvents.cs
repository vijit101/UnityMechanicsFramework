using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Event structs for the boomerang weapon system.
    /// Published via EventBus for decoupled communication with VFX, audio, UI, and camera systems.
    /// </summary>

    /// <summary>Fired when the weapon is thrown.</summary>
    public struct WeaponThrownEvent
    {
        public Vector3 Direction;
        public GameObject Weapon;
    }

    /// <summary>Fired when the weapon embeds into a surface.</summary>
    public struct WeaponStuckEvent
    {
        public GameObject Surface;
        public Vector3 ContactPoint;
        public GameObject Weapon;
    }

    /// <summary>Fired when recall begins.</summary>
    public struct WeaponRecallStartedEvent
    {
        public GameObject Weapon;
    }

    /// <summary>Fired when the weapon returns to the player's hand.</summary>
    public struct WeaponCaughtEvent
    {
        public GameObject Weapon;
    }

    /// <summary>Fired when the weapon hits a target during flight or recall.</summary>
    public struct WeaponHitEvent
    {
        public GameObject Target;
        public GameObject Weapon;
        public Vector3 HitPosition;
        public float DamageDealt;
    }
}
