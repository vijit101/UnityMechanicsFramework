using System.Collections.Generic;
using NUnit.Framework;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Tests
{
    // Verifies Person 1 acceptance criteria:
    // - correct OnExit → OnEnter call order on transition
    // - self-transition does nothing
    // - anyTransitions fire before per-state transitions
    // - PreviousState is set correctly
    // - Tick() is safe with no transitions registered
    public class StateMachineOrderTest_UMFOSS
    {
        // records every method call so we can assert order
        private List<string> callLog;

        private class RecordingState : IState_UMFOSS
        {
            private readonly string name;
            private readonly List<string> log;

            public RecordingState(string name, List<string> log)
            {
                this.name = name;
                this.log  = log;
            }

            public void OnEnter()     => log.Add($"{name}.OnEnter");
            public void OnTick()      => log.Add($"{name}.OnTick");
            public void OnFixedTick() => log.Add($"{name}.OnFixedTick");
            public void OnExit()      => log.Add($"{name}.OnExit");
        }

        [SetUp]
        public void SetUp()
        {
            callLog = new List<string>();
        }

        [Test]
        public void ChangeState_CallsOnExitThenOnEnterInOrder()
        {
            var fsm = new StateMachine_UMFOSS();
            var stateA = new RecordingState("A", callLog);
            var stateB = new RecordingState("B", callLog);

            fsm.ChangeState(stateA);
            callLog.Clear(); // clear initial enter

            fsm.ChangeState(stateB);

            Assert.AreEqual("A.OnExit",  callLog[0], "OnExit should fire before OnEnter");
            Assert.AreEqual("B.OnEnter", callLog[1], "OnEnter should fire after OnExit");
            Assert.AreEqual(2, callLog.Count);
        }

        [Test]
        public void ChangeState_ToSameState_DoesNothing()
        {
            var fsm    = new StateMachine_UMFOSS();
            var stateA = new RecordingState("A", callLog);

            fsm.ChangeState(stateA);
            callLog.Clear();

            fsm.ChangeState(stateA); // self-transition

            Assert.AreEqual(0, callLog.Count, "Self-transition must not call OnExit or OnEnter");
        }

        [Test]
        public void PreviousState_IsSetBeforeNewStateEnters()
        {
            var fsm    = new StateMachine_UMFOSS();
            var stateA = new RecordingState("A", callLog);
            var stateB = new RecordingState("B", callLog);

            fsm.ChangeState(stateA);
            fsm.ChangeState(stateB);

            Assert.AreEqual(stateA, fsm.PreviousState, "PreviousState should be A after moving to B");
            Assert.AreEqual(stateB, fsm.CurrentState,  "CurrentState should be B");
        }

        [Test]
        public void Tick_WithNoTransitions_DoesNotThrow()
        {
            var fsm    = new StateMachine_UMFOSS();
            var stateA = new RecordingState("A", callLog);

            fsm.ChangeState(stateA);
            callLog.Clear();

            Assert.DoesNotThrow(() => fsm.Tick());
            Assert.IsTrue(callLog.Contains("A.OnTick"), "OnTick should still fire with no transitions");
        }

        [Test]
        public void AnyTransition_FiresBeforePerStateTransition()
        {
            var fsm      = new StateMachine_UMFOSS();
            var stateA   = new RecordingState("A", callLog);
            var stateB   = new RecordingState("B", callLog);
            var deadState = new RecordingState("Dead", callLog);

            bool deadFlag    = false;
            bool normalFlag  = false;

            fsm.AddTransition(stateA, stateB,    () => normalFlag);
            fsm.AddAnyTransition(deadState,       () => deadFlag);

            fsm.ChangeState(stateA);
            callLog.Clear();

            // fire both flags at the same time
            deadFlag   = true;
            normalFlag = true;

            fsm.Tick();

            // anyTransition must win — should go to Dead, not B
            Assert.AreEqual(deadState, fsm.CurrentState,
                "anyTransition to Dead must override per-state transition to B");
        }

        [Test]
        public void FixedTick_CallsOnFixedTickOnCurrentState()
        {
            var fsm    = new StateMachine_UMFOSS();
            var stateA = new RecordingState("A", callLog);

            fsm.ChangeState(stateA);
            callLog.Clear();

            fsm.FixedTick();

            Assert.IsTrue(callLog.Contains("A.OnFixedTick"));
        }
    }
}
