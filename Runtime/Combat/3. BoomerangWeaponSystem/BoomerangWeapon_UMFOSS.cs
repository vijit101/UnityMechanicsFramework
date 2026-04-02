using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Physics;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Throw-and-recall weapon inspired by God of War's Leviathan Axe.
    /// Supports embedding in surfaces and curved Bezier return paths.
    /// </summary>
    public class BoomerangWeapon_UMFOSS : MonoBehaviour
    {
        private enum WeaponState
        {
            Equipped,
            Thrown,
            Embedded,
            Recalling
        }

        [Header("Throw Settings")]
        [Tooltip("Throw velocity in units/second.")]
        [SerializeField] private float throwForce = 25f;

        [Tooltip("Max travel distance before auto-embedding.")]
        [SerializeField] private float maxRange = 50f;

        [Header("Return Settings")]
        [Tooltip("Base return flight speed.")]
        [SerializeField] private float returnSpeed = 15f;

        [Tooltip("Return speed increase per second.")]
        [SerializeField] private float returnAcceleration = 25f;

        [Tooltip("Distance at which the weapon snaps back into the hand.")]
        [SerializeField] private float catchDistance = 0.5f;

        [Tooltip("Use a curved Bezier arc instead of a straight-line return.")]
        [SerializeField] private bool useCurvedReturn = true;

        [Header("Collision & Layers")]
        [Tooltip("Layers that take damage on hit.")]
        [SerializeField] private LayerMask hurtboxLayer;

        [Tooltip("Layers the weapon embeds into on impact.")]
        [SerializeField] private LayerMask stuckLayer;

        [Tooltip("Spin speed (deg/s) applied to the visual pivot while airborne.")]
        [SerializeField] private Vector3 rotationPerSecond = new Vector3(1440f, 0f, 0f);

        [Header("Visuals & Points")]
        [Tooltip("Transform where the weapon sits when equipped.")]
        [SerializeField] private Transform handSocket;

        [Tooltip("Child mesh transform. Spin goes here so the parent forward stays correct.")]
        [SerializeField] private Transform visualPivot;

        private WeaponState currentState = WeaponState.Equipped;
        private IPhysicsAdapter physics;
        private Rigidbody rb; // needed for useGravity/freezeRotation (not on IPhysicsAdapter)
        private Collider weaponCollider;

        private Vector3 throwOrigin;
        private Vector3 throwDirection;
        private float currentReturnSpeed;
        private float returnT;

        private Vector3 bezierStart;
        private Vector3 bezierControl;

        private Vector3 equippedLocalPos;
        private Quaternion equippedLocalRot;

        public bool IsEquipped => currentState == WeaponState.Equipped;
        public bool IsEmbedded => currentState == WeaponState.Embedded;

        private void Awake()
        {
            physics = GetComponent<IPhysicsAdapter>();
            rb = GetComponent<Rigidbody>();
            weaponCollider = GetComponent<Collider>();

            if (physics == null)
            {
                Debug.LogError($"[BoomerangWeapon] No IPhysicsAdapter found on {gameObject.name}. " +
                               "Add a Rigidbody3DAdapter component.");
            }

            equippedLocalPos = transform.localPosition;
            equippedLocalRot = transform.localRotation;
        }

        private void Update()
        {
            switch (currentState)
            {
                case WeaponState.Thrown:
                    HandleThrownUpdate();
                    break;
                case WeaponState.Recalling:
                    HandleRecallingUpdate();
                    break;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (currentState != WeaponState.Thrown)
                return;

            if (IsInLayerMask(collision.gameObject.layer, stuckLayer))
            {
                // Skip floors -- only embed in walls/vertical surfaces
                ContactPoint contact = collision.GetContact(0);
                if (contact.normal.y > 0.7f)
                    return;

                EmbedWeapon(collision);
            }
        }

        /// <summary>Throws the weapon in the given direction. Only works when equipped.</summary>
        public void Throw(Vector3 direction)
        {
            if (currentState != WeaponState.Equipped)
                return;

            throwOrigin = transform.position;

            transform.SetParent(null);

            throwDirection = direction.normalized;
            physics.IsKinematic = false;
            rb.useGravity = false;
            rb.freezeRotation = true;
            weaponCollider.enabled = true;

            currentState = WeaponState.Thrown;
            EventBus.Publish(new WeaponThrownEvent { Direction = direction.normalized });
        }

        /// <summary>Recalls the weapon back to the player's hand.</summary>
        public void Recall()
        {
            if (currentState == WeaponState.Equipped || currentState == WeaponState.Recalling)
                return;

            transform.SetParent(null);

            // Kinematic during recall -- we drive position manually
            physics.ClearForces();
            physics.IsKinematic = true;

            // Phase through geometry on the way back
            weaponCollider.enabled = false;

            bezierStart = transform.position;
            bezierControl = ComputeBezierControl();
            returnT = 0f;
            currentReturnSpeed = returnSpeed;

            currentState = WeaponState.Recalling;
            EventBus.Publish(new WeaponRecallStartedEvent());
        }

        private void HandleThrownUpdate()
        {
            // Force velocity every frame -- physics can't drift it
            physics.Velocity = throwDirection * throwForce;
            transform.forward = throwDirection;

            visualPivot.Rotate(rotationPerSecond * Time.deltaTime, Space.Self);

            float distanceTravelled = Vector3.Distance(throwOrigin, transform.position);

            if (distanceTravelled >= maxRange)
            {
                physics.ClearForces();
                physics.IsKinematic = true;
                visualPivot.localRotation = Quaternion.identity;
                currentState = WeaponState.Embedded;
            }
        }

        private void HandleRecallingUpdate()
        {
            visualPivot.Rotate(rotationPerSecond * Time.deltaTime, Space.Self);
            currentReturnSpeed += returnAcceleration * Time.deltaTime;

            if (useCurvedReturn)
            {
                float remainingDistance = Vector3.Distance(transform.position, handSocket.position);
                float step = currentReturnSpeed * Time.deltaTime;

                if (remainingDistance > 0.01f)
                {
                    returnT += step / remainingDistance;
                }
                else
                {
                    returnT = 1f;
                }

                // Recompute each frame because the player might be moving
                bezierControl = ComputeBezierControl();

                transform.position = BezierUtility.QuadraticBezier(
                    bezierStart, bezierControl, handSocket.position, returnT);
            }
            else
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, handSocket.position, currentReturnSpeed * Time.deltaTime);
            }

            Vector3 toHand = handSocket.position - transform.position;
            if (toHand.sqrMagnitude > catchDistance * catchDistance)
                transform.forward = toHand.normalized;
            else
            {
                CatchWeapon();
            }
        }

        private void EmbedWeapon(Collision collision)
        {
            physics.ClearForces();
            physics.IsKinematic = true;

            // Align perpendicular to the hit surface
            ContactPoint contact = collision.GetContact(0);
            transform.forward = -contact.normal;

            // Parent to surface so it moves with platforms
            transform.SetParent(collision.transform);
            visualPivot.localRotation = Quaternion.identity;

            currentState = WeaponState.Embedded;
            EventBus.Publish(new WeaponStuckEvent
            {
                Surface = collision.gameObject,
                Point = contact.point
            });
        }

        private void CatchWeapon()
        {
            transform.SetParent(handSocket);
            transform.localPosition = equippedLocalPos;
            transform.localRotation = equippedLocalRot;

            physics.ClearForces();
            physics.IsKinematic = true;
            visualPivot.localRotation = Quaternion.identity;
            weaponCollider.enabled = true;

            currentState = WeaponState.Equipped;
            EventBus.Publish(new WeaponCaughtEvent());
        }

        /// <summary>Bezier control point: midpoint offset up and to the right for the arc.</summary>
        private Vector3 ComputeBezierControl()
        {
            Vector3 midpoint = (transform.position + handSocket.position) * 0.5f;
            Vector3 offset = Vector3.up * 3f + handSocket.right * 2f;
            return midpoint + offset;
        }

        private static bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (handSocket != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(handSocket.position, catchDistance);
            }

            if (currentState == WeaponState.Recalling && useCurvedReturn)
            {
                Gizmos.color = Color.yellow;
                Vector3 prev = bezierStart;
                for (int i = 1; i <= 20; i++)
                {
                    float t = i / 20f;
                    Vector3 point = BezierUtility.QuadraticBezier(
                        bezierStart, bezierControl, handSocket.position, t);
                    Gizmos.DrawLine(prev, point);
                    prev = point;
                }
            }
        }
#endif
    }
}
