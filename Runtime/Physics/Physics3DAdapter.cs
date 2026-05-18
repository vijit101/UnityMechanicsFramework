using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// 3D implementation of IPhysicsAdapter wrapping Unity's Rigidbody.
    /// Attach to any GameObject with a Rigidbody to use physics-agnostic mechanics.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Physics3DAdapter : MonoBehaviour, IPhysicsAdapter
    {
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public Vector3 Velocity
        {
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
        }

        public bool IsKinematic
        {
            get => rb.isKinematic;
            set => rb.isKinematic = value;
        }

        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            rb.AddForce(force, mode);
        }

        public void SetKinematic(bool kinematic)
        {
            if (kinematic)
            {
                if (!rb.isKinematic)
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                rb.isKinematic = true;
            }
            else
            {
                rb.isKinematic = false;
            }
        }
    }
}
