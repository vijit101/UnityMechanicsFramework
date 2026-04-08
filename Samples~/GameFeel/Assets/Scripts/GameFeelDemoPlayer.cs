using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.GameFeel
{
    /// <summary>
    /// Demo player controller that publishes GameFeel events in response to
    /// keyboard input. Demonstrates how any gameplay mechanic can trigger
    /// visual feedback through the EventBus without referencing GameFeel_UMFOSS.
    ///
    /// Controls:
    ///   WASD / Arrow Keys — Move
    ///   Space — Jump
    ///   Left Shift — Dash
    ///   Left Click — Attack (hit registered)
    ///   F — Take damage
    ///   G — Simulate death
    ///   E — Pick up item
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class GameFeelDemoPlayer : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float jumpForce = 14f;

        [Header("Dash")]
        [SerializeField] private float dashSpeed = 20f;
        [SerializeField] private float dashDuration = 0.2f;

        [Header("Ground Check")]
        [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundLayer;

        private Rigidbody2D rb;
        private bool isGrounded;
        private bool wasGrounded;
        private bool isDashing;
        private float dashTimer;
        private Vector2 dashDirection;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            CheckGround();
            HandleMovement();
            HandleJump();
            HandleDash();
            HandleAttack();
            HandleDamage();
            HandleDeath();
            HandlePickup();
            DetectLanding();

            wasGrounded = isGrounded;
        }

        private void CheckGround()
        {
            if (groundCheckPoint != null)
            {
                isGrounded = Physics2D.OverlapCircle(
                    groundCheckPoint.position,
                    groundCheckRadius,
                    groundLayer
                );
            }
        }

        private void HandleMovement()
        {
            if (isDashing) return;

            float horizontal = Input.GetAxisRaw("Horizontal");
            rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);
        }

        private void HandleJump()
        {
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                EventBus.Publish(new OnJumpStart { jumpForce = jumpForce });
            }
        }

        private void HandleDash()
        {
            if (isDashing)
            {
                dashTimer -= Time.deltaTime;
                rb.linearVelocity = dashDirection * dashSpeed;

                if (dashTimer <= 0f)
                {
                    isDashing = false;
                    EventBus.Publish(new OnDashEnd());
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                isDashing = true;
                dashTimer = dashDuration;
                float facing = Mathf.Sign(transform.localScale.x);
                float horizontal = Input.GetAxisRaw("Horizontal");
                dashDirection = horizontal != 0
                    ? new Vector2(horizontal, 0f).normalized
                    : new Vector2(facing, 0f);

                EventBus.Publish(new OnDashStart { dashDirection = dashDirection });
            }
        }

        private void HandleAttack()
        {
            if (Input.GetMouseButtonDown(0))
            {
                EventBus.Publish(new OnHitRegistered
                {
                    hitPoint = transform.position + transform.right,
                    intensity = 1f
                });
            }
        }

        private void HandleDamage()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                EventBus.Publish(new OnDamageTaken
                {
                    damageAmount = 10f,
                    hitDirection = -transform.right
                });
            }
        }

        private void HandleDeath()
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                EventBus.Publish(new OnDeath { deathPosition = transform.position });
            }
        }

        private void HandlePickup()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                EventBus.Publish(new OnItemPickedUp { itemName = "DemoItem" });
            }
        }

        private void DetectLanding()
        {
            if (isGrounded && !wasGrounded)
            {
                EventBus.Publish(new OnLanding { fallSpeed = Mathf.Abs(rb.linearVelocity.y) });
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }
        }
    }
}
