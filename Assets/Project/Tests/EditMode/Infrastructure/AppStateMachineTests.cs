using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using SelfBulletting.Infrastructure.StateMachine;
using UnityEngine;
using UnityEngine.TestTools;

namespace SelfBulletting.Tests.Infrastructure
{
    public sealed class AppStateMachineTests
    {
        private List<string> _journal;
        private AppStateMachine _machine;
        private StateA _stateA;
        private StateB _stateB;
        private PayloadState _payloadState;

        [SetUp]
        public void SetUp()
        {
            _journal = new List<string>();
            _machine = new AppStateMachine();
            _stateA = new StateA(_journal);
            _stateB = new StateB(_journal);
            _payloadState = new PayloadState();

            _machine.Register(_stateA);
            _machine.Register(_stateB);
            _machine.Register(_payloadState);
        }

        [TearDown]
        public void TearDown()
        {
            _machine.Dispose();
        }

        [Test]
        public void Enter_EntersRequestedState()
        {
            _machine.Enter<StateA>();

            Assert.AreEqual(1, _stateA.EnterCount);
            Assert.AreSame(_stateA, _machine.Current);
            Assert.IsFalse(_machine.IsTransitioning);
        }

        [Test]
        public void Enter_ExitsPreviousStateBeforeEnteringNext()
        {
            _machine.Enter<StateA>();
            _machine.Enter<StateB>();

            CollectionAssert.AreEqual(new[] { "A.Enter", "A.Exit", "B.Enter" }, _journal);
        }

        [Test]
        public void Enter_WithPayload_PassesPayloadToState()
        {
            _machine.Enter<PayloadState, int>(42);

            Assert.AreEqual(42, _payloadState.ReceivedPayload);
        }

        [Test]
        public void Enter_SameStateDuringTransition_IsIgnored()
        {
            _stateA.EnterGate = new UniTaskCompletionSource();
            _machine.Enter<StateA>();

            LogAssert.Expect(LogType.Warning, new Regex("Ignored transition to StateA"));
            _machine.Enter<StateA>();
            _stateA.EnterGate.TrySetResult();

            Assert.AreEqual(1, _stateA.EnterCount);
            Assert.AreEqual(0, _stateA.ExitCount);
        }

        [Test]
        public void Enter_OtherStateDuringTransition_RunsAfterCurrentTransition()
        {
            _stateA.EnterGate = new UniTaskCompletionSource();
            _machine.Enter<StateA>();

            _machine.Enter<StateB>();
            Assert.AreEqual(0, _stateB.EnterCount);

            _stateA.EnterGate.TrySetResult();

            Assert.AreSame(_stateB, _machine.Current);
            CollectionAssert.AreEqual(new[] { "A.Enter", "A.Exit", "B.Enter" }, _journal);
        }

        [Test]
        public void Enter_ThirdRequestDuringTransition_IsIgnored()
        {
            _stateA.EnterGate = new UniTaskCompletionSource();
            _machine.Enter<StateA>();
            _machine.Enter<StateB>();

            LogAssert.Expect(LogType.Warning, new Regex("Ignored transition to PayloadState"));
            _machine.Enter<PayloadState, int>(7);
            _stateA.EnterGate.TrySetResult();

            Assert.AreSame(_stateB, _machine.Current);
            Assert.AreEqual(0, _payloadState.EnterCount);
        }

        [Test]
        public void Enter_RequestedFromInsideEnter_RunsRightAfterIt()
        {
            _stateA.OnEnter = () => _machine.Enter<StateB>();

            _machine.Enter<StateA>();

            Assert.AreSame(_stateB, _machine.Current);
        }

        [Test]
        public void Enter_AfterDispose_DoesNothing()
        {
            _machine.Dispose();

            _machine.Enter<StateA>();

            Assert.AreEqual(0, _stateA.EnterCount);
        }

        [Test]
        public void Enter_UnregisteredState_ThrowsAndKeepsCurrentState()
        {
            var machine = new AppStateMachine();
            machine.Register(_stateA);
            machine.Enter<StateA>();

            var exception = Assert.Throws<InvalidOperationException>(() => machine.Enter<StateB>());

            StringAssert.Contains("StateB is not registered", exception.Message);
            Assert.AreSame(_stateA, machine.Current);
            Assert.AreEqual(0, _stateA.ExitCount);
            machine.Dispose();
        }

        private abstract class JournalState : IState
        {
            private readonly List<string> _journal;
            private readonly string _name;

            protected JournalState(List<string> journal, string name)
            {
                _journal = journal;
                _name = name;
            }

            public int EnterCount { get; private set; }
            public int ExitCount { get; private set; }
            public UniTaskCompletionSource EnterGate { get; set; }
            public Action OnEnter { get; set; }

            public UniTask EnterAsync(CancellationToken cancellationToken)
            {
                EnterCount++;
                _journal.Add($"{_name}.Enter");
                OnEnter?.Invoke();
                return EnterGate?.Task ?? UniTask.CompletedTask;
            }

            public UniTask ExitAsync(CancellationToken cancellationToken)
            {
                ExitCount++;
                _journal.Add($"{_name}.Exit");
                return UniTask.CompletedTask;
            }
        }

        private sealed class StateA : JournalState
        {
            public StateA(List<string> journal) : base(journal, "A")
            {
            }
        }

        private sealed class StateB : JournalState
        {
            public StateB(List<string> journal) : base(journal, "B")
            {
            }
        }

        private sealed class PayloadState : IPayloadState<int>
        {
            public int EnterCount { get; private set; }
            public int ReceivedPayload { get; private set; }

            public UniTask EnterAsync(int payload, CancellationToken cancellationToken)
            {
                EnterCount++;
                ReceivedPayload = payload;
                return UniTask.CompletedTask;
            }

            public UniTask ExitAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;
        }
    }
}
