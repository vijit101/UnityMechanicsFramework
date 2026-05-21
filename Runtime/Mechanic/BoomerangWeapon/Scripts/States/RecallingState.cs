using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Combat.States
{
    /// <summary>
    /// Weapon is flying back to the player's hand along a quadratic Bezier curve.
    /// Starts with a brief wiggle to telegraph the recall, then accelerates along
    /// the arc for the signature "snap" feel.
    /// Checks for hurtbox overlaps on the return path.
    /// </summary>
    public class RecallingState : IState
    {
        private readonly BoomerangContext ctx;
        private Vector3 returnStartPos;
        private Vector3 bezierControlPoint;
        private float currentSpeed;
        private float progress;

        private const float WiggleDuration = 0.12f;
        private const float WiggleIntensity = 0.03f;
        private float wiggleTimer;
        private Vector3 wiggleOrigin;
        private bool isWiggling;

        public RecallingState(BoomerangContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            // Safe unparent: capture world position before unparenting.
            // If parent was destroyed, Transform.position still returns the last known world position.
            Vector3 worldPos = ctx.Transform.position;
            ctx.Transform.SetParent(null);
            ctx.Transform.position = worldPos;

            ctx.Physics.SetKinematic(true);
            ctx.WeaponCollider.enabled = false;

            returnStartPos = ctx.Transform.position;
            currentSpeed = ctx.Config.ReturnSpeed;
            progress = 0f;

            wiggleOrigin = returnStartPos;
            wiggleTimer = 0f;
            isWiggling = true;

            ctx.HitDetector.ClearHitTargets(returnFlight: true);

            // Exclude the target we were embedded in from recall damage
            if (ctx.EmbeddedInCollider != null)
            {
                ctx.HitDetector.ExcludeCollider(ctx.EmbeddedInCollider);
                ctx.EmbeddedInCollider = null;
            }

            EventBus.Publish(new WeaponRecallStartedEvent
            {
                Weapon = ctx.Transform.gameObject
            });
        }

        public void Update()
        {
            if (ctx.HandSocket == null)
            {
                // Safety: if hand socket is gone, snap back to equipped
                ctx.StateMachine.ChangeState(WeaponStateKey.Equipped);
                return;
            }

            if (isWiggling)
            {
                wiggleTimer += Time.deltaTime;
                if (wiggleTimer >= WiggleDuration)
                {
                    isWiggling = false;
                    ctx.Transform.position = wiggleOrigin;

                    bezierControlPoint = BezierUtils.ComputeArcControlPoint(
                        returnStartPos,
                        ctx.HandSocket.position,
                        ctx.Config.ArcHeightRatio,
                        ctx.Config.ArcSideRatio
                    );
                }
                else
                {
                    Vector3 offset = new Vector3(
                        Mathf.Sin(wiggleTimer * 120f) * WiggleIntensity,
                        Mathf.Cos(wiggleTimer * 90f) * WiggleIntensity,
                        0f
                    );
                    ctx.Transform.position = wiggleOrigin + offset;
                }
                return;
            }

            ctx.VisualHandler.Spin();

            currentSpeed += ctx.Config.ReturnAcceleration * Time.deltaTime;

            float totalDistance = Vector3.Distance(returnStartPos, ctx.HandSocket.position);
            if (totalDistance > 0f)
            {
                progress += currentSpeed * Time.deltaTime / totalDistance;
            }
            progress = Mathf.Clamp01(progress);

            // Recompute control point each frame so the arc adapts to player movement
            bezierControlPoint = BezierUtils.ComputeArcControlPoint(
                returnStartPos,
                ctx.HandSocket.position,
                ctx.Config.ArcHeightRatio,
                ctx.Config.ArcSideRatio
            );

            Vector3 targetPos = BezierUtils.QuadraticBezier(
                returnStartPos,
                bezierControlPoint,
                ctx.HandSocket.position,
                progress
            );
            ctx.Transform.position = targetPos;

            Vector3 toHand = ctx.HandSocket.position - ctx.Transform.position;
            if (toHand.sqrMagnitude > 0.001f)
            {
                ctx.Transform.forward = toHand.normalized;
            }

            ctx.HitDetector.CheckOverlap(ctx.Transform.position);

            float distToHand = Vector3.Distance(ctx.Transform.position, ctx.HandSocket.position);
            if (distToHand <= ctx.Config.CatchDistance || progress >= 1f)
            {
                EventBus.Publish(new WeaponCaughtEvent
                {
                    Weapon = ctx.Transform.gameObject
                });

                ctx.StateMachine.ChangeState(WeaponStateKey.Equipped);
            }
        }

        public void FixedUpdate() { }
        public void Exit() { }
    }
}
