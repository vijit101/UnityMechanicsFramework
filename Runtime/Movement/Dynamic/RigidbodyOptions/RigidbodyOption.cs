using UnityEngine;

namespace GameplayMechanicsUMFOSS.Movement.Dynamic.RigidbodyOptions {
  public abstract class RigidbodyOption : MonoBehaviour {
    public Axes Velocity = new Axes(0f, 0f, 0f);
    public Axes Acceleration = new Axes(0f, 0f, 0f);

    public abstract void SyncVelocity();
    public abstract void ApplyVelocity();

    void UpdateVelocityFromAcceleration() {
      Velocity.X += Acceleration.X * Time.deltaTime;
      Velocity.Y += Acceleration.Y * Time.deltaTime;
      Velocity.Z += Acceleration.Z * Time.deltaTime;
    }

    void Start() {
      SyncVelocity();
    }

    void Update() {}

    void FixedUpdate() {
      SyncVelocity();
      UpdateVelocityFromAcceleration();
      ApplyVelocity();
    }
  }
}
