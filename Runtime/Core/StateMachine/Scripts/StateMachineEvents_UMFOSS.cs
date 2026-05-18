namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>Published every time the state machine transitions to a new state.</summary>
    public struct StateChangedEvent
    {
        public string previousStateName;
        public string newStateName;
        public float  timestamp;
        public object owner;
    }

    /// <summary>Published when a state becomes active (after OnEnter runs).</summary>
    public struct StateEnteredEvent
    {
        public string stateName;
        public object owner;
    }

    /// <summary>Published when a state is about to be replaced (after OnExit runs).</summary>
    public struct StateExitedEvent
    {
        public string stateName;
        public float  duration;
        public object owner;
    }
}
