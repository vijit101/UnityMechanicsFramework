using UnityEngine;
using GameplayMechanicsUMFOSS.Adapters;

namespace GameplayMechanicsUMFOSS.Adapters
{
    public interface IInputAdapter
    {
        Vector2 GetMovementInput();
    }

    public interface IPhysicsAdapter
    {
        void SetVelocity(Rigidbody2D rb2d, Vector2 velocity);
        void AddForce(Rigidbody2D rb2d, Vector2 force, ForceMode2D mode);
        void MovePosition(Rigidbody2D rb2d, Vector2 position);
    }

    public interface IEventBus
    {
        void Publish<T>(T eventData);
        void Subscribe<T>(System.Action<T> handler);
        void Unsubscribe<T>(System.Action<T> handler);
    }
}
