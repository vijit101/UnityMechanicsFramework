using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// 3D physics adapter using Rigidbody.
    /// Ground detection uses Physics.SphereCast.
    /// Since Rigidbody has no built-in gravity scale, gravity is applied
    /// manually in FixedUpdate using the GravityScale property.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Gameplay Mechanics UMFOSS/Physics/Physics 3D Adapter")]
    public class Physics3DAdapter_UMFOSS : MonoBehaviour, IPhysicsAdapter
    {
        // Serialized fields
        [Header("Ground Detection")]
        [Tooltip("Radius of the sphere used for ground detection via SphereCast")]
        [SerializeField] private float sphereCastRadius = 0.3f;

        // Private fields
        private Rigidbody rb;
        private float gravityScale = 1f;

        // IPhysicsAdapter implementation
        public Vector3 Velocity
        {
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
        }

        public float GravityScale
        {
            get => gravityScale;
            set => gravityScale = value;
        }

        // Unity lifecycle
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
        }

        private void FixedUpdate()
        {
            // Apply gravity manually with scale since Rigidbody lacks a gravityScale property.
            // ForceMode.Acceleration ignores mass, matching how gravity naturally works.
            rb.AddForce(UnityEngine.Physics.gravity * gravityScale, ForceMode.Acceleration);
        }

        // Public methods
        public void AddForce(Vector3 force, bool impulse = false)
        {
            ForceMode mode = impulse ? ForceMode.Impulse : ForceMode.Force;
            rb.AddForce(force, mode);
        }

        public bool CheckGrounded(Vector3 origin, float distance, LayerMask groundLayer)
        {
            return UnityEngine.Physics.SphereCast(
                origin, sphereCastRadius, Vector3.down, out _, distance, groundLayer
            );
        }
    }
}
