using UnityEngine;

namespace GameplayMechanicsUMFOSS.Movement
{
    [CreateAssetMenu(fileName = "GroundPoundConfig", menuName = "UMFOSS/Movement/GroundPoundConfig")]
    public class GroundPoundConfig_UMFOSS : ScriptableObject 
    {
        [Header("Trigger")]
        public KeyCode poundKey = KeyCode.S;
        public float hangDuration = 0.08f;
        public bool requireDoubleDown = false;

        [Header("Descent")]
        public float poundGravityScale = 8f;
        public float maxDescentSpeed = 30f;
        public bool lockHorizontal = true;

        [Header("Impact")]
        public float shockwaveRadius = 2.5f;
        public float shockwaveDuration = 0.1f;
        public float shockwaveDamage = 20f;
        public LayerMask damageLayer;

        [Header("Feedback")]
        public float shakeIntensity = 0.5f;
        public float shakeDuration = 0.3f;
        public Vector3 squashScale = new Vector3(1.4f, 0.6f, 1f);
        public float squashRecovery = 8f;

        [Header("Recovery")]
        public float recoveryDuration = 0.15f;
        public bool allowJumpCancel = true;

        [Header("Cooldown")]
        public float cooldown = 0.5f;
    }
}