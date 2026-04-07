using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// Simple oscillating platform for the RecallDemo scene.
    /// Moves back and forth between two points to test weapon embedding on moving objects.
    /// </summary>
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 moveDirection = Vector3.right;
        [SerializeField] private float moveDistance = 5f;
        [SerializeField] private float moveSpeed = 2f;

        private Vector3 startPosition;

        private void Start()
        {
            startPosition = transform.position;
        }

        private void Update()
        {
            float offset = Mathf.PingPong(Time.time * moveSpeed, moveDistance) - (moveDistance * 0.5f);
            transform.position = startPosition + moveDirection.normalized * offset;
        }
    }
}
