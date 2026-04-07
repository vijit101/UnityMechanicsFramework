using UnityEngine;

namespace GameplayMechanicsUMFOSS.World
{
    public class SpawnPoint_UMFOSS : MonoBehaviour
    {
        [Header("Spawn Area")]
        [SerializeField] SpawnShape shape = SpawnShape.Point;
        [SerializeField] float radius = 1f;
        [SerializeField] Vector2 size = Vector2.one;

        public Vector3 GetSpawnPosition()
        {
            return shape switch
            {
                SpawnShape.Point => transform.position,
                SpawnShape.Circle => transform.position + (Vector3)Random.insideUnitCircle * radius,
                SpawnShape.Rectangle => transform.position + new Vector3(
                    Random.Range(-size.x / 2f, size.x / 2f),
                    Random.Range(-size.y / 2f, size.y / 2f),
                    0f),
                _ => transform.position
            };
        }

        void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            var pos = transform.position;
            switch (shape)
            {
                case SpawnShape.Point:
                    Gizmos.DrawSphere(pos, 0.15f);
                    break;
                case SpawnShape.Circle:
                    Gizmos.DrawWireSphere(pos, radius);
                    break;
                case SpawnShape.Rectangle:
                    var half = new Vector3(size.x / 2f, size.y / 2f, 0.1f);
                    Gizmos.DrawCube(pos, half * 2f);
                    break;
            }
        }
    }
}
