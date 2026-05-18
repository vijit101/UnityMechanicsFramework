using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>
    /// Generic finite state machine. Plain C# class — not a MonoBehaviour.
    /// Call Tick() from Update() and FixedTick() from FixedUpdate().
    /// </summary>
    public class StateMachine_UMFOSS
    {
        private IState_UMFOSS currentState;
        private IState_UMFOSS previousState;

        private readonly Dictionary<IState_UMFOSS, List<StateTransition_UMFOSS>> transitions
            = new Dictionary<IState_UMFOSS, List<StateTransition_UMFOSS>>();

        // fires from any active state — use for death, stun, pause
        private readonly List<StateTransition_UMFOSS> anyTransitions
            = new List<StateTransition_UMFOSS>();

        private List<StateTransition_UMFOSS> currentTransitions
            = new List<StateTransition_UMFOSS>();

        private static readonly List<StateTransition_UMFOSS> EmptyTransitions
            = new List<StateTransition_UMFOSS>(0);

        private readonly StateHistory_UMFOSS history = new StateHistory_UMFOSS();
        private float stateEnteredTime;

        // the MonoBehaviour that owns this machine — used as event owner identifier
        private readonly object owner;

        public StateMachine_UMFOSS(object owner = null)
        {
            this.owner = owner;
        }

        /// <summary>The state currently running.</summary>
        public IState_UMFOSS CurrentState  => currentState;

        /// <summary>The state that was active before the last transition.</summary>
        public IState_UMFOSS PreviousState => previousState;

        /// <summary>Read-only access to state history.</summary>
        public StateHistory_UMFOSS History => history;

        /// <summary>Call from MonoBehaviour.Update(). Checks transitions then ticks current state.</summary>
        public void Tick()
        {
            var triggered = GetTriggeredTransition();
            if (triggered != null)
                ChangeState(triggered.GetTargetState());

            currentState?.OnTick();
        }

        /// <summary>Call from MonoBehaviour.FixedUpdate(). Ticks physics on current state.</summary>
        public void FixedTick()
        {
            currentState?.OnFixedTick();
        }

        /// <summary>
        /// Switches to newState. Calls OnExit on current, then OnEnter on new.
        /// Does nothing if newState is already the current state.
        /// </summary>
        public void ChangeState(IState_UMFOSS newState)
        {
            if (newState == null)
                throw new ArgumentNullException(nameof(newState));

            if (newState == currentState)
                return;

            var previousName = currentState?.GetType().Name ?? string.Empty;
            var duration     = Time.time - stateEnteredTime;

            currentState?.OnExit();

            EventBus_UMFOSS.Publish(new StateExitedEvent
            {
                stateName = previousName,
                duration  = duration,
                owner     = owner
            });

            history.MarkCurrentExited();

            previousState    = currentState;
            currentState     = newState;
            stateEnteredTime = Time.time;

            transitions.TryGetValue(currentState, out currentTransitions);
            if (currentTransitions == null)
                currentTransitions = EmptyTransitions;

            history.Record(currentState);

            currentState.OnEnter();

            EventBus_UMFOSS.Publish(new StateEnteredEvent
            {
                stateName = currentState.GetType().Name,
                owner     = owner
            });

            EventBus_UMFOSS.Publish(new StateChangedEvent
            {
                previousStateName = previousName,
                newStateName      = currentState.GetType().Name,
                timestamp         = stateEnteredTime,
                owner             = owner
            });
        }

        /// <summary>Adds a transition from one specific state to another.</summary>
        public void AddTransition(IState_UMFOSS from, IState_UMFOSS to, Func<bool> condition)
        {
            if (!transitions.ContainsKey(from))
                transitions[from] = new List<StateTransition_UMFOSS>();

            transitions[from].Add(new StateTransition_UMFOSS(condition, to));
        }

        /// <summary>Adds a transition that can fire from any active state.</summary>
        public void AddAnyTransition(IState_UMFOSS to, Func<bool> condition)
        {
            anyTransitions.Add(new StateTransition_UMFOSS(condition, to));
        }

        // anyTransitions checked first — death/stun must interrupt everything
        private StateTransition_UMFOSS GetTriggeredTransition()
        {
            foreach (var t in anyTransitions)
                if (t.ShouldTransition()) return t;

            foreach (var t in currentTransitions)
                if (t.ShouldTransition()) return t;

            return null;
        }
    }
}
