using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SelfBulletting.Infrastructure.StateMachine
{
    /// <summary>
    /// Runs one transition at a time. While a transition runs, the machine remembers at most one
    /// follow-up request (so a state can move on from its own Enter); a repeated request for the
    /// state being entered, or any third request, is ignored.
    /// </summary>
    public sealed class AppStateMachine : IDisposable
    {
        private readonly Dictionary<Type, IExitableState> _states = new Dictionary<Type, IExitableState>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private Transition _running;
        private Transition _pending;
        private bool _isDisposed;

        public IExitableState Current { get; private set; }
        public bool IsTransitioning => _running != null;

        public void Register(IExitableState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            _states[state.GetType()] = state;
        }

        public void Enter<TState>() where TState : class, IState
        {
            Request(new Transition(typeof(TState), token => Resolve<TState>().EnterAsync(token)));
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadState<TPayload>
        {
            Request(new Transition(typeof(TState), token => Resolve<TState>().EnterAsync(payload, token)));
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            _pending = null;
            _lifetime.Cancel();
            _lifetime.Dispose();
        }

        private void Request(Transition transition)
        {
            if (_isDisposed)
            {
                return;
            }

            // Fail before the current state is exited, so the machine never ends up in a closed state.
            if (!_states.ContainsKey(transition.Target))
            {
                throw new InvalidOperationException($"State {transition.Target.Name} is not registered.");
            }

            if (_running == null)
            {
                RunAsync(transition).Forget();
                return;
            }

            if (_running.Target == transition.Target || _pending != null)
            {
                Debug.LogWarning($"[{nameof(AppStateMachine)}] Ignored transition to {transition.Target.Name}: " +
                                 $"transition to {_running.Target.Name} is in progress.");
                return;
            }

            _pending = transition;
        }

        private async UniTaskVoid RunAsync(Transition transition)
        {
            CancellationToken token = _lifetime.Token;

            try
            {
                while (transition != null)
                {
                    _running = transition;

                    if (Current != null)
                    {
                        await Current.ExitAsync(token);
                    }

                    Current = _states[transition.Target];
                    await transition.EnterAsync(token);

                    transition = _pending;
                    _pending = null;
                }
            }
            catch (OperationCanceledException)
            {
                // The machine was disposed in the middle of a transition.
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                _running = null;
            }
        }

        private TState Resolve<TState>() where TState : class, IExitableState
        {
            return (TState)_states[typeof(TState)];
        }

        private sealed class Transition
        {
            private readonly Func<CancellationToken, UniTask> _enter;

            public Transition(Type target, Func<CancellationToken, UniTask> enter)
            {
                Target = target;
                _enter = enter;
            }

            public Type Target { get; }

            public UniTask EnterAsync(CancellationToken cancellationToken) => _enter(cancellationToken);
        }
    }
}
