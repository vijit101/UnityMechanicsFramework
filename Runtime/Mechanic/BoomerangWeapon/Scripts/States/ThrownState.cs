using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Combat.States
{
    /// <summary>
    /// Weapon is flying through the air after being thrown.
    /// Uses physics velocity shaped by the throw curve. Spins the visual pivot.
    /// Transitions to Embedded on surface collision or auto-recalls at max range.
    /// </summary>
    public class ThrownState : IState
    {
        private readonly BoomerangContext ctx;
        private float distanceTravelled;

        public ThrownState(BoomerangContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            distanceTravelled = 0f;

            ctx.Transform.SetParent(null);
            ctx.Transform.forward = ctx.ThrowDirection;

            ctx.Physics.SetKinematic(false);
            ctx.Physics.AddForce(ctx.ThrowDirection * ctx.Config.ThrowForce, ForceMode.VelocityChange);

            ctx.WeaponCollider.enabled = true;
            ctx.HitDetector.ClearHitTargets(returnFlight: false);

            EventBus.Publish(new WeaponThrownEvent
            {
                Direction = ctx.ThrowDirection,
                Weapon = ctx.Transform.gameObject
            });
        }

        public void Update()
        {
            ctx.VisualHandler.Spin();

            distanceTravelled = Vector3.Distance(ctx.ThrowOrigin, ctx.Transform.position);

            float normalizedDist = Mathf.Clamp01(distanceTravelled / ctx.Config.MaxRange);
            float curveMultiplier = ctx.Config.ThrowCurve.Evaluate(normalizedDist);
            ctx.Physics.Velocity = ctx.ThrowDirection * (ctx.Config.ThrowForce * curveMultiplier);

            ctx.HitDetector.CheckOverlap(ctx.Transform.position);

            if (ctx.Config.EmbedInTargets && ctx.HitDetector.LastHitCollider != null)
            {
                ctx.LastHurtboxHit = ctx.HitDetector.LastHitCollider;
                ctx.StateMachine.ChangeState(WeaponStateKey.Embedded);
                return;
            }

            if (distanceTravelled >= ctx.Config.MaxRange)
            {
                ctx.StateMachine.ChangeState(WeaponStateKey.Recalling);
            }
        }

        public void FixedUpdate() { }

        public void Exit() { }

        /// <summary>
        /// Called by the orchestrator's OnCollisionEnter when hitting a stuck-layer surface.
        /// Stores the collision in context so EmbeddedState.Enter() can parent to the surface.
        /// </summary>
        public void HandleCollision(Collision collision)
        {
            if (!LayerMaskUtils.IsInMask(collision.gameObject.layer, ctx.Config.StuckLayer))
                return;

            ctx.LastCollision = collision;
            ctx.StateMachine.ChangeState(WeaponStateKey.Embedded);
        }
    }
}
