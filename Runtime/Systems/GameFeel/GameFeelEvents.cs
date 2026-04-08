using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Event definitions for the Game Feel system.
    /// Any mechanic can publish these events to trigger visual feedback
    /// without coupling to the GameFeel_UMFOSS component directly.
    /// </summary>

    /// <summary>
    /// Published when an attack connects with a target.
    /// Triggers: Hitpause + Screen Flash (white).
    /// </summary>
    public struct OnHitRegistered
    {
        public Vector3 hitPoint;
        public float intensity;
    }

    /// <summary>
    /// Published when the player or an entity takes damage.
    /// Triggers: Hitpause + Squash/Stretch (hit) + Screen Flash (white).
    /// </summary>
    public struct OnDamageTaken
    {
        public float damageAmount;
        public Vector3 hitDirection;
    }

    /// <summary>
    /// Published when an entity dies.
    /// Triggers: Screen Flash (red) + Squash/Stretch (death).
    /// </summary>
    public struct OnDeath
    {
        public Vector3 deathPosition;
    }

    /// <summary>
    /// Published at the moment a jump begins.
    /// Triggers: Squash/Stretch (stretch upward).
    /// </summary>
    public struct OnJumpStart
    {
        public float jumpForce;
    }

    /// <summary>
    /// Published the frame the player touches ground after being airborne.
    /// Triggers: Squash/Stretch (squash flat).
    /// </summary>
    public struct OnLanding
    {
        public float fallSpeed;
    }

    /// <summary>
    /// Published when a dash ability activates.
    /// Triggers: Afterimage enable + Ghost Trail enable.
    /// </summary>
    public struct OnDashStart
    {
        public Vector2 dashDirection;
    }

    /// <summary>
    /// Published when a dash ability ends.
    /// Triggers: Afterimage disable + Ghost Trail disable.
    /// </summary>
    public struct OnDashEnd { }

    /// <summary>
    /// Published when the player picks up an item.
    /// Triggers: Squash/Stretch (pop) + Screen Flash (yellow subtle).
    /// </summary>
    public struct OnItemPickedUp
    {
        public string itemName;
    }
}
