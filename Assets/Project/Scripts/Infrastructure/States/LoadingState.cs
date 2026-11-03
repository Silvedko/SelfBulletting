using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Configs;
using SelfBulletting.Infrastructure.Services.SceneLoading;
using SelfBulletting.Infrastructure.StateMachine;
using SelfBulletting.UI;

namespace SelfBulletting.Infrastructure.States
{
    public sealed class LoadingState : IState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _curtain;
        private readonly LoadingSettings _settings;

        public LoadingState(AppStateMachine stateMachine, SceneLoader sceneLoader, LoadingCurtain curtain, LoadingSettings settings)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
            _settings = settings;
        }

        public async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            _curtain.Show();

            UniTask minimumSplash = UniTask.Delay(TimeSpan.FromSeconds(_settings.MinDurationSeconds), cancellationToken: cancellationToken);
            UniTask menuLoading = _sceneLoader.LoadAsync(SceneNames.Menu, _curtain, cancellationToken);
            await UniTask.WhenAll(minimumSplash, menuLoading);

            _stateMachine.Enter<MenuState>();
        }

        public UniTask ExitAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}
