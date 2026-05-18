namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Interface for a state in a finite state machine.
    /// Each state handles its own enter, update, and exit logic.
    /// </summary>
    public interface IState
    {
        /// <summary>Called once when entering this state.</summary>
        void Enter();

        /// <summary>Called every frame while this state is active.</summary>
        void Update();

        /// <summary>Called every physics step while this state is active.</summary>
        void FixedUpdate();

        /// <summary>Called once when leaving this state.</summary>
        void Exit();
    }
}
