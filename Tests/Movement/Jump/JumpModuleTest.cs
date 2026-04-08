using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using GameplayMechanicsUMFOSS.Movement;

public class ModularJumpSystemTests
{
    [UnityTest]
    public IEnumerator Jump_AppliesForce_WhenGrounded()
    {
        // Arrange
        var go = new GameObject();
        var rb = go.AddComponent<Rigidbody2D>();
        var jumpSystem = go.AddComponent<ModularJumpSystem>();

        yield return new WaitForFixedUpdate();

        // Act
        jumpSystem.TriggerJump();

        yield return new WaitForFixedUpdate();

        // Assert
        Assert.Greater(rb.velocity.y, 0f, "Rigidbody2D should move up after jump");

        Object.Destroy(go);
    }
}