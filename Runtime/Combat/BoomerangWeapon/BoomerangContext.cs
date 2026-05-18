using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Physics;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Shared context passed to all boomerang weapon states.
    /// Contains references to components and the state machine so states can
    /// trigger transitions and access shared data without coupling to the MonoBehaviour.
    /// </summary>
    public class BoomerangContext
    {
        public Transform Transform;
        public IPhysicsAdapter Physics;
        public Collider WeaponCollider;
        public Transform HandSocket;
        public BoomerangConfig Config;
        public WeaponVisualHandler VisualHandler;
        public HitDetector HitDetector;
        public StateMachine<WeaponStateKey> StateMachine;

        /// <summary>Stored throw direction for the current flight.</summary>
        public Vector3 ThrowDirection;

        /// <summary>World position where the current throw originated.</summary>
        public Vector3 ThrowOrigin;

        /// <summary>Collision data from the most recent surface impact. Set before entering Embedded state.</summary>
        public UnityEngine.Collision LastCollision;

        /// <summary>Collider of the last hurtbox target hit via overlap detection. Set before entering Embedded state when embedding in targets.</summary>
        public Collider LastHurtboxHit;

        /// <summary>Cached collider of the object the weapon is currently embedded in, used to exclude from recall hit detection.</summary>
        public Collider EmbeddedInCollider;
    }

    /// <summary>
    /// Enum keys for the boomerang weapon state machine.
    /// </summary>
    public enum WeaponStateKey
    {
        Equipped,
        Thrown,
        Embedded,
        Recalling
    }
}
