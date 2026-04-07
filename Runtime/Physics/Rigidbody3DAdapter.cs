using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>IPhysicsAdapter implementation wrapping Unity's 3D Rigidbody.</summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Rigidbody3DAdapter : MonoBehaviour, IPhysicsAdapter
    {
        [SerializeField] private Rigidbody rb;

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

        private void Awake()
        {
            if (rb == null)
            {
                rb = GetComponent<Rigidbody>();
            }
        }

        public void AddForce(Vector3 force, bool isImpulse = false)
        {
            rb.AddForce(force, isImpulse ? ForceMode.Impulse : ForceMode.Force);
        }

        public void SetPosition(Vector3 position)
        {
            rb.position = position;
        }

        public void MovePosition(Vector3 position)
        {
            rb.MovePosition(position);
        }

        public void ClearForces()
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}
