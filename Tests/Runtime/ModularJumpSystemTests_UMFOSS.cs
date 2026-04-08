using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GameplayMechanicsUMFOSS.Physics;
using GameplayMechanicsUMFOSS.Movement;

namespace GameplayMechanicsUMFOSS.Tests.Movement
{
    public class ModularJumpSystemTests
    {
        private GameObject playerGo;
        private GameObject groundGo;
        private ModularJumpSystem jumpSystem;
        private Physics2DAdapter adapter;

        private const int GROUND_LAYER = 6;

        private void CreateTestScene()
        {
            // Create ground
            groundGo = new GameObject("Ground");
            groundGo.layer = GROUND_LAYER;
            var groundCollider = groundGo.AddComponent<BoxCollider2D>();
            groundCollider.size = new Vector2(20f, 1f);
            groundGo.transform.position = new Vector3(0f, -1f, 0f);

            // Create player
            playerGo = new GameObject("Player");
            var rb = playerGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 1f;

            var collider = playerGo.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1f);

            adapter = playerGo.AddComponent<Physics2DAdapter>();

            jumpSystem = playerGo.AddComponent<ModularJumpSystem>();

            playerGo.transform.position = new Vector3(0f, 1f, 0f);
        }

        private void DestroyTestScene()
        {
            if (playerGo != null) Object.Destroy(playerGo);
            if (groundGo != null) Object.Destroy(groundGo);
        }

        [UnityTest]
        public IEnumerator Jump_AppliesUpwardVelocity()
        {
            CreateTestScene();

            // Wait for physics to settle and player to land
            yield return new WaitForSeconds(0.5f);

            jumpSystem.TriggerJump();

            yield return new WaitForFixedUpdate();

            Assert.Greater(
                playerGo.GetComponent<Rigidbody2D>().linearVelocity.y, 0f,
                "Player should have positive Y velocity after jump"
            );

            DestroyTestScene();
        }

        [UnityTest]
        public IEnumerator Jump_DecreasesJumpsRemaining()
        {
            CreateTestScene();
            yield return new WaitForSeconds(0.5f);

            int jumpsBefore = jumpSystem.JumpsRemaining;
            jumpSystem.TriggerJump();

            yield return new WaitForFixedUpdate();

            Assert.AreEqual(
                jumpsBefore - 1, jumpSystem.JumpsRemaining,
                "JumpsRemaining should decrease by 1 after a jump"
            );

            DestroyTestScene();
        }

        [UnityTest]
        public IEnumerator OnJumpStartEvent_FiresOnJump()
        {
            CreateTestScene();
            yield return new WaitForSeconds(0.5f);

            bool eventFired = false;
            jumpSystem.OnJumpStart += () => eventFired = true;

            jumpSystem.TriggerJump();

            yield return new WaitForFixedUpdate();

            Assert.IsTrue(eventFired, "OnJumpStart event should fire when jump is executed");

            DestroyTestScene();
        }

        [UnityTest]
        public IEnumerator OnJumpEndEvent_FiresOnLand()
        {
            CreateTestScene();
            yield return new WaitForSeconds(0.5f);

            bool eventFired = false;
            jumpSystem.OnJumpEnd += () => eventFired = true;

            jumpSystem.TriggerJump();

            // Wait for jump to complete and player to land
            yield return new WaitForSeconds(2f);

            Assert.IsTrue(eventFired, "OnJumpEnd event should fire when player lands");

            DestroyTestScene();
        }

        [UnityTest]
        public IEnumerator State_IsRisingAfterJump()
        {
            CreateTestScene();
            yield return new WaitForSeconds(0.5f);

            jumpSystem.TriggerJump();

            yield return new WaitForFixedUpdate();
            yield return null; // One Update frame for state to update

            Assert.AreEqual(
                JumpState.Rising, jumpSystem.CurrentState,
                "State should be Rising immediately after jump"
            );

            DestroyTestScene();
        }

        [UnityTest]
        public IEnumerator JumpsResetOnLanding()
        {
            CreateTestScene();
            yield return new WaitForSeconds(0.5f);

            jumpSystem.TriggerJump();

            // Wait for landing
            yield return new WaitForSeconds(2f);

            Assert.AreEqual(
                2, jumpSystem.JumpsRemaining,
                "JumpsRemaining should reset to maxJumps on landing"
            );

            DestroyTestScene();
        }
    }
}
