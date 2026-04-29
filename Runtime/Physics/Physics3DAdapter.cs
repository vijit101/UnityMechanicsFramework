using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    [RequireComponent(typeof(Rigidbody))]
    public class Physics3DAdapter : MonoBehaviour, IPhysicsAdapter
    {
        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            if (rb == null)
                Debug.LogError("Rigidbody missing on 3D object!");
        }

        public void SetVelocity(Vector3 velocity)
        {
            if (rb == null) return;
            rb.velocity = velocity;
        }
    }
}