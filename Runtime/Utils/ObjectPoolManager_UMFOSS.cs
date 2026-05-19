using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// Manages reusable GameObject pools and avoids gameplay-time allocations after prewarm.
    /// </summary>
    public class ObjectPoolManager_UMFOSS : MonoBehaviour
    {
        public struct PoolStats
        {
            public PoolStats(int activeCount, int availableCount, int totalCount)
            {
                ActiveCount = activeCount;
                AvailableCount = availableCount;
                TotalCount = totalCount;
            }

            public int ActiveCount { get; }
            public int AvailableCount { get; }
            public int TotalCount { get; }
        }

        private sealed class PoolData
        {
            public GameObject prefab;
            public Transform container;
            public readonly Queue<GameObject> available = new Queue<GameObject>();
            public readonly HashSet<GameObject> availableLookup = new HashSet<GameObject>();
            public readonly LinkedList<GameObject> activeOrder = new LinkedList<GameObject>();
            public readonly Dictionary<GameObject, LinkedListNode<GameObject>> activeLookup = new Dictionary<GameObject, LinkedListNode<GameObject>>();
            public readonly List<GameObject> allInstances = new List<GameObject>();
        }

        private static ObjectPoolManager_UMFOSS instance;

        private readonly Dictionary<int, PoolData> poolsByPrefabId = new Dictionary<int, PoolData>();
        private readonly Dictionary<int, PoolData> poolsByInstanceId = new Dictionary<int, PoolData>();

        public static ObjectPoolManager_UMFOSS Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ObjectPoolManager_UMFOSS>();
                }

                if (instance == null)
                {
                    GameObject managerObject = new GameObject("ObjectPoolManager_UMFOSS");
                    instance = managerObject.AddComponent<ObjectPoolManager_UMFOSS>();
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <summary>
        /// Pre-instantiates a fixed number of pooled instances for a prefab.
        /// </summary>
        public void Prewarm(GameObject prefab, int count, Transform parent = null)
        {
            if (prefab == null || count <= 0)
            {
                return;
            }

            PoolData pool = GetOrCreatePool(prefab, parent);
            while (pool.allInstances.Count < count)
            {
                GameObject instanceObject = CreateInstance(pool);
                ReturnToAvailable(pool, instanceObject, false);
            }
        }

        /// <summary>
        /// Borrows an instance from the pool. When exhausted, the oldest active instance is reused.
        /// </summary>
        public GameObject Get(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                return null;
            }

            PoolData pool = GetOrCreatePool(prefab, parent);
            if (pool.available.Count == 0)
            {
                if (pool.activeOrder.First != null)
                {
                    Return(pool.activeOrder.First.Value);
                }
                else
                {
                    GameObject createdObject = CreateInstance(pool);
                    ReturnToAvailable(pool, createdObject, false);
                }
            }

            GameObject instanceObject = pool.available.Dequeue();
            pool.availableLookup.Remove(instanceObject);

            if (parent != null)
            {
                instanceObject.transform.SetParent(parent, false);
            }
            else if (pool.container != null)
            {
                instanceObject.transform.SetParent(pool.container, false);
            }

            LinkedListNode<GameObject> node = pool.activeOrder.AddLast(instanceObject);
            pool.activeLookup[instanceObject] = node;

            InvokePoolables(instanceObject, true);
            if (!instanceObject.activeSelf)
            {
                instanceObject.SetActive(true);
            }

            return instanceObject;
        }

        /// <summary>
        /// Returns an instance back to its originating pool.
        /// </summary>
        public void Return(GameObject instanceObject)
        {
            if (instanceObject == null)
            {
                return;
            }

            if (!poolsByInstanceId.TryGetValue(instanceObject.GetInstanceID(), out PoolData pool))
            {
                if (instanceObject.activeSelf)
                {
                    instanceObject.SetActive(false);
                }

                return;
            }

            ReturnToAvailable(pool, instanceObject, true);
        }

        /// <summary>
        /// Returns current pool usage for a prefab.
        /// </summary>
        public PoolStats GetStats(GameObject prefab)
        {
            if (prefab == null)
            {
                return default;
            }

            return poolsByPrefabId.TryGetValue(prefab.GetInstanceID(), out PoolData pool)
                ? new PoolStats(pool.activeLookup.Count, pool.available.Count, pool.allInstances.Count)
                : default;
        }

        /// <summary>
        /// Clears and destroys all tracked instances. Intended for tests only.
        /// </summary>
        public void ClearAllPoolsForTests()
        {
            foreach (KeyValuePair<int, PoolData> entry in poolsByPrefabId)
            {
                PoolData pool = entry.Value;
                foreach (GameObject instanceObject in pool.allInstances)
                {
                    if (instanceObject != null)
                    {
                        DestroyImmediate(instanceObject);
                    }
                }

                if (pool.container != null)
                {
                    DestroyImmediate(pool.container.gameObject);
                }
            }

            poolsByPrefabId.Clear();
            poolsByInstanceId.Clear();
        }

        private PoolData GetOrCreatePool(GameObject prefab, Transform parent)
        {
            int prefabId = prefab.GetInstanceID();
            if (poolsByPrefabId.TryGetValue(prefabId, out PoolData existingPool))
            {
                return existingPool;
            }

            GameObject containerObject = new GameObject(prefab.name + "_Pool");
            if (parent != null)
            {
                containerObject.transform.SetParent(parent, false);
            }
            else
            {
                containerObject.transform.SetParent(transform, false);
            }

            PoolData pool = new PoolData
            {
                prefab = prefab,
                container = containerObject.transform
            };

            poolsByPrefabId[prefabId] = pool;
            return pool;
        }

        private GameObject CreateInstance(PoolData pool)
        {
            GameObject instanceObject = Instantiate(pool.prefab, pool.container);
            instanceObject.name = pool.prefab.name;
            instanceObject.SetActive(false);
            pool.allInstances.Add(instanceObject);
            poolsByInstanceId[instanceObject.GetInstanceID()] = pool;
            return instanceObject;
        }

        private void ReturnToAvailable(PoolData pool, GameObject instanceObject, bool invokePoolable)
        {
            if (pool.availableLookup.Contains(instanceObject))
            {
                return;
            }

            if (pool.activeLookup.TryGetValue(instanceObject, out LinkedListNode<GameObject> activeNode))
            {
                pool.activeLookup.Remove(instanceObject);
                pool.activeOrder.Remove(activeNode);
            }

            if (invokePoolable)
            {
                InvokePoolables(instanceObject, false);
            }

            if (instanceObject.activeSelf)
            {
                instanceObject.SetActive(false);
            }

            if (pool.container != null)
            {
                instanceObject.transform.SetParent(pool.container, false);
            }

            pool.available.Enqueue(instanceObject);
            pool.availableLookup.Add(instanceObject);
        }

        private static void InvokePoolables(GameObject instanceObject, bool isSpawn)
        {
            MonoBehaviour[] components = instanceObject.GetComponents<MonoBehaviour>();
            for (int index = 0; index < components.Length; index++)
            {
                if (!(components[index] is IPoolable poolable))
                {
                    continue;
                }

                if (isSpawn)
                {
                    poolable.OnSpawnFromPool();
                }
                else
                {
                    poolable.OnReturnToPool();
                }
            }
        }
    }
}
