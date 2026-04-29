// using UnityEngine;
// using GameplayMechanicsUMFOSS.Physics;

// namespace GameplayMechanicsUMFOSS.Movement
// {
//     public class DashSystem_UMFOSS : MonoBehaviour
//     {
//         [Header("Movement")]
//         [SerializeField] private float moveSpeed = 5f;

//         [Header("Dash")]
//         [SerializeField] private float dashDistance = 5f;
//         [SerializeField] private float dashDuration = 0.2f;

//         private IPhysicsAdapter _physics;

//         private bool _isDashing;
//         private float _dashTimer;
//         private Vector3 _dashVelocity;

//         private Vector3 _moveInput;
//         private Vector3 _lastMoveDirection = Vector3.forward;

//         private void Start()
//         {
//             _physics = GetComponent<Physics3DAdapter>();

//             if (_physics == null)
//                 Debug.LogError("Physics3DAdapter missing!");
//         }

//         private void Update()
//         {
//             // 🎮 Movement input
//             float h = Input.GetAxis("Horizontal");
//             float v = Input.GetAxis("Vertical");

//             _moveInput = new Vector3(h, 0, v);

//             // Store last valid direction
//             if (_moveInput.sqrMagnitude > 0.01f)
//             {
//                 _lastMoveDirection = _moveInput.normalized;

//                 // Rotate player (optional)
//                 transform.forward = _lastMoveDirection;
//             }

//             // ⚡ Dash input
//             if (Input.GetKeyDown(KeyCode.Space))
//             {
//                 StartDash();
//             }
//         }

//         private void FixedUpdate()
//         {
//             if (_isDashing)
//             {
//                 _dashTimer -= Time.fixedDeltaTime;

//                 _physics.SetVelocity(_dashVelocity);

//                 if (_dashTimer <= 0f)
//                 {
//                     _isDashing = false;
//                 }
//             }
//             else
//             {
//                 // Normal movement
//                 Vector3 velocity = _moveInput * moveSpeed;
//                 _physics.SetVelocity(velocity);
//             }
//         }

//         private void StartDash()
//         {
//             if (_isDashing) return;

//             _isDashing = true;
//             _dashTimer = dashDuration;

//             // 🔥 DASH IN LAST MOVE DIRECTION
//             Vector3 direction = _lastMoveDirection.normalized;

//             _dashVelocity = (direction * dashDistance) / dashDuration;
//         }
//     }
// }


using UnityEngine;
using GameplayMechanicsUMFOSS.Physics;

namespace GameplayMechanicsUMFOSS.Movement
{
    public class DashSystem_UMFOSS : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 5f;

        [Header("Dash")]
        [SerializeField] private float dashDistance = 5f;
        [SerializeField] private float dashDuration = 0.2f;

        private IPhysicsAdapter _physics;

        private bool _isDashing;
        private float _dashTimer;
        private Vector3 _dashVelocity;

        private Vector3 _moveInput;
        private Vector3 _lastMoveDirection = Vector3.right;

        private bool _is2D;

        private void Start()
        {
            // 🔥 SAFE adapter fetch (works with interface)
            var adapters = GetComponents<MonoBehaviour>();

            foreach (var adapter in adapters)
            {
                if (adapter is IPhysicsAdapter physicsAdapter)
                {
                    _physics = physicsAdapter;
                    break;
                }
            }

            if (_physics == null)
            {
                Debug.LogError("No Physics Adapter found!");
                return;
            }

            _is2D = GetComponent<Rigidbody2D>() != null;
        }

        private void Update()
        {
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            if (_is2D)
                _moveInput = new Vector3(h, v, 0); // 2D
            else
                _moveInput = new Vector3(h, 0, v); // 3D

            if (_moveInput.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = _moveInput.normalized;

                if (!_is2D)
                    transform.forward = _lastMoveDirection;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartDash();
            }
        }

        private void FixedUpdate()
        {
            if (_physics == null) return;

            if (_isDashing)
            {
                _dashTimer -= Time.fixedDeltaTime;

                _physics.SetVelocity(_dashVelocity);

                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                }
            }
            else
            {
                Vector3 velocity = _moveInput * moveSpeed;
                _physics.SetVelocity(velocity);
            }
        }

        private void StartDash()
        {
            if (_isDashing) return;

            _isDashing = true;
            _dashTimer = dashDuration;

            Vector3 direction = _lastMoveDirection.normalized;
            _dashVelocity = (direction * dashDistance) / dashDuration;
        }
    }
}