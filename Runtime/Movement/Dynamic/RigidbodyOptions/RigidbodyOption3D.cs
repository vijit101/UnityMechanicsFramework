using UnityEngine;

namespace GameplayMechanicsUMFOSS.Movement.Dynamic.RigidbodyOptions {
  public class RigidbodyOption3D : RigidbodyOption {
    public Rigidbody rb;

    void Start() {
      rb ??= GetComponent<Rigidbody>();
    }

    public override void SyncVelocity() {
      if (rb == null)
        return;

      Velocity.X = rb.linearVelocity.x;
      Velocity.Y = rb.linearVelocity.y;
      Velocity.Z = rb.linearVelocity.z;
    }

    public override void ApplyVelocity() {
      if (rb == null) return;
      rb.linearVelocity = new Vector3(Velocity.X, Velocity.Y, Velocity.Z);
    }
  }
}
