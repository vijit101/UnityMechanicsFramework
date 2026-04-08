using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Movement;

namespace GameplayMechanicsUMFOSS.Demo
{
    public class SimpleMovementDemo : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private Movement2D_UMFOSS playerMovement;
        
        [Header("UI Setup")]
        [SerializeField] private TextMeshProUGUI currentModeText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        
        private void Start()
        {
            if (playerMovement == null)
            {
                Debug.LogError("Player Movement reference not set!");
                return;
            }

            SetupUI();
            SubscribeToEvents();
            UpdateDisplay();
        }

        private void SetupUI()
        {
            if (instructionsText != null)
            {
                instructionsText.text = "Use Arrow Keys or WASD to move\n" +
                                      "Press 1-9 to switch movement modes\n" +
                                      "Or change mode in Inspector";
            }
        }

        private void SubscribeToEvents()
        {
            if (playerMovement != null)
            {
                playerMovement.OnModeChanged += OnModeChanged;
                playerMovement.OnMovementStarted += OnMovementStarted;
                playerMovement.OnMovementStopped += OnMovementStopped;
                playerMovement.OnDirectionChanged += OnDirectionChanged;
            }
        }

        private void Update()
        {
            HandleModeSwitching();
        }

        private void HandleModeSwitching()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                playerMovement.SetMode(MovementMode.TransformDirect);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                playerMovement.SetMode(MovementMode.TransformTranslate);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                playerMovement.SetMode(MovementMode.MoveTowards);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                playerMovement.SetMode(MovementMode.LerpSmooth);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                playerMovement.SetMode(MovementMode.SmoothDamp);
            else if (Input.GetKeyDown(KeyCode.Alpha6))
                playerMovement.SetMode(MovementMode.VelocityDirect);
            else if (Input.GetKeyDown(KeyCode.Alpha7))
                playerMovement.SetMode(MovementMode.ForceAdditive);
            else if (Input.GetKeyDown(KeyCode.Alpha8))
                playerMovement.SetMode(MovementMode.ForceImpulse);
            else if (Input.GetKeyDown(KeyCode.Alpha9))
                playerMovement.SetMode(MovementMode.KinematicMovePosition);
        }

        private void OnModeChanged(MovementMode previousMode, MovementMode newMode)
        {
            Debug.Log($"Movement mode changed from {previousMode} to {newMode}");
            UpdateDisplay();
        }

        private void OnMovementStarted(MovementMode mode)
        {
            // Could add visual feedback here
        }

        private void OnMovementStopped(MovementMode mode)
        {
            // Could add visual feedback here
        }

        private void OnDirectionChanged(Vector2 newDirection)
        {
            // Could add visual feedback here
        }

        private void UpdateDisplay()
        {
            if (currentModeText != null && playerMovement != null)
            {
                currentModeText.text = $"Current Mode: {playerMovement.CurrentMode}\n" +
                                      $"Direction: {playerMovement.CurrentDirection}\n" +
                                      $"Is Moving: {playerMovement.IsMoving}\n\n" +
                                      GetModeDescription(playerMovement.CurrentMode);
            }
        }

        private string GetModeDescription(MovementMode mode)
        {
            switch (mode)
            {
                case MovementMode.TransformDirect:
                    return "TransformDirect\nInstant, zero float, pixel-perfect";
                case MovementMode.TransformTranslate:
                    return "TransformTranslate\nSame as Direct, try rotating object";
                case MovementMode.MoveTowards:
                    return "MoveTowards\nLinear, constant pace, no easing";
                case MovementMode.LerpSmooth:
                    return "LerpSmooth\nSmooth, floaty, eases in/out";
                case MovementMode.SmoothDamp:
                    return "SmoothDamp\nSpringy, slight overshoot, organic";
                case MovementMode.VelocityDirect:
                    return "VelocityDirect\nResponsive, slight slide on stop";
                case MovementMode.ForceAdditive:
                    return "ForceAdditive\nBuilds up, slippery, takes time to stop";
                case MovementMode.ForceImpulse:
                    return "ForceImpulse\nStaccato, discrete pushes";
                case MovementMode.KinematicMovePosition:
                    return "KinematicMovePosition\nSolid, collision-aware, no physics forces";
                default:
                    return "";
            }
        }

        private void OnDestroy()
        {
            if (playerMovement != null)
            {
                playerMovement.OnModeChanged -= OnModeChanged;
                playerMovement.OnMovementStarted -= OnMovementStarted;
                playerMovement.OnMovementStopped -= OnMovementStopped;
                playerMovement.OnDirectionChanged -= OnDirectionChanged;
            }
        }
    }
}
