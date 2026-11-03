using System;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay;
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

        private AppStateMachine _stateMachine;
        private IVibrationService _vibration;

        private void Awake()
        {
            Application.targetFrameRate = _settings.Application.TargetFrameRate;

            var sceneLoader = new SceneLoader();
            ISaveService saveService = new PlayerPrefsSaveService();
            IInputService input = new PointerInputService();
            IAudioService audio = new NullAudioService();
            _vibration = CreateVibrationService();

            var gameDependencies = new GameDependencies(_settings, input, _vibration, audio);

            _stateMachine = new AppStateMachine();
            _stateMachine.Register(new LoadingState(_stateMachine, sceneLoader, _loadingCurtain, _settings.Loading));
            _stateMachine.Register(new MenuState(_stateMachine, sceneLoader, _loadingCurtain, saveService, audio));
            _stateMachine.Register(new GameState(_stateMachine, sceneLoader, _loadingCurtain, gameDependencies));
            _stateMachine.Register(new FinalState(_stateMachine, sceneLoader, _loadingCurtain, saveService, audio));
        }

        private void Start()
        {
            _stateMachine.Enter<LoadingState>();
        }

        private void OnDestroy()
        {
            _stateMachine?.Dispose();
            (_vibration as IDisposable)?.Dispose();
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
