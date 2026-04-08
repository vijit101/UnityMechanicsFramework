using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.PauseSystem
{
    /// <summary>
    /// Rotates a GameObject continuously using Time.deltaTime.
    /// When the game is paused (Time.timeScale = 0), deltaTime becomes 0,
    /// so the rotation naturally freezes — no EventBus subscription needed.
    ///
    /// This demonstrates that physics/transform objects freeze automatically
    /// when timeScale is 0 — no manual pause handling required for simple transforms.
    ///
    /// Setup:
    ///   - Attach to any GameObject (cube, sphere, etc.).
    ///   - Adjust rotationSpeed in the Inspector.
    /// </summary>
    public class DemoRotator : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 45f);

        private void Update()
        {
            // Time.deltaTime is 0 when timeScale is 0, so this stops during pause automatically.
            transform.Rotate(rotationSpeed * Time.deltaTime);
        }
    }
}
