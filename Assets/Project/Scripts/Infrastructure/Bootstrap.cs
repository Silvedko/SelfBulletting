using System;
using SelfBulletting.Configs;
using SelfBulletting.Configs.Modes;
using SelfBulletting.Gameplay;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Modes;
using SelfBulletting.Infrastructure.Services.Audio;
using SelfBulletting.Infrastructure.Services.Bonus;
using SelfBulletting.Infrastructure.Services.Input;
using SelfBulletting.Infrastructure.Services.Save;
using SelfBulletting.Infrastructure.Services.SceneLoading;
using SelfBulletting.Infrastructure.Services.Vibration;
using SelfBulletting.Infrastructure.StateMachine;
using SelfBulletting.Infrastructure.States;
using SelfBulletting.UI;
using UnityEngine;

namespace SelfBulletting.Infrastructure
{
    /// <summary>
    /// Composition root of the app. Lives in the Boot scene, which is never unloaded.
    /// Creates every app service by hand and passes them to the states through constructors.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        [Tooltip("Values shared by every game mode.")]
        [SerializeField] private SettingsConfig _settings;

        [Tooltip("One config for every game mode.")]
        [SerializeField] private ModeConfig[] _modeConfigs;

        [SerializeField] private LoadingCurtain _loadingCurtain;
        [SerializeField] private SoundLibrary _soundLibrary;
        [Tooltip("Levels of the Classic mode.")]
        [SerializeField] private LevelCatalog _levelCatalog;

        [Tooltip("Curved levels shared by Slingshot, Arrow and Hose.")]
        [SerializeField] private LevelCatalog _curvedLevelCatalog;

        private AppStateMachine _stateMachine;
        private IVibrationService _vibration;
        private IAudioService _audio;

        private void Awake()
        {
            // On the web the browser paces frames (requestAnimationFrame); a fixed rate there only adds judder.
#if !UNITY_WEBGL || UNITY_EDITOR
            Application.targetFrameRate = _settings.Application.TargetFrameRate;
#endif
            _loadingCurtain.Construct(_settings.Loading);

            var sceneLoader = new SceneLoader();
            ISaveService saveService = new PlayerPrefsSaveService();
            IInputService input = new PointerInputService();
            _audio = CreateAudioService();
            _vibration = CreateVibrationService();

            var modes = new ModeConfigs(_modeConfigs);
            IBonusService bonus = new BonusService(modes);
            var gameDependencies = new GameDependencies(_settings, input, _vibration, _audio, bonus);
            var levels = new ModeLevels(_levelCatalog, _curvedLevelCatalog, saveService);

            _stateMachine = new AppStateMachine();
            _stateMachine.Register(new LoadingState(_stateMachine, sceneLoader, _loadingCurtain, _settings.Loading));
            _stateMachine.Register(new MenuState(_stateMachine, sceneLoader, _loadingCurtain, saveService, levels, _audio));
            _stateMachine.Register(new GameState(_stateMachine, sceneLoader, _loadingCurtain, gameDependencies, levels, modes));
            _stateMachine.Register(new FinalState(_stateMachine, sceneLoader, _loadingCurtain, saveService, levels, _audio));
        }

        private void Start()
        {
            _stateMachine.Enter<LoadingState>();
        }

        private void OnDestroy()
        {
            _stateMachine?.Dispose();
            (_vibration as IDisposable)?.Dispose();
            (_audio as IDisposable)?.Dispose();
        }

        private IAudioService CreateAudioService()
        {
            if (_soundLibrary == null)
            {
                Debug.LogWarning($"{nameof(SoundLibrary)} is not assigned, the game runs without sound.", this);
                return new NullAudioService();
            }

            // The sources go under this object, so they live in the Boot scene for the whole app.
            return new AudioService(_soundLibrary, transform);
        }

        private static IVibrationService CreateVibrationService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidVibrationService();
#else
            return new NullVibrationService();
#endif
        }
    }
}
