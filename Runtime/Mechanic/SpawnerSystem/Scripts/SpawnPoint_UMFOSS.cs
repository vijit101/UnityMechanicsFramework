using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public class SpawnPoint_UMFOSS : MonoBehaviour
    {
        [Header("Spawn Area")]
        [SerializeField] private SpawnShape shape = SpawnShape.Point;
        [SerializeField] private float radius = 0f;
        [SerializeField] private Vector2 size = Vector2.one;

        public void Configure(SpawnShape shape, float radius = 0f, Vector2 size = default)
        {
            this.shape = shape;
            this.radius = radius;
            this.size = size == default ? Vector2.one : size;
        }

        public Vector3 GetSpawnPosition()
        {
            return shape switch
            {
                SpawnShape.Point => transform.position,
                SpawnShape.Circle => transform.position +
                    (Vector3)(Random.insideUnitCircle * radius),
                SpawnShape.Rectangle => transform.position + new Vector3(
                    Random.Range(-size.x / 2f, size.x / 2f),
                    Random.Range(-size.y / 2f, size.y / 2f), 0f),
                _ => transform.position
            };
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);

            switch (shape)
            {
                case SpawnShape.Point:
                    Gizmos.DrawWireSphere(transform.position, 0.2f);
                    break;
                case SpawnShape.Circle:
                    Gizmos.DrawWireSphere(transform.position, radius);
                    break;
                case SpawnShape.Rectangle:
                    Gizmos.DrawWireCube(transform.position,
                        new Vector3(size.x, size.y, 0.1f));
                    break;
            }
        }
    }
}
