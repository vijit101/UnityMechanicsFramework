using UnityEngine;
using System;
using GameplayMechanicsUMFOSS.Physics;

namespace GameplayMechanicsUMFOSS.Movement
{
    public enum MovementMode
    {
        // Transform Group - no Rigidbody required
        TransformDirect,
        TransformTranslate,
        MoveTowards,
        LerpSmooth,
        SmoothDamp,
        
        // Physics Group - requires Rigidbody2D
        VelocityDirect,
        ForceAdditive,
        ForceImpulse,
        KinematicMovePosition
    }

    public enum UpdateMode
    {
        Update,
        FixedUpdate
    }

    public enum SpaceMode
    {
        World,
        Self
    }

    public enum LerpTargetMode
    {
        InputDirection,
        TargetTransform
    }

    public enum MovementCollisionDetectionMode
    {
        Discrete,
        Continuous
    }

    public enum InterpolationMode
    {
        None,
        Interpolate,
        Extrapolate
    }

    public class Movement2D_UMFOSS : MonoBehaviour
    {
        [Header("Movement Mode")]
        [SerializeField] private MovementMode movementMode = MovementMode.TransformDirect;

        [Header("Common — applies to all modes")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private bool faceDirection = true;
        [SerializeField] private LayerMask groundLayer = -1;

        [Header("Transform Direct")]
        [SerializeField] private UpdateMode updateIn = UpdateMode.Update;

        [Header("Transform Translate")]
        [SerializeField] private SpaceMode space = SpaceMode.World;

        [Header("MoveTowards")]
        [SerializeField] private float maxDelta = 5f;

        [Header("Lerp Smooth")]
        [SerializeField] private float lerpSpeed = 6f;
        [SerializeField] private LerpTargetMode lerpTarget = LerpTargetMode.InputDirection;
        [SerializeField] private Transform targetTransform;

        [Header("SmoothDamp")]
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private float maxSmoothSpeed = 10f;

        [Header("Velocity Direct")]
        [SerializeField] private float horizontalDeceleration = 8f;
        [SerializeField] private bool preserveVertical = true;

        [Header("Force Additive")]
        [SerializeField] private float accelerationForce = 30f;
        [SerializeField] private float maxSpeed = 5f;
        [SerializeField] private float drag = 2f;

        [Header("Force Impulse")]
        [SerializeField] private float impulseForce = 8f;
        [SerializeField] private float impulseCooldown = 0.1f;

        [Header("Kinematic MovePosition")]
        [SerializeField] private MovementCollisionDetectionMode collisionDetection = MovementCollisionDetectionMode.Discrete;
        [SerializeField] private InterpolationMode interpolationMode = InterpolationMode.None;

        // Events
        public event Action<MovementMode> OnMovementStarted;
        public event Action<MovementMode> OnMovementStopped;
        public event Action<Vector2> OnDirectionChanged;
        public event Action<MovementMode, MovementMode> OnModeChanged;

        // Private fields
        private Rigidbody2D rb2d;
        private Vector2 currentInput;
        private Vector2 previousInput;
        private Vector3 smoothDampVelocity;
        private float lastImpulseTime;
        private bool isMoving;
        private MovementMode previousMode;
        private Vector2 currentDirection;

        // Adapters
        private IPhysicsAdapter physicsAdapter;
        private IInputAdapter inputAdapter;

        private void Awake()
        {
            // Get or add Rigidbody2D if needed
            rb2d = GetComponent<Rigidbody2D>();
            
            // Initialize physics adapter
            physicsAdapter = GetComponent<IPhysicsAdapter>();
            if (physicsAdapter == null)
            {
                physicsAdapter = gameObject.AddComponent<Physics2DAdapter_UMFOSS>();
            }

            // Initialize input adapter (Legacy Input by default)
            inputAdapter = new UnityLegacyInputAdapter();

            previousMode = movementMode;
            InitializeMode();
        }

        private void Update()
        {
            if (updateIn == UpdateMode.Update)
            {
                HandleMovement();
            }
        }

        private void FixedUpdate()
        {
            if (updateIn == UpdateMode.FixedUpdate)
            {
                HandleMovement();
            }
        }

        private void HandleMovement()
        {
            if (inputAdapter != null)
            {
                currentInput = inputAdapter.GetMovementInput();
            }
            
            // Handle direction change
            if (currentInput != previousInput)
            {
                currentDirection = currentInput.normalized;
                OnDirectionChanged?.Invoke(currentDirection);
                previousInput = currentInput;
            }

            // Handle movement start/stop
            bool wasMoving = isMoving;
            isMoving = currentInput.magnitude > 0.1f;

            if (!wasMoving && isMoving)
            {
                OnMovementStarted?.Invoke(movementMode);
            }
            else if (wasMoving && !isMoving)
            {
                OnMovementStopped?.Invoke(movementMode);
            }

            // Execute movement based on mode
            ExecuteMovement();

            // Handle face direction
            if (faceDirection && currentInput.x != 0)
            {
                FaceMovementDirection();
            }
        }

        private void ExecuteMovement()
        {
            switch (movementMode)
            {
                case MovementMode.TransformDirect:
                    ExecuteTransformDirect();
                    break;
                case MovementMode.TransformTranslate:
                    ExecuteTransformTranslate();
                    break;
                case MovementMode.MoveTowards:
                    ExecuteMoveTowards();
                    break;
                case MovementMode.LerpSmooth:
                    ExecuteLerpSmooth();
                    break;
                case MovementMode.SmoothDamp:
                    ExecuteSmoothDamp();
                    break;
                case MovementMode.VelocityDirect:
                    ExecuteVelocityDirect();
                    break;
                case MovementMode.ForceAdditive:
                    ExecuteForceAdditive();
                    break;
                case MovementMode.ForceImpulse:
                    ExecuteForceImpulse();
                    break;
                case MovementMode.KinematicMovePosition:
                    ExecuteKinematicMovePosition();
                    break;
            }
        }

        private void ExecuteTransformDirect()
        {
            Vector3 movement = new Vector3(currentInput.x, currentInput.y, 0) * moveSpeed * Time.deltaTime;
            transform.position += movement;
        }

        private void ExecuteTransformTranslate()
        {
            Vector3 movement = new Vector3(currentInput.x, currentInput.y, 0) * moveSpeed * Time.deltaTime;
            Space unitySpace = (space == SpaceMode.World) ? Space.World : Space.Self;
            transform.Translate(movement, unitySpace);
        }

        private void ExecuteMoveTowards()
        {
            Vector3 target = transform.position + new Vector3(currentInput.x, currentInput.y, 0) * moveSpeed;
            transform.position = Vector3.MoveTowards(transform.position, target, maxDelta * Time.deltaTime);
        }

        private void ExecuteLerpSmooth()
        {
            Vector3 target;
            if (lerpTarget == LerpTargetMode.InputDirection)
            {
                target = transform.position + new Vector3(currentInput.x, currentInput.y, 0) * moveSpeed;
            }
            else
            {
                if (targetTransform != null)
                {
                    target = targetTransform.position;
                }
                else
                {
                    target = transform.position + new Vector3(currentInput.x, currentInput.y, 0) * moveSpeed;
                }
            }
            transform.position = Vector3.Lerp(transform.position, target, lerpSpeed * Time.deltaTime);
        }

        private void ExecuteSmoothDamp()
        {
            Vector3 target = transform.position + new Vector3(currentInput.x, currentInput.y, 0) * moveSpeed;
            transform.position = Vector3.SmoothDamp(transform.position, target, ref smoothDampVelocity, smoothTime, maxSmoothSpeed);
        }

        private void ExecuteVelocityDirect()
        {
            if (rb2d == null)
            {
                Debug.LogError("VelocityDirect mode requires Rigidbody2D component!");
                return;
            }

            Vector2 newVelocity = new Vector2(currentInput.x * moveSpeed, currentInput.y * moveSpeed);
            
            if (preserveVertical)
            {
                newVelocity.y = physicsAdapter.Velocity.y;
            }

            if (currentInput.x == 0 && currentInput.y == 0)
            {
                // Apply deceleration when no input
                newVelocity.x = Mathf.MoveTowards(physicsAdapter.Velocity.x, 0, horizontalDeceleration * Time.deltaTime);
                if (!preserveVertical)
                {
                    newVelocity.y = Mathf.MoveTowards(physicsAdapter.Velocity.y, 0, horizontalDeceleration * Time.deltaTime);
                }
            }
            
            physicsAdapter.Velocity = newVelocity;
        }

        private void ExecuteForceAdditive()
        {
            if (rb2d == null)
            {
                Debug.LogError("ForceAdditive mode requires Rigidbody2D component!");
                return;
            }

            if (currentInput.magnitude > 0.1f)
            {
                Vector2 force = new Vector2(currentInput.x * accelerationForce, currentInput.y * accelerationForce);
                physicsAdapter.AddForce(force, false);
                
                // Clamp velocity
                Vector2 clampedVelocity = Vector2.ClampMagnitude(physicsAdapter.Velocity, maxSpeed);
                physicsAdapter.Velocity = clampedVelocity;
            }
        }

        private void ExecuteForceImpulse()
        {
            if (rb2d == null)
            {
                Debug.LogError("ForceImpulse mode requires Rigidbody2D component!");
                return;
            }

            if (currentInput.magnitude > 0.1f && Time.time > lastImpulseTime + impulseCooldown)
            {
                Vector2 impulse = new Vector2(currentInput.x * impulseForce, currentInput.y * impulseForce);
                physicsAdapter.AddForce(impulse, true);
                lastImpulseTime = Time.time;
            }
        }

        private void ExecuteKinematicMovePosition()
        {
            if (rb2d == null)
            {
                Debug.LogError("KinematicMovePosition mode requires Rigidbody2D component!");
                return;
            }

            Vector2 movement = new Vector2(currentInput.x, currentInput.y) * moveSpeed * Time.fixedDeltaTime;
            rb2d.MovePosition(rb2d.position + movement);
        }

        private void FaceMovementDirection()
        {
            Vector3 currentScale = transform.localScale;
            if (currentInput.x > 0)
            {
                transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
            }
            else if (currentInput.x < 0)
            {
                transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
            }
        }

        private void InitializeMode()
        {
            // Configure Rigidbody2D based on mode
            if (rb2d != null)
            {
                switch (movementMode)
                {
                    case MovementMode.KinematicMovePosition:
                        rb2d.bodyType = RigidbodyType2D.Kinematic;
                        rb2d.collisionDetectionMode = (UnityEngine.CollisionDetectionMode2D)collisionDetection;
                        rb2d.interpolation = (RigidbodyInterpolation2D)interpolationMode;
                        rb2d.linearDamping = 0;
                        break;
                    case MovementMode.ForceAdditive:
                        rb2d.bodyType = RigidbodyType2D.Dynamic;
                        rb2d.linearDamping = drag;
                        break;
                    case MovementMode.ForceImpulse:
                        rb2d.bodyType = RigidbodyType2D.Dynamic;
                        rb2d.linearDamping = drag;
                        break;
                    case MovementMode.VelocityDirect:
                        rb2d.bodyType = RigidbodyType2D.Dynamic;
                        rb2d.linearDamping = 0;
                        break;
                    default:
                        // Transform modes - make sure Rigidbody2D doesn't interfere
                        if (rb2d.bodyType == RigidbodyType2D.Dynamic)
                        {
                            rb2d.linearVelocity = Vector2.zero;
                        }
                        break;
                }
            }
        }

        private void CleanupPreviousMode(MovementMode oldMode)
        {
            switch (oldMode)
            {
                case MovementMode.SmoothDamp:
                    smoothDampVelocity = Vector3.zero;
                    break;
                case MovementMode.VelocityDirect:
                case MovementMode.ForceAdditive:
                case MovementMode.ForceImpulse:
                    if (physicsAdapter != null)
                    {
                        physicsAdapter.Velocity = Vector2.zero;
                    }
                    break;
                case MovementMode.KinematicMovePosition:
                    // Restore kinematic setting if needed
                    break;
                default:
                    // Transform modes - no cleanup needed
                    break;
            }
        }

        public void SetMode(MovementMode newMode)
        {
            if (newMode == movementMode) return;

            CleanupPreviousMode(movementMode);
            
            MovementMode oldMode = movementMode;
            movementMode = newMode;
            
            InitializeMode();
            OnModeChanged?.Invoke(oldMode, newMode);
        }

        // Public getters for runtime inspection
        public MovementMode CurrentMode => movementMode;
        public Vector2 CurrentDirection => currentDirection;
        public bool IsMoving => isMoving;
    }

    /// <summary>
    /// Interface for abstraction of input systems.
    /// </summary>
    public interface IInputAdapter
    {
        Vector2 GetMovementInput();
    }

    /// <summary>
    /// Standard Unity implementation of the Input Adapter using legacy Input.
    /// </summary>
    public class UnityLegacyInputAdapter : IInputAdapter
    {
        public Vector2 GetMovementInput()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }
}
