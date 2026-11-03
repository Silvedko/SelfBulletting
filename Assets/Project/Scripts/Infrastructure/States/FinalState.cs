using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Infrastructure.Services.Audio;
using SelfBulletting.Infrastructure.Services.Save;
using SelfBulletting.Infrastructure.Services.SceneLoading;
using SelfBulletting.Infrastructure.StateMachine;
using SelfBulletting.UI;
using SelfBulletting.UI.Final;

namespace SelfBulletting.Infrastructure.States
{
    /// <summary>
    /// Credits the round score, moves to the next level after a win and waits for Play (Next or Retry) or Menu.
    /// Leaving the state unloads the Game scene, so the next round goes through the same fresh load as entering from the menu.
    /// </summary>
    public sealed class FinalState : IPayloadState<FinalPayload>
    {
        private readonly AppStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _curtain;
        private readonly ISaveService _saveService;
        private readonly LevelProgress _progress;
        private readonly IAudioService _audio;

        private FinalScreen _screen;

        public FinalState(AppStateMachine stateMachine, SceneLoader sceneLoader, LoadingCurtain curtain,
            ISaveService saveService, LevelProgress progress, IAudioService audio)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
            _saveService = saveService;
            _progress = progress;
            _audio = audio;
        }

        public async UniTask EnterAsync(FinalPayload payload, CancellationToken cancellationToken)
        {
            _saveService.AddToTotalScore(payload.Result.Score.Total);
            if (payload.Result.IsWin)
            {
                _progress.CompleteCurrent();
            }

            _screen = payload.Game.FinalScreen;
            _screen.PlayRequested += OnPlayRequested;
            _screen.MenuRequested += OnMenuRequested;

            await _screen.ShowAsync(payload.Result, cancellationToken);
        }

        public async UniTask ExitAsync(CancellationToken cancellationToken)
        {
            _screen.PlayRequested -= OnPlayRequested;
            _screen.MenuRequested -= OnMenuRequested;
            _screen = null;

            await _curtain.ShowAsync(cancellationToken);
            await _sceneLoader.UnloadAsync(SceneNames.Game, cancellationToken);
        }

        // After a win the current level is already the next one, so the same button means Next; after a loss, Retry.
        private void OnPlayRequested()
        {
            _audio.Play(SoundId.UiClick);
            _stateMachine.Enter<GameState>();
        }

        private void OnMenuRequested()
        {
            _audio.Play(SoundId.UiClick);
            _stateMachine.Enter<MenuState>();
        }
    }
}
