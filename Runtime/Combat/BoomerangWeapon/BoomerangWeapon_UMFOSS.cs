using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Physics;
using GameplayMechanicsUMFOSS.Combat.States;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Orchestrator for the God-of-War-style throw-and-return weapon system.
    /// Delegates state logic to individual state classes via the generic StateMachine.
    /// Configuration lives in a BoomerangConfig ScriptableObject for reuse across weapon variants.
    /// </summary>
    public class BoomerangWeapon_UMFOSS : MonoBehaviour
    {
        // ───────────────────────────────────────────────
        // Serialized Fields
        // ───────────────────────────────────────────────

        [Header("Configuration")]
        [SerializeField] private BoomerangConfig config;

        [Header("References")]
        [SerializeField] private Transform handSocket;
        [SerializeField] private Transform visualPivot;
        [SerializeField] private Collider weaponCollider;

        // ───────────────────────────────────────────────
        // Private Fields
        // ───────────────────────────────────────────────

        private StateMachine<WeaponStateKey> stateMachine;
        private BoomerangContext context;
        private ThrownState thrownState;

        // ───────────────────────────────────────────────
        // Public Properties
        // ───────────────────────────────────────────────

        /// <summary>The weapon's current movement state.</summary>
        public WeaponStateKey CurrentState => stateMachine != null ? stateMachine.CurrentStateKey : WeaponStateKey.Equipped;

        /// <summary>True when the weapon is in the player's hand and ready to throw.</summary>
        public bool IsEquipped => stateMachine == null || stateMachine.CurrentStateKey == WeaponStateKey.Equipped;

        // ───────────────────────────────────────────────
        // Unity Lifecycle
        // ───────────────────────────────────────────────

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError($"[BoomerangWeapon] Missing BoomerangConfig on {name}. Disabling.", this);
                enabled = false;
                return;
            }

            var physics = GetComponent<IPhysicsAdapter>();
            if (physics == null)
            {
                // Auto-add Physics3DAdapter if a Rigidbody is present
                if (GetComponent<Rigidbody>() != null)
                {
                    physics = gameObject.AddComponent<Physics3DAdapter>();
                    Debug.Log($"[BoomerangWeapon] Auto-added Physics3DAdapter to {name}.", this);
                }
                else
                {
                    Debug.LogError($"[BoomerangWeapon] Missing Rigidbody and IPhysicsAdapter on {name}. Disabling.", this);
                    enabled = false;
                    return;
                }
            }

            var visualHandler = new WeaponVisualHandler(visualPivot, config.RotationPerSecond);
            var hitDetector = new HitDetector(weaponCollider, config.HurtboxLayer, gameObject, config);

            context = new BoomerangContext
            {
                Transform = transform,
                Physics = physics,
                WeaponCollider = weaponCollider,
                HandSocket = handSocket,
                Config = config,
                VisualHandler = visualHandler,
                HitDetector = hitDetector
            };

            stateMachine = new StateMachine<WeaponStateKey>();
            context.StateMachine = stateMachine;

            var equippedState = new EquippedState(context);
            thrownState = new ThrownState(context);
            var embeddedState = new EmbeddedState(context);
            var recallingState = new RecallingState(context);

            stateMachine.AddState(WeaponStateKey.Equipped, equippedState);
            stateMachine.AddState(WeaponStateKey.Thrown, thrownState);
            stateMachine.AddState(WeaponStateKey.Embedded, embeddedState);
            stateMachine.AddState(WeaponStateKey.Recalling, recallingState);
        }

        private void Start()
        {
            if (stateMachine == null) return;
            stateMachine.ChangeState(WeaponStateKey.Equipped);
        }

        private void Update()
        {
            if (stateMachine == null) return;
            stateMachine.Update();
        }

        private void FixedUpdate()
        {
            if (stateMachine == null) return;
            stateMachine.FixedUpdate();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (CurrentState != WeaponStateKey.Thrown) return;
            thrownState.HandleCollision(collision);
        }

        // ───────────────────────────────────────────────
        // Public Methods
        // ───────────────────────────────────────────────

        /// <summary>
        /// Throws the weapon in the given world-space direction.
        /// Only works when the weapon is currently Equipped.
        /// </summary>
        public void Throw(Vector3 direction)
        {
            if (stateMachine == null || CurrentState != WeaponStateKey.Equipped) return;

            context.ThrowDirection = direction.normalized;
            context.ThrowOrigin = transform.position;

            stateMachine.ChangeState(WeaponStateKey.Thrown);
        }

        /// <summary>
        /// Recalls the weapon back to the player's hand.
        /// Works when the weapon is Thrown or Embedded.
        /// </summary>
        public void Recall()
        {
            if (stateMachine == null || CurrentState == WeaponStateKey.Equipped || CurrentState == WeaponStateKey.Recalling) return;
            stateMachine.ChangeState(WeaponStateKey.Recalling);
        }
    }
}
