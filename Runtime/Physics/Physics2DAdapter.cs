using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Physics2DAdapter : MonoBehaviour, IPhysicsAdapter
    {
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
                Debug.LogError("Rigidbody2D missing on 2D object!");
        }

        public void SetVelocity(Vector3 velocity)
        {
            if (rb == null) return;

            rb.velocity = new Vector2(velocity.x, velocity.y);
        }
    }
}