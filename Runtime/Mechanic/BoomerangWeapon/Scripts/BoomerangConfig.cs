using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// ScriptableObject holding all configuration for the boomerang weapon.
    /// Create multiple assets for different weapon types (axe, spear, hammer)
    /// without duplicating MonoBehaviour components.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBoomerangConfig", menuName = "UMF/Combat/Boomerang Config")]
    public class BoomerangConfig : ScriptableObject
    {
        [Header("Throw Settings")]
        [Tooltip("Initial velocity applied on throw (ignores mass).")]
        [SerializeField] private float throwForce = 30f;

        [Tooltip("Speed multiplier over normalized distance. X = 0-1 distance ratio, Y = speed multiplier.")]
        [SerializeField] private AnimationCurve throwCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [Tooltip("Auto-recalls if the weapon travels beyond this distance.")]
        [SerializeField] private float maxRange = 25f;

        [Header("Return Settings")]
        [Tooltip("Base return flight speed.")]
        [SerializeField] private float returnSpeed = 15f;

        [Tooltip("Speed increase per second during return for the 'snap' effect.")]
        [SerializeField] private float returnAcceleration = 2f;

        [Tooltip("Distance to hand socket that triggers the catch.")]
        [SerializeField] private float catchDistance = 0.5f;

        [Header("Return Arc")]
        [Tooltip("Upward offset of the Bezier control point as a ratio of return distance.")]
        [SerializeField] private float arcHeightRatio = 0.3f;

        [Tooltip("Sideways offset of the Bezier control point as a ratio of return distance.")]
        [SerializeField] private float arcSideRatio = 0.15f;

        [Header("Damage")]
        [Tooltip("Damage dealt to targets on the outgoing throw.")]
        [SerializeField] private float throwDamage = 25f;

        [Tooltip("Damage dealt to targets on the return flight.")]
        [SerializeField] private float returnDamage = 15f;

        [Tooltip("Knockback impulse applied to targets with a Rigidbody.")]
        [SerializeField] private float knockbackForce = 8f;

        [Header("Collision & Layers")]
        [Tooltip("Layers that take damage when the weapon passes through.")]
        [SerializeField] private LayerMask hurtboxLayer;

        [Tooltip("Layers where the weapon embeds on collision.")]
        [SerializeField] private LayerMask stuckLayer;

        [Tooltip("If true, the weapon also embeds in hurtbox targets on hit.")]
        [SerializeField] private bool embedInTargets = true;

        [Header("Visual")]
        [Tooltip("Spin speed in degrees per second per axis applied to the visual pivot.")]
        [SerializeField] private Vector3 rotationPerSecond = new Vector3(0f, 0f, 1080f);

        // ─── Public Accessors ───

        public float ThrowForce => throwForce;
        public AnimationCurve ThrowCurve => throwCurve;
        public float MaxRange => maxRange;
        public float ReturnSpeed => returnSpeed;
        public float ReturnAcceleration => returnAcceleration;
        public float CatchDistance => catchDistance;
        public float ArcHeightRatio => arcHeightRatio;
        public float ArcSideRatio => arcSideRatio;
        public float ThrowDamage => throwDamage;
        public float ReturnDamage => returnDamage;
        public float KnockbackForce => knockbackForce;
        public LayerMask HurtboxLayer => hurtboxLayer;
        public LayerMask StuckLayer => stuckLayer;
        public bool EmbedInTargets => embedInTargets;
        public Vector3 RotationPerSecond => rotationPerSecond;
    }
}
