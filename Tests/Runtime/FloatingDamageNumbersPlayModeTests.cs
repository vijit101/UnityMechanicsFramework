using System.Collections;
using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Combat;
using GameplayMechanicsUMFOSS.UI;
using GameplayMechanicsUMFOSS.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameplayMechanicsUMFOSS.Tests.Runtime
{
    public class FloatingDamageNumbersPlayModeTests
    {
        private readonly List<GameObject> createdObjects = new List<GameObject>();

        [SetUp]
        public void SetUp()
        {
            global::EventBus.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            global::EventBus.Clear();

            ObjectPoolManager_UMFOSS poolManager = Object.FindObjectOfType<ObjectPoolManager_UMFOSS>();
            if (poolManager != null)
            {
                poolManager.ClearAllPoolsForTests();
                Object.DestroyImmediate(poolManager.gameObject);
            }

            for (int index = 0; index < createdObjects.Count; index++)
            {
                if (createdObjects[index] != null)
                {
                    Object.Destroy(createdObjects[index]);
                }
            }

            createdObjects.Clear();
        }

        [UnityTest]
        public IEnumerator DamageEvent_SpawnsOneFloatingNumber()
        {
            List<FloatingNumberSpawnedEvent> spawnedEvents = new List<FloatingNumberSpawnedEvent>();
            global::EventBus.Subscribe<FloatingNumberSpawnedEvent>(spawnedEvents.Add);

            CreateMainCamera();
            GameObject managerObject = CreateTrackedObject("FloatingDamageNumbersManager_Test");
            managerObject.AddComponent<FloatingDamageNumbers_UMFOSS>();

            yield return null;

            GameObject targetObject = CreateTrackedObject("Target_Test");
            HealthSystem_UMFOSS healthSystem = targetObject.AddComponent<HealthSystem_UMFOSS>();

            healthSystem.ApplyDamage(15f, DamagePresentation.Damage);
            yield return null;

            Assert.AreEqual(1, spawnedEvents.Count);
            Assert.AreEqual(NumberType.Damage, spawnedEvents[0].Type);
            Assert.AreEqual(15f, spawnedEvents[0].Amount);
        }

        [UnityTest]
        public IEnumerator CombineRapid_EmitsOneCombinedPopup()
        {
            List<FloatingNumberSpawnedEvent> spawnedEvents = new List<FloatingNumberSpawnedEvent>();
            global::EventBus.Subscribe<FloatingNumberSpawnedEvent>(spawnedEvents.Add);

            CreateMainCamera();
            GameObject managerObject = CreateTrackedObject("FloatingDamageNumbersManager_Combine_Test");
            FloatingDamageNumbers_UMFOSS manager = managerObject.AddComponent<FloatingDamageNumbers_UMFOSS>();

            yield return null;

            manager.CombineRapid = true;
            manager.RapidWindow = 0.05f;

            GameObject targetObject = CreateTrackedObject("Target_Combine_Test");
            HealthSystem_UMFOSS healthSystem = targetObject.AddComponent<HealthSystem_UMFOSS>();

            for (int hitIndex = 0; hitIndex < 5; hitIndex++)
            {
                healthSystem.ApplyDamage(10f, DamagePresentation.Damage);
            }

            yield return new WaitForSecondsRealtime(0.08f);
            yield return null;

            Assert.AreEqual(1, spawnedEvents.Count);
            Assert.AreEqual(NumberType.Damage, spawnedEvents[0].Type);
            Assert.AreEqual(50f, spawnedEvents[0].Amount);
        }

        private void CreateMainCamera()
        {
            GameObject cameraObject = CreateTrackedObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.AddComponent<Camera>();
            cameraObject.transform.position = new Vector3(0f, 2f, -10f);
            cameraObject.transform.rotation = Quaternion.identity;
        }

        private GameObject CreateTrackedObject(string name)
        {
            GameObject gameObject = new GameObject(name);
            createdObjects.Add(gameObject);
            return gameObject;
        }
    }
}
