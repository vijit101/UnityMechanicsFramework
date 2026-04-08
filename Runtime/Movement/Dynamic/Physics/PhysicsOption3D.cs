using UnityEngine;

namespace GameplayMechanicsUMFOSS.Movement.Dynamic.PhysicsOptions {
  public class PhysicsOption3D : PhysicsOption {
    public override bool CheckRadialCollision(float radius, LayerMask collisionLayer) {
      return Physics.CheckSphere(transform.position, radius, collisionLayer);
    }
  }
}
