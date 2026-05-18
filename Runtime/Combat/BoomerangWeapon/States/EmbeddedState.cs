using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Combat.States
{
    /// <summary>
    /// Weapon is lodged in a surface or target, parented to whatever it hit.
    /// Kinematic, no collider. Waits for a recall command.
    /// Auto-recalls if the parent is destroyed or the embedded target dies.
    /// </summary>
    public class EmbeddedState : IState
    {
        private readonly BoomerangContext ctx;
        private Transform embeddedParent;
        private Damageable embeddedDamageable;

        public EmbeddedState(BoomerangContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            ctx.Physics.SetKinematic(true);
            ctx.WeaponCollider.enabled = false;

            embeddedParent = null;
            embeddedDamageable = null;
            ctx.EmbeddedInCollider = null;

            Collision collision = ctx.LastCollision;
            if (collision != null)
            {
                embeddedParent = collision.transform;
                ctx.Transform.SetParent(embeddedParent);

                ContactPoint contact = collision.GetContact(0);
                EventBus.Publish(new WeaponStuckEvent
                {
                    Surface = collision.gameObject,
                    ContactPoint = contact.point,
                    Weapon = ctx.Transform.gameObject
                });

                ctx.LastCollision = null;
            }
            else if (ctx.LastHurtboxHit != null)
            {
                embeddedParent = ctx.LastHurtboxHit.transform;
                ctx.EmbeddedInCollider = ctx.LastHurtboxHit;
                ctx.Transform.SetParent(embeddedParent);

                // Subscribe to target death for auto-recall
                embeddedDamageable = ctx.LastHurtboxHit.GetComponentInParent<Damageable>();
                if (embeddedDamageable != null)
                {
                    embeddedDamageable.OnDeath += OnEmbeddedTargetDied;
                }

                EventBus.Publish(new WeaponStuckEvent
                {
                    Surface = ctx.LastHurtboxHit.gameObject,
                    ContactPoint = ctx.Transform.position,
                    Weapon = ctx.Transform.gameObject
                });

                ctx.LastHurtboxHit = null;
            }

            ctx.VisualHandler.ResetRotation();
        }

        public void Update()
        {
            // If the parent was destroyed (e.g., object pooling), auto-recall
            if (embeddedParent == null)
            {
                ctx.Transform.SetParent(null);
                ctx.StateMachine.ChangeState(WeaponStateKey.Recalling);
            }
        }

        public void FixedUpdate() { }

        public void Exit()
        {
            if (embeddedDamageable != null)
            {
                embeddedDamageable.OnDeath -= OnEmbeddedTargetDied;
                embeddedDamageable = null;
            }
        }

        private void OnEmbeddedTargetDied(Damageable dead)
        {
            // Unparent before the target ragdolls/teleports, then auto-recall
            Vector3 worldPos = ctx.Transform.position;
            ctx.Transform.SetParent(null);
            ctx.Transform.position = worldPos;
            ctx.StateMachine.ChangeState(WeaponStateKey.Recalling);
        }
    }
}
