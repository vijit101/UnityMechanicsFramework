using UnityEngine;

namespace GameplayMechanicsUMFOSS.Movement.Dynamic.RigidbodyOptions {
  public class RigidbodyOption2D : RigidbodyOption {
    public Rigidbody2D rb;

    void Start() {
      rb ??= GetComponent<Rigidbody2D>();
    }

    public override void SyncVelocity() {
      if (rb == null)
        return;
      
      Velocity.X = rb.linearVelocity.x;
      Velocity.Y = rb.linearVelocity.y;
    }

    public override void ApplyVelocity() {
      if (rb == null) return;    
      rb.linearVelocity = new Vector2(Velocity.X, Velocity.Y);
    }
  }
}
