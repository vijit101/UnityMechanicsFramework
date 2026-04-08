using UnityEngine;
using GameplayMechanicsUMFOSS.Adapters;

namespace GameplayMechanicsUMFOSS.Adapters
{
    public class UnityInputAdapter : IInputAdapter
    {
        public Vector2 GetMovementInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2(horizontal, vertical);
        }
    }

    public class UnityPhysicsAdapter : IPhysicsAdapter
    {
        public void SetVelocity(Rigidbody2D rb2d, Vector2 velocity)
        {
            rb2d.linearVelocity = velocity;
        }

        public void AddForce(Rigidbody2D rb2d, Vector2 force, ForceMode2D mode)
        {
            rb2d.AddForce(force, mode);
        }

        public void MovePosition(Rigidbody2D rb2d, Vector2 position)
        {
            rb2d.MovePosition(position);
        }
    }

    public class UnityEventBus : IEventBus
    {
        private System.Collections.Generic.Dictionary<System.Type, System.Delegate> eventHandlers = 
            new System.Collections.Generic.Dictionary<System.Type, System.Delegate>();

        public void Publish<T>(T eventData)
        {
            if (eventHandlers.TryGetValue(typeof(T), out var handlers))
            {
                handlers?.DynamicInvoke(eventData);
            }
        }

        public void Subscribe<T>(System.Action<T> handler)
        {
            if (eventHandlers.ContainsKey(typeof(T)))
            {
                eventHandlers[typeof(T)] = System.Delegate.Combine(eventHandlers[typeof(T)], handler);
            }
            else
            {
                eventHandlers[typeof(T)] = handler;
            }
        }

        public void Unsubscribe<T>(System.Action<T> handler)
        {
            if (eventHandlers.ContainsKey(typeof(T)))
            {
                eventHandlers[typeof(T)] = System.Delegate.Remove(eventHandlers[typeof(T)], handler);
            }
        }
    }
}
