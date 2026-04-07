using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Core;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// Prefab-keyed object pools. Use <see cref="Get"/> / <see cref="Release"/> instead of Instantiate/Destroy at runtime.
    /// </summary>
    public sealed class ObjectPoolManager_UMFOSS : MonoSingletonGeneric<ObjectPoolManager_UMFOSS>
    {
        readonly Dictionary<int, Queue<GameObject>> _freeByPrefabId = new Dictionary<int, Queue<GameObject>>();
        readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new Dictionary<GameObject, GameObject>();

        /// <summary>Warms the pool by creating inactive instances up to <paramref name="count"/>.</summary>
        public void WarmPool(GameObject prefab, int count)
        {
            if (prefab == null) return;
            var id = prefab.GetInstanceID();
            if (!_freeByPrefabId.TryGetValue(id, out var q))
            {
                q = new Queue<GameObject>();
                _freeByPrefabId[id] = q;
            }

            for (var i = q.Count; i < count; i++)
            {
                var inst = Instantiate(prefab);
                inst.SetActive(false);
                _instanceToPrefab[inst] = prefab;
                q.Enqueue(inst);
            }
        }

        /// <summary>Obtains an instance, activating it and placing it at the given pose.</summary>
        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            var id = prefab.GetInstanceID();
            if (!_freeByPrefabId.TryGetValue(id, out var q))
            {
                q = new Queue<GameObject>();
                _freeByPrefabId[id] = q;
            }

            GameObject inst;
            if (q.Count > 0)
                inst = q.Dequeue();
            else
            {
                inst = Instantiate(prefab);
                _instanceToPrefab[inst] = prefab;
            }

            inst.transform.SetPositionAndRotation(position, rotation);
            inst.SetActive(true);
            return inst;
        }

        /// <summary>Returns an instance to its pool (deactivated).</summary>
        public void Release(GameObject instance)
        {
            if (instance == null) return;
            if (!_instanceToPrefab.TryGetValue(instance, out var prefab))
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            var id = prefab.GetInstanceID();
            if (!_freeByPrefabId.TryGetValue(id, out var q))
            {
                q = new Queue<GameObject>();
                _freeByPrefabId[id] = q;
            }

            q.Enqueue(instance);
        }
    }
}
