using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>Fired on throw. Direction can drive directional audio/VFX.</summary>
    public struct WeaponThrownEvent
    {
        public Vector3 Direction;
    }

    /// <summary>Fired when the weapon embeds into a surface.</summary>
    public struct WeaponStuckEvent
    {
        public GameObject Surface;
        public Vector3 Point;
    }

    /// <summary>Fired when the player initiates a recall.</summary>
    public struct WeaponRecallStartedEvent { }

    /// <summary>Fired when the weapon snaps back into the hand.</summary>
    public struct WeaponCaughtEvent { }
}
