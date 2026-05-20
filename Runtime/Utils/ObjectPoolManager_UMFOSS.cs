using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// A generic object pool that pre-instantiates GameObjects at startup and
    /// recycles them at runtime, eliminating garbage collection spikes from
    /// repeated Instantiate/Destroy calls. Used by the Afterimage system to
    /// spawn and recycle faded sprite copies with zero runtime allocation.
    /// </summary>
    public class ObjectPoolManager_UMFOSS : MonoBehaviour
    {
        [Header("Pool Configuration")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialPoolSize = 10;

        private readonly Queue<GameObject> availableObjects = new Queue<GameObject>();
        private readonly List<GameObject> allObjects = new List<GameObject>();

        private void Awake()
        {
            // Only pre-warm from Awake if the prefab was set via Inspector.
            // When initialized programmatically via Initialize(), that method
            // handles pre-warming itself — calling PreWarm here with a null
            // prefab would throw NullReferenceException.
            if (prefab != null)
            {
                PreWarm();
            }
        }

        /// <summary>
        /// Pre-instantiates the pool with the configured number of inactive objects.
        /// Called once on Awake to front-load the allocation cost.
        /// </summary>
        private void PreWarm()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject obj = CreateNewObject();
                obj.SetActive(false);
                availableObjects.Enqueue(obj);
            }
        }

        /// <summary>
        /// Retrieve an object from the pool. If the pool is exhausted, a new
        /// object is created and added to the pool automatically.
        /// The returned object is already SetActive(true).
        /// </summary>
        /// <param name="position">World position for the retrieved object.</param>
        /// <param name="rotation">World rotation for the retrieved object.</param>
        /// <returns>An active, positioned GameObject from the pool.</returns>
        public GameObject Get(Vector3 position, Quaternion rotation)
        {
            GameObject obj;

            if (availableObjects.Count > 0)
            {
                obj = availableObjects.Dequeue();
            }
            else
            {
                obj = CreateNewObject();
            }

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Return an object to the pool for reuse. Deactivates the object
        /// and re-enqueues it. Always call this instead of Destroy().
        /// </summary>
        /// <param name="obj">The GameObject to return to the pool.</param>
        public void Return(GameObject obj)
        {
            obj.SetActive(false);
            availableObjects.Enqueue(obj);
        }

        private GameObject CreateNewObject()
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            allObjects.Add(obj);
            return obj;
        }

        /// <summary>
        /// Initializes the pool at runtime with a specific prefab and size.
        /// Use this when the pool needs to be configured programmatically
        /// rather than through the Inspector.
        /// </summary>
        /// <param name="poolPrefab">The prefab to pool.</param>
        /// <param name="size">Number of objects to pre-instantiate.</param>
        public void Initialize(GameObject poolPrefab, int size)
        {
            prefab = poolPrefab;
            initialPoolSize = size;
            PreWarm();
        }
    }
}
