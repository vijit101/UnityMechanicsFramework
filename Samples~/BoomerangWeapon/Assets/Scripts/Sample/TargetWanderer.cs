using UnityEngine;
using GameplayMechanicsUMFOSS.Combat;

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// Moves a target in a straight line in a random 3D direction (x, y, z).
    /// After a random interval, picks a new random direction. Clamps position
    /// within a bounding box around the spawn point so it doesn't fly off.
    /// Stops on death, resumes on respawn.
    /// </summary>
    [RequireComponent(typeof(TargetDummy))]
    [RequireComponent(typeof(Damageable))]
    public class TargetWanderer : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 1.5f;
        [SerializeField] private float directionChangeMin = 1f;
        [SerializeField] private float directionChangeMax = 3f;
        [SerializeField] private Vector3 boundsExtents = new Vector3(4f, 2f, 4f);

        private Vector3 spawnPosition;
        private Vector3 moveDirection;
        private float directionTimer;
        private bool isMoving = true;
        private Damageable damageable;
        private TargetDummy targetDummy;

        private void Awake()
        {
            spawnPosition = transform.position;
            damageable = GetComponent<Damageable>();
            targetDummy = GetComponent<TargetDummy>();
            PickNewDirection();
        }

        private void OnEnable()
        {
            damageable.OnDeath += HandleDeath;
            targetDummy.OnRespawned += HandleRespawn;
        }

        private void OnDisable()
        {
            damageable.OnDeath -= HandleDeath;
            targetDummy.OnRespawned -= HandleRespawn;
        }

        private void Update()
        {
            if (!isMoving) return;

            directionTimer -= Time.deltaTime;
            if (directionTimer <= 0f)
            {
                PickNewDirection();
            }

            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Clamp within bounds and bounce off edges
            Vector3 offset = transform.position - spawnPosition;
            if (Mathf.Abs(offset.x) > boundsExtents.x)
            {
                moveDirection.x = -moveDirection.x;
                offset.x = Mathf.Clamp(offset.x, -boundsExtents.x, boundsExtents.x);
            }
            if (Mathf.Abs(offset.y) > boundsExtents.y)
            {
                moveDirection.y = -moveDirection.y;
                offset.y = Mathf.Clamp(offset.y, -boundsExtents.y, boundsExtents.y);
            }
            if (Mathf.Abs(offset.z) > boundsExtents.z)
            {
                moveDirection.z = -moveDirection.z;
                offset.z = Mathf.Clamp(offset.z, -boundsExtents.z, boundsExtents.z);
            }
            transform.position = spawnPosition + offset;
        }

        private void PickNewDirection()
        {
            moveDirection = Random.onUnitSphere;
            directionTimer = Random.Range(directionChangeMin, directionChangeMax);
        }

        private void HandleDeath(Damageable dead)
        {
            isMoving = false;
        }

        private void HandleRespawn()
        {
            spawnPosition = transform.position;
            isMoving = true;
            PickNewDirection();
        }
    }
}
