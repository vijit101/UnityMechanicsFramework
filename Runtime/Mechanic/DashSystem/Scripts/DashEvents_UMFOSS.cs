using UnityEngine;

namespace GameplayMechanicsUMFOSS.Movement
{
    // ─────────────────────────────────────────────
    // Dash EventBus Events
    // ─────────────────────────────────────────────
    // These structs are published on the EventBus so that any system
    // (audio, VFX, hurtbox, UI) can react to dash state changes
    // without holding a direct reference to the DashSystem.

    /// <summary>
    /// Published the frame a dash begins.
    /// Subscribers can use this to trigger dash VFX, audio, or trail effects.
    /// </summary>
    public struct DashStartEvent
    {
        /// <summary>World-space direction of the dash.</summary>
        public Vector3 direction;

        /// <summary>Total duration of the dash in seconds.</summary>
        public float duration;
    }

    /// <summary>
    /// Published the frame a dash ends (duration fully elapsed).
    /// Subscribers can use this to stop VFX/audio or restore normal state.
    /// </summary>
    public struct DashEndEvent { }

    /// <summary>
    /// Published when the iframe window activates during a dash.
    /// Hurtbox systems should subscribe to this and disable the player's hurtbox
    /// without the dash system needing a direct reference to the hurtbox component.
    /// </summary>
    public struct DashIframeStartEvent { }

    /// <summary>
    /// Published when the iframe window ends.
    /// Hurtbox systems should re-enable the player's hurtbox on receiving this.
    /// </summary>
    public struct DashIframeEndEvent { }

    /// <summary>
    /// Published every time a dash charge is consumed or restored.
    /// UI elements displaying dash charges should subscribe to this.
    /// </summary>
    public struct DashCountChangedEvent
    {
        /// <summary>Number of dash charges remaining after the change.</summary>
        public int remaining;
    }

    /// <summary>
    /// Published when the cooldown fully resets and the player can dash again.
    /// Useful for UI cooldown indicators or audio cues.
    /// </summary>
    public struct DashReadyEvent { }

    /// <summary>
    /// Published when the dash collides with an enemy while dashCanKillEnemies is enabled.
    /// Enemy health/death systems can subscribe to handle damage or destruction.
    /// </summary>
    public struct DashKillEvent
    {
        /// <summary>The GameObject that was hit by the dash.</summary>
        public GameObject target;
    }

    /// <summary>
    /// External event that other systems publish when the player kills an enemy.
    /// If resetCooldownOnKill is enabled, the DashSystem subscribes to this
    /// and immediately resets its cooldown timer.
    /// This struct can live anywhere — it is placed here for convenience
    /// but any system can define and publish it.
    /// </summary>
    public struct PlayerKillEvent { }
}
