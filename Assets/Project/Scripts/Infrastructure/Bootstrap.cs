using System;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Infrastructure.Services.Audio;
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
        [SerializeField] private SettingsConfig _settings;
        [SerializeField] private LoadingCurtain _loadingCurtain;
        [SerializeField] private SoundLibrary _soundLibrary;
        [SerializeField] private LevelCatalog _levelCatalog;

        private AppStateMachine _stateMachine;
        private IVibrationService _vibration;
        private IAudioService _audio;

        private void Awake()
        {
            Application.targetFrameRate = _settings.Application.TargetFrameRate;
            _loadingCurtain.Construct(_settings.Loading);

            var sceneLoader = new SceneLoader();
            ISaveService saveService = new PlayerPrefsSaveService();
            IInputService input = new PointerInputService();
            _audio = CreateAudioService();
            _vibration = CreateVibrationService();

            var gameDependencies = new GameDependencies(_settings, input, _vibration, _audio);
            var levelProgress = new LevelProgress(saveService, _levelCatalog.Count);

            _stateMachine = new AppStateMachine();
            _stateMachine.Register(new LoadingState(_stateMachine, sceneLoader, _loadingCurtain, _settings.Loading));
            _stateMachine.Register(new MenuState(_stateMachine, sceneLoader, _loadingCurtain, saveService, levelProgress, _audio));
            _stateMachine.Register(new GameState(_stateMachine, sceneLoader, _loadingCurtain, gameDependencies, _levelCatalog, levelProgress));
            _stateMachine.Register(new FinalState(_stateMachine, sceneLoader, _loadingCurtain, saveService, levelProgress, _audio));
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
