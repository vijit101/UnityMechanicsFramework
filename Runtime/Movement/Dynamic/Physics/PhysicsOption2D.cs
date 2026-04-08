using UnityEngine;

namespace GameplayMechanicsUMFOSS.Movement.Dynamic.PhysicsOptions {
  public class PhysicsOption2D : PhysicsOption {
    public override bool CheckRadialCollision(float radius, LayerMask collisionLayer) {
      return Physics2D.OverlapCircle(transform.position, radius, collisionLayer) != null;
    }
  }
}
