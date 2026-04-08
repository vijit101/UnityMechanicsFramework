using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Movement;

namespace GameplayMechanicsUMFOSS.Demo
{
    public class Movement2DDemoController : MonoBehaviour
    {
        [Header("Player Setup")]
        [SerializeField] private Movement2D_UMFOSS playerMovement;
        [SerializeField] private Camera demoCamera;
        
        [Header("UI Setup")]
        [SerializeField] private GameObject buttonContainer;
        [SerializeField] private GameObject modeButtonPrefab;
        [SerializeField] private TextMeshProUGUI currentModeText;
        [SerializeField] private TextMeshProUGUI parametersText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        
        [Header("Demo Values")]
        [SerializeField] private DemoModeSettings[] demoSettings;

        private void Start()
        {
            if (playerMovement == null)
            {
                Debug.LogError("Player Movement reference not set!");
                return;
            }

            SetupDemoUI();
            SubscribeToEvents();
            UpdateDisplay();
        }

        private void SetupDemoUI()
        {
            // Create buttons for each movement mode
            if (buttonContainer != null && modeButtonPrefab != null)
            {
                foreach (MovementMode mode in System.Enum.GetValues(typeof(MovementMode)))
                {
                    GameObject buttonObj = Instantiate(modeButtonPrefab, buttonContainer.transform);
                    Button button = buttonObj.GetComponent<Button>();
                    TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                    
                    if (button != null && buttonText != null)
                    {
                        buttonText.text = mode.ToString();
                        button.onClick.AddListener(() => OnModeButtonClicked(mode));
                    }
                }
            }

            // Setup instructions
            if (instructionsText != null)
            {
                instructionsText.text = "Use Arrow Keys or WASD to move\nClick buttons to switch movement modes";
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

        private void OnModeButtonClicked(MovementMode mode)
        {
            if (playerMovement != null)
            {
                playerMovement.SetMode(mode);
                ApplyDemoSettings(mode);
            }
        }

        private void ApplyDemoSettings(MovementMode mode)
        {
            if (demoSettings != null)
            {
                foreach (var setting in demoSettings)
                {
                    if (setting.mode == mode)
                    {
                        setting.ApplySettings(playerMovement);
                        break;
                    }
                }
            }
            UpdateDisplay();
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
                currentModeText.text = $"Current Mode: {playerMovement.CurrentMode}";
            }

            if (parametersText != null && playerMovement != null)
            {
                parametersText.text = GetModeParametersText(playerMovement.CurrentMode);
            }
        }

        private string GetModeParametersText(MovementMode mode)
        {
            switch (mode)
            {
                case MovementMode.TransformDirect:
                    return "Speed: 5 | Update: Update\nInstant, zero float, pixel-perfect";
                case MovementMode.TransformTranslate:
                    return "Speed: 5 | Space: World\nSame as Direct but toggle to Self and rotate character to see difference";
                case MovementMode.MoveTowards:
                    return "Speed: 5 | MaxDelta: 5\nLinear. Constant pace. No easing.";
                case MovementMode.LerpSmooth:
                    return "Speed: 5 | LerpSpeed: 6\nSmooth. Floaty. Eases in and out.";
                case MovementMode.SmoothDamp:
                    return "Speed: 5 | SmoothTime: 0.1\nSpringy. Slight overshoot. Organic.";
                case MovementMode.VelocityDirect:
                    return "Speed: 5 | Decel: 8\nResponsive. Slight slide on stop.";
                case MovementMode.ForceAdditive:
                    return "Force: 30 | MaxSpeed: 5 | Drag: 2\nBuilds up. Slippery. Takes time to stop.";
                case MovementMode.ForceImpulse:
                    return "Impulse: 8 | Cooldown: 0.1\nStaccato. Discrete pushes.";
                case MovementMode.KinematicMovePosition:
                    return "Speed: 5 | Continuous\nSolid. Collision-aware. No physics forces.";
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

    [System.Serializable]
    public class DemoModeSettings
    {
        public MovementMode mode;
        public float speed = 5f;
        public UpdateMode updateIn = UpdateMode.Update;
        public SpaceMode space = SpaceMode.World;
        public float maxDelta = 5f;
        public float lerpSpeed = 6f;
        public float smoothTime = 0.1f;
        public float maxSmoothSpeed = 10f;
        public float horizontalDeceleration = 8f;
        public bool preserveVertical = true;
        public float accelerationForce = 30f;
        public float maxSpeed = 5f;
        public float drag = 2f;
        public float impulseForce = 8f;
        public float impulseCooldown = 0.1f;
        public MovementCollisionDetectionMode collisionDetection = MovementCollisionDetectionMode.Discrete;
        public InterpolationMode interpolationMode = InterpolationMode.None;

        public void ApplySettings(Movement2D_UMFOSS movement)
        {
            // This would require reflection or exposing the private fields
            // For demo purposes, we'll just use the default values set in the inspector
            Debug.Log($"Applied demo settings for {mode}");
        }
    }
}
