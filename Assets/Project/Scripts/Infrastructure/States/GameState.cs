using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Infrastructure.Services.SceneLoading;
using SelfBulletting.Infrastructure.StateMachine;
using SelfBulletting.UI;

namespace SelfBulletting.Infrastructure.States
{
    /// <summary>
    /// Loads a fresh Game scene with the current level and runs one round. The scene stays loaded under the final screen;
    /// <see cref="FinalState"/> unloads it.
    /// </summary>
    public sealed class GameState : IState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _curtain;
        private readonly GameDependencies _dependencies;
        private readonly LevelCatalog _levels;
        private readonly LevelProgress _progress;

        private CancellationTokenSource _roundCancellation;

        public GameState(AppStateMachine stateMachine, SceneLoader sceneLoader, LoadingCurtain curtain,
            GameDependencies dependencies, LevelCatalog levels, LevelProgress progress)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
            _dependencies = dependencies;
            _levels = levels;
            _progress = progress;
        }

        public async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            await _curtain.ShowAsync(cancellationToken);
            await _sceneLoader.LoadAsync(SceneNames.Game, _curtain, cancellationToken);

            GameEntryPoint game = _sceneLoader.FindEntryPoint<GameEntryPoint>(SceneNames.Game);
            game.Construct(_dependencies, _levels.Get(_progress.CurrentIndex));

            await _curtain.HideAsync(cancellationToken);

            _roundCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            PlayRoundAsync(game, _roundCancellation.Token).Forget();
        }

        public UniTask ExitAsync(CancellationToken cancellationToken)
        {
            _roundCancellation?.Cancel();
            _roundCancellation?.Dispose();
            _roundCancellation = null;
            return UniTask.CompletedTask;
        }

        private async UniTaskVoid PlayRoundAsync(GameEntryPoint game, CancellationToken cancellationToken)
        {
            try
            {
                GameResult result = await game.PlayRoundAsync(cancellationToken);
                _stateMachine.Enter<FinalState, FinalPayload>(new FinalPayload(result, game));
            }
            catch (OperationCanceledException)
            {
                // The round was interrupted by leaving the state or unloading the scene.
            }
        }
    }
}
