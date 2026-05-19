using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Combat;
using GameplayMechanicsUMFOSS.UI;
using GameplayMechanicsUMFOSS.Utils;
using NUnit.Framework;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Tests.Editor
{
    public class FloatingDamageNumbersEditorTests
    {
        private sealed class PoolableStub : MonoBehaviour, IPoolable
        {
            public int spawnCount;
            public int returnCount;

            public void OnSpawnFromPool()
            {
                spawnCount++;
                gameObject.SetActive(true);
            }

            public void OnReturnToPool()
            {
                returnCount++;
                gameObject.SetActive(false);
            }
        }

        [SetUp]
        public void SetUp()
        {
            global::EventBus.Clear();
            ClearPoolManager();
        }

        [TearDown]
        public void TearDown()
        {
            global::EventBus.Clear();
            ClearPoolManager();
        }

        [Test]
        public void EventBus_PublishesAndStopsAfterUnsubscribe()
        {
            int callCount = 0;
            System.Action<DamageTakenEvent> listener = _ => callCount++;

            global::EventBus.Subscribe(listener);
            global::EventBus.Publish(new DamageTakenEvent(null, 10f, DamagePresentation.Damage));
            global::EventBus.Unsubscribe(listener);
            global::EventBus.Publish(new DamageTakenEvent(null, 10f, DamagePresentation.Damage));

            Assert.AreEqual(1, callCount);
        }

        [Test]
        public void Config_DefaultFactoryReturnsStylesAndAnimationsForAllTypes()
        {
            FloatingNumberConfig_UMFOSS config = FloatingNumberConfig_UMFOSS.CreateDefault();

            foreach (NumberType numberType in System.Enum.GetValues(typeof(NumberType)))
            {
                Assert.NotNull(config.GetStyle(numberType));
                Assert.NotNull(config.GetAnimation(numberType));
                Assert.NotNull(config.GetAnimation(numberType).movementCurve);
                Assert.NotNull(config.GetAnimation(numberType).fadeCurve);
            }
        }

        [Test]
        public void Formatter_HandlesCritMissAndDecimals()
        {
            Assert.AreEqual("MISS", FloatingNumberFormatter_UMFOSS.Format(100f, NumberType.Miss, 0));
            Assert.AreEqual("81!", FloatingNumberFormatter_UMFOSS.Format(80.6f, NumberType.CriticalHit, 0));
            Assert.AreEqual("12.3", FloatingNumberFormatter_UMFOSS.Format(12.34f, NumberType.Heal, 1));
            Assert.AreEqual("+40 XP", FloatingNumberFormatter_UMFOSS.Format(40f, NumberType.Experience, 0));
        }

        [Test]
        public void CombineBuffer_MergesByTargetAndTypeOnly()
        {
            FloatingNumberCombineBuffer_UMFOSS buffer = new FloatingNumberCombineBuffer_UMFOSS();
            List<FloatingNumberCombineBuffer_UMFOSS.CombinedNumber> results = new List<FloatingNumberCombineBuffer_UMFOSS.CombinedNumber>();

            buffer.Add(1, NumberType.Damage, 10f, Vector3.one, 0f, 0.1f);
            buffer.Add(1, NumberType.Damage, 5f, Vector3.one * 2f, 0.05f, 0.1f);
            buffer.Add(1, NumberType.Heal, 3f, Vector3.up, 0.05f, 0.1f);
            buffer.Add(2, NumberType.Damage, 7f, Vector3.right, 0.05f, 0.1f);

            buffer.CollectExpired(0.16f, results);

            Assert.AreEqual(3, results.Count);
            Assert.AreEqual(15f, FindAmount(results, 1, NumberType.Damage));
            Assert.AreEqual(3f, FindAmount(results, 1, NumberType.Heal));
            Assert.AreEqual(7f, FindAmount(results, 2, NumberType.Damage));
        }

        [Test]
        public void PoolManager_PrewarmsAndReusesWithoutGrowingAfterExhaustion()
        {
            GameObject managerObject = new GameObject("PoolManager_Test");
            ObjectPoolManager_UMFOSS poolManager = managerObject.AddComponent<ObjectPoolManager_UMFOSS>();

            GameObject prefab = new GameObject("PoolablePrefab_Test");
            prefab.AddComponent<PoolableStub>();

            poolManager.Prewarm(prefab, 2);
            Assert.AreEqual(2, poolManager.GetStats(prefab).TotalCount);

            GameObject first = poolManager.Get(prefab);
            GameObject second = poolManager.Get(prefab);
            GameObject third = poolManager.Get(prefab);

            ObjectPoolManager_UMFOSS.PoolStats stats = poolManager.GetStats(prefab);
            Assert.AreEqual(2, stats.TotalCount);
            Assert.AreEqual(2, stats.ActiveCount);

            Assert.IsTrue(first == third || second == third);

            poolManager.ClearAllPoolsForTests();
            Object.DestroyImmediate(managerObject);
            Object.DestroyImmediate(prefab);
        }

        private static float FindAmount(List<FloatingNumberCombineBuffer_UMFOSS.CombinedNumber> results, int targetId, NumberType type)
        {
            for (int index = 0; index < results.Count; index++)
            {
                if (results[index].TargetInstanceId == targetId && results[index].Type == type)
                {
                    return results[index].Amount;
                }
            }

            return float.MinValue;
        }

        private static void ClearPoolManager()
        {
            ObjectPoolManager_UMFOSS existingManager = Object.FindObjectOfType<ObjectPoolManager_UMFOSS>();
            if (existingManager == null)
            {
                return;
            }

            existingManager.ClearAllPoolsForTests();
            Object.DestroyImmediate(existingManager.gameObject);
        }
    }
}
