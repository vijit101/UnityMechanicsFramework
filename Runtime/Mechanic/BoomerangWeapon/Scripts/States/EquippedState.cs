using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Combat.States
{
    /// <summary>
    /// Weapon is parented to the player's hand socket.
    /// Physics disabled, collider off, waiting for a throw command.
    /// </summary>
    public class EquippedState : IState
    {
        private readonly BoomerangContext ctx;

        public EquippedState(BoomerangContext context)
        {
            ctx = context;
        }

        public void Enter()
        {
            ctx.Physics.SetKinematic(true);
            ctx.WeaponCollider.enabled = false;

            ctx.Transform.SetParent(ctx.HandSocket);
            ctx.Transform.localPosition = Vector3.zero;
            ctx.Transform.localRotation = Quaternion.identity;

            // Clear stale references from previous flight
            ctx.LastCollision = null;
            ctx.LastHurtboxHit = null;
            ctx.EmbeddedInCollider = null;

            ctx.VisualHandler.ResetRotation();
        }

        public void Update() { }
        public void FixedUpdate() { }
        public void Exit() { }
    }
}
