using UnityEngine;

namespace GameplayMechanicsUMFOSS.Movement.Dynamic.PhysicsOptions {
  public abstract class PhysicsOption : MonoBehaviour {
    public abstract bool CheckRadialCollision(float radius, LayerMask collisionLayer);

    void Start() {}
    void Update() {}
  }
}
