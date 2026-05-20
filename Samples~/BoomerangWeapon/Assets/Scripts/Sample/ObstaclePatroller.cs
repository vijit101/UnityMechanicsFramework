using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// Moves a kinematic object back and forth between two world-space points.
    /// Uses Rigidbody.MovePosition in FixedUpdate for proper physics sync,
    /// so embedded (parented) weapons track smoothly and collisions are detected correctly.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ObstaclePatroller : MonoBehaviour
    {
        [SerializeField] private Vector3 pointA;
        [SerializeField] private Vector3 pointB;
        [SerializeField] private float speed = 1.5f;
        [SerializeField] private float pauseAtEnds = 0.5f;

        private Rigidbody rb;
        private float pauseTimer;
        private bool movingToB = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (pauseTimer > 0f)
            {
                pauseTimer -= Time.fixedDeltaTime;
                return;
            }

            Vector3 target = movingToB ? pointB : pointA;
            Vector3 newPos = Vector3.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);

            if (Vector3.Distance(newPos, target) < 0.01f)
            {
                movingToB = !movingToB;
                pauseTimer = pauseAtEnds;
            }
        }
    }
}
