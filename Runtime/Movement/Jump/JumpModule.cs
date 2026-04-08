using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

using GameplayMechanicsUMFOSS.Movement.Dynamic.RigidbodyOptions;
using GameplayMechanicsUMFOSS.Movement.Dynamic.PhysicsOptions;

namespace GameplayMechanicsUMFOSS.Movement.Jump {
  public class JumpModule : MonoBehaviour {
    public RigidbodyOption MovementData;
    public PhysicsOption PhysicsData;
    public LayerMask CollisionMask;

    public Key JumpKey = Key.Space;
    
    public float CollisionRadius = 1.05f;
    public float GravityAcceleration = -20f;
    public float JumpForce = 10f;
    public int ExtraJumps = 1;

    public UnityEvent OnJumpStart;
    public UnityEvent OnJumpEnd;

    private int _jumpsRemaining;
    private bool _isJumping;

    void PerformJump() {
      if (MovementData == null)
        return;

      _jumpsRemaining--;
      _isJumping = true;

      OnJumpStart.Invoke();
      
      MovementData.Velocity.Y = 0f;
      MovementData.ApplyVelocity();
      MovementData.Velocity.Y += JumpForce;
      MovementData.ApplyVelocity();
    }

    void ResetJumps() {
      if (!_isJumping) return;
      
      _jumpsRemaining = ExtraJumps;
      _isJumping = false;
      
      OnJumpEnd.Invoke();
    }

    bool JumpKeyPressed() {
      if (Keyboard.current == null) return false;
      return Keyboard.current[JumpKey].wasPressedThisFrame;
    }

    bool CanJump() {
      bool jumpsAvailable = _jumpsRemaining > 0;
      return JumpKeyPressed() && jumpsAvailable;
    }

    void AccelerateDownwards() {
      if (MovementData == null) return;
      MovementData.Acceleration.Y = GravityAcceleration;
    }

    bool IsGrounded() {
      if (PhysicsData == null)
        return false;
      
      return PhysicsData.CheckRadialCollision(CollisionRadius, CollisionMask);
    }

    void Start() {
      MovementData ??= GetComponent<RigidbodyOption>();
      PhysicsData ??= GetComponent<PhysicsOption>();
      _jumpsRemaining = ExtraJumps;
    }

    void Update() {
      if (IsGrounded())
        ResetJumps();

      AccelerateDownwards();

      if (CanJump())
        PerformJump();
    }
  }
}
