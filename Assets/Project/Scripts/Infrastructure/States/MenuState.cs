using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Infrastructure.Services.Audio;
using SelfBulletting.Infrastructure.Services.Save;
using SelfBulletting.Infrastructure.Services.SceneLoading;
using SelfBulletting.Infrastructure.StateMachine;
using SelfBulletting.UI;
using SelfBulletting.UI.Menu;

namespace SelfBulletting.Infrastructure.States
{
    public sealed class MenuState : IState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly SceneLoader _sceneLoader;
        private readonly LoadingCurtain _curtain;
        private readonly ISaveService _saveService;
        private readonly LevelProgress _progress;
        private readonly IAudioService _audio;

        private MenuEntryPoint _menu;

        public MenuState(AppStateMachine stateMachine, SceneLoader sceneLoader, LoadingCurtain curtain,
            ISaveService saveService, LevelProgress progress, IAudioService audio)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
            _saveService = saveService;
            _progress = progress;
            _audio = audio;
        }

        public async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            await _curtain.ShowAsync(cancellationToken);
            await _sceneLoader.LoadAsync(SceneNames.Menu, _curtain, cancellationToken);

            _menu = _sceneLoader.FindEntryPoint<MenuEntryPoint>(SceneNames.Menu);
            _menu.Construct(_saveService.TotalScore, _progress.CurrentNumber);
            _menu.PlayRequested += OnPlayRequested;

            await _curtain.HideAsync(cancellationToken);
        }

        public async UniTask ExitAsync(CancellationToken cancellationToken)
        {
            _menu.PlayRequested -= OnPlayRequested;
            _menu = null;

            await _curtain.ShowAsync(cancellationToken);
            await _sceneLoader.UnloadAsync(SceneNames.Menu, cancellationToken);
        }

        private void OnPlayRequested()
        {
            _audio.Play(SoundId.UiClick);
            _stateMachine.Enter<GameState>();
        }
    }
}
