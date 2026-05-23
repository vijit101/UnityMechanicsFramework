using UnityEngine;
using System.Collections;

namespace GameplayMechanicsUMFOSS.Movement
{
    public class GroundPound_UMFOSS : MonoBehaviour 
    {
        [Header("Configuration")]
        public GroundPoundConfig_UMFOSS config;

        [Header("References")]
        public Rigidbody2D rb;
        public Transform shockwaveOrigin; 
        
        [SerializeField] private bool isGrounded; 
        private GroundPoundState state = GroundPoundState.Ready;
        private Vector3 originalScale;
        private float cooldownTimer = 0f;
        private bool jumpCancelRequested = false;

        private void Start() => originalScale = transform.localScale;

        //Restore gravity if script is disabled mid-slam
        private void OnDisable() 
        {
            StopAllCoroutines();
            if (rb != null) rb.gravityScale = 1f;
        }

        private void Update() 
        {
            if (config == null || rb.velocity == null) return;

            // Handle Cooldown logic
            if (cooldownTimer > 0) 
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0) state = GroundPoundState.Ready;
            }

            // Trigger: Only while airborne and Ready
            if (state == GroundPoundState.Ready && !isGrounded && Input.GetKeyDown(config.poundKey)) 
            {
                StartCoroutine(PoundSequence());
            }

            // Capture cancel input in Update where GetKeyDown is reliable
            if (state == GroundPoundState.Descending && config.allowJumpCancel && Input.GetKeyDown(KeyCode.Space))
            {
                jumpCancelRequested = true;
            }
        }

        private IEnumerator PoundSequence() 
        {
            // MOMENT 1: TRIGGER
            state = GroundPoundState.HangTime;
            rb.velocity = Vector2.zero; 
            rb.gravityScale = 0f;       
            yield return new WaitForSeconds(config.hangDuration);

            // MOMENT 2: DESCENT
            state = GroundPoundState.Descending;
            rb.gravityScale = config.poundGravityScale; 

            while (!isGrounded) 
            {
                // Jump Cancel Check
                if (config.allowJumpCancel && jumpCancelRequested)
                {
                    jumpCancelRequested = false;
                    CancelPound(); 
                    yield break; 
                }

                if (config.lockHorizontal) rb.velocity = new Vector2(0, rb.velocity.y);
                
                // Requirement: Sync with Physics
                yield return new WaitForFixedUpdate(); 
            }

            // MOMENT 3 + 4: IMPACT & FEEDBACK
            state = GroundPoundState.Impact;
            rb.velocity = Vector2.zero; 
            rb.gravityScale = 1f;
            
            StartCoroutine(SquashSequence());
            Debug.Log($"SCREEN SHAKE: Intensity {config.shakeIntensity}");

            // OverlapCircleAll for shockwave (Requirement)
            Collider2D[] hits = Physics2D.OverlapCircleAll(shockwaveOrigin.position, config.shockwaveRadius, config.damageLayer);
            foreach (var hit in hits) { Debug.Log("Hit: " + hit.name); }

            // MOMENT 5: RECOVERY
            state = GroundPoundState.Recovery;
            yield return new WaitForSeconds(config.recoveryDuration);

            // COOLDOWN
            state = GroundPoundState.Cooldown;
            cooldownTimer = config.cooldown;
        }

        private IEnumerator SquashSequence()
        {
            transform.localScale = config.squashScale;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime * config.squashRecovery;
                transform.localScale = Vector3.Lerp(config.squashScale, originalScale, t);
                yield return null;
            }
            transform.localScale = originalScale;
        }

        private void CancelPound()
        {
            jumpCancelRequested = false;
            StopAllCoroutines();
            rb.gravityScale = 1f; 
            rb.velocity = Vector2.zero; 
            state = GroundPoundState.Ready; 
            Debug.Log("Ground Pound Cancelled!");
        }

        private void OnCollisionEnter2D(Collision2D collision) => isGrounded = true;
        private void OnCollisionExit2D(Collision2D collision) => isGrounded = false;

        private void OnDrawGizmos() 
        {
            if (shockwaveOrigin == null || config == null) return;
            Gizmos.color = new Color(1, 0.3f, 0, 0.3f);
            Gizmos.DrawWireSphere(shockwaveOrigin.position, config.shockwaveRadius);
        }
    }

    public enum GroundPoundState { Ready, HangTime, Descending, Impact, Recovery, Cooldown }
}