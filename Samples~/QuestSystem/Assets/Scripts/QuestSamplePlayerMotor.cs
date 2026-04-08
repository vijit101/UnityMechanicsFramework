using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.QuestSystem
{
    /// <summary>
    /// Simple third-person movement for the sample (no quest references).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class QuestSamplePlayerMotor : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeed = 8f;

        [SerializeField]
        private float gravity = -20f;

        private CharacterController _controller;

        private Vector3 _velocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!QuestSamplePlayerLifecycle.IsActionAllowed)
            {
                return;
            }

            var h = Input.GetAxisRaw("Horizontal");
            var v = Input.GetAxisRaw("Vertical");
            var cam = Camera.main;
            Vector3 move;
            if (cam != null)
            {
                var forward = cam.transform.forward;
                forward.y = 0f;
                forward.Normalize();
                var right = cam.transform.right;
                right.y = 0f;
                right.Normalize();
                move = (forward * v + right * h).normalized;
            }
            else
            {
                move = (transform.forward * v + transform.right * h).normalized;
            }

            if (_controller.isGrounded && _velocity.y < 0f)
            {
                _velocity.y = -2f;
            }

            _controller.Move(move * (moveSpeed * Time.deltaTime));
            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
    }
}
