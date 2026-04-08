using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.PauseSystem
{
    /// <summary>
    /// Demo enemy that moves back and forth between two points.
    /// Subscribes to GamePausedEvent / GameResumedEvent to freeze and unfreeze.
    ///
    /// This demonstrates the EventBus pattern: the PauseSystem never touches this script.
    /// This script independently reacts to the event it cares about.
    ///
    /// Setup:
    ///   - Attach to any GameObject (cube, capsule, sprite, etc.).
    ///   - Set leftPoint and rightPoint to control the patrol range.
    /// </summary>
    public class DemoEnemy : MonoBehaviour
    {
        [Header("Patrol Settings")]
        [SerializeField] private float leftPoint  = -4f;
        [SerializeField] private float rightPoint =  4f;
        [SerializeField] private float moveSpeed  =  3f;

        // ── Private State ────────────────────────────────────────────────────────

        private bool  inputEnabled = true;
        private float direction    = 1f;

        // ── Unity Lifecycle ──────────────────────────────────────────────────────

        private void OnEnable()
        {
            EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
        }

        private void Update()
        {
            if (!inputEnabled) return;

            // Move in current direction
            transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);

            // Flip at boundaries
            float x = transform.position.x;
            if (x >= rightPoint) direction = -1f;
            if (x <= leftPoint)  direction =  1f;
        }

        // ── Event Handlers ───────────────────────────────────────────────────────

        private void OnGamePaused(GamePausedEvent e)
        {
            inputEnabled = false;
        }

        private void OnGameResumed(GameResumedEvent e)
        {
            inputEnabled = true;
        }
    }
}
