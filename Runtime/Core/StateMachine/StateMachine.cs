using System;
using System.Collections.Generic;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// A generic finite state machine that manages transitions between IState instances.
    /// States are registered by an enum key and transitioned via ChangeState.
    /// </summary>
    /// <typeparam name="TStateKey">An enum type used to identify states.</typeparam>
    public class StateMachine<TStateKey> where TStateKey : Enum
    {
        private readonly Dictionary<TStateKey, IState> states = new Dictionary<TStateKey, IState>();
        private IState currentStateInstance;
        private TStateKey currentStateKey;
        private bool hasState;

        /// <summary>The currently active state key.</summary>
        public TStateKey CurrentStateKey => currentStateKey;

        /// <summary>True if any state is currently active.</summary>
        public bool IsRunning => hasState;

        /// <summary>Fired after a state transition completes. Passes old and new state keys.</summary>
        public event Action<TStateKey, TStateKey> OnStateChanged;

        /// <summary>
        /// Registers a state instance under the given key.
        /// </summary>
        public void AddState(TStateKey key, IState state)
        {
            states[key] = state;
        }

        /// <summary>
        /// Transitions to the specified state. Exits the current state and enters the new one.
        /// Does nothing if already in the requested state.
        /// </summary>
        public void ChangeState(TStateKey newStateKey)
        {
            if (hasState && EqualityComparer<TStateKey>.Default.Equals(currentStateKey, newStateKey))
                return;

            TStateKey previousKey = currentStateKey;

            if (hasState)
            {
                currentStateInstance.Exit();
            }

            currentStateKey = newStateKey;
            currentStateInstance = states[newStateKey];
            hasState = true;
            currentStateInstance.Enter();

            OnStateChanged?.Invoke(previousKey, newStateKey);
        }

        /// <summary>Forwards the Update call to the current state.</summary>
        public void Update()
        {
            if (hasState) currentStateInstance.Update();
        }

        /// <summary>Forwards the FixedUpdate call to the current state.</summary>
        public void FixedUpdate()
        {
            if (hasState) currentStateInstance.FixedUpdate();
        }
    }
}
