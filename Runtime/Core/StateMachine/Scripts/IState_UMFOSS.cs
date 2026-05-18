namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>Contract every state must implement.</summary>
    public interface IState_UMFOSS
    {
        /// <summary>Runs once when this state becomes active.</summary>
        void OnEnter();

        /// <summary>Runs every frame — input, timers, logic.</summary>
        void OnTick();

        /// <summary>Runs every fixed update — rigidbody, physics.</summary>
        void OnFixedTick();

        /// <summary>Runs once just before this state is replaced.</summary>
        void OnExit();
    }
}
