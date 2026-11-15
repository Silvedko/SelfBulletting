using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Modes;
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
        private readonly ModeLevels _levels;
        private readonly IAudioService _audio;

        private MenuEntryPoint _menu;

        public MenuState(AppStateMachine stateMachine, SceneLoader sceneLoader, LoadingCurtain curtain,
            ISaveService saveService, ModeLevels levels, IAudioService audio)
        {
            _stateMachine = stateMachine;
            _sceneLoader = sceneLoader;
            _curtain = curtain;
            _saveService = saveService;
            _levels = levels;
            _audio = audio;
        }

        public async UniTask EnterAsync(CancellationToken cancellationToken)
        {
            await _curtain.ShowAsync(cancellationToken);
            await _sceneLoader.LoadAsync(SceneNames.Menu, _curtain, cancellationToken);

            _menu = _sceneLoader.FindEntryPoint<MenuEntryPoint>(SceneNames.Menu);
            foreach (GameMode mode in ModeLevels.Modes)
            {
                _menu.ShowMode(mode, _saveService.GetTotalScore(mode), _levels.GetProgress(mode).CurrentNumber);
            }

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

        private void OnPlayRequested(GameMode mode)
        {
            _audio.Play(SoundId.UiClick);
            _stateMachine.Enter<GameState, GameMode>(mode);
        }
    }
}
