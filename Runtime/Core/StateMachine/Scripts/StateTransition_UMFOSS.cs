using System;

namespace GameplayMechanicsUMFOSS.Core
{
    /// <summary>A single rule: if condition is true, move to targetState.</summary>
    public class StateTransition_UMFOSS
    {
        private readonly Func<bool>    condition;
        private readonly IState_UMFOSS targetState;

        /// <summary>
        /// Creates a transition rule.
        /// </summary>
        /// <param name="condition">Lambda evaluated every frame. Example: () => isGrounded</param>
        /// <param name="targetState">State to activate when condition returns true.</param>
        public StateTransition_UMFOSS(Func<bool> condition, IState_UMFOSS targetState)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (targetState == null)
                throw new ArgumentNullException(nameof(targetState));

            this.condition   = condition;
            this.targetState = targetState;
        }

        /// <summary>Returns true when the machine should switch to the target state.</summary>
        public bool ShouldTransition() => condition();

        /// <summary>The state this transition leads to.</summary>
        public IState_UMFOSS GetTargetState() => targetState;
    }
}
