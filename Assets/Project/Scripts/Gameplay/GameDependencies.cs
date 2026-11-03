using SelfBulletting.Configs;
using SelfBulletting.Infrastructure.Services.Audio;
using SelfBulletting.Infrastructure.Services.Input;
using SelfBulletting.Infrastructure.Services.Vibration;

namespace SelfBulletting.Gameplay
{
    /// <summary>
    /// App services the Game scene needs, handed over in one piece by the game state.
    /// </summary>
    public sealed class GameDependencies
    {
        public GameDependencies(SettingsConfig settings, IInputService input, IVibrationService vibration, IAudioService audio)
        {
            Settings = settings;
            Input = input;
            Vibration = vibration;
            Audio = audio;
        }

        public SettingsConfig Settings { get; }
        public IInputService Input { get; }
        public IVibrationService Vibration { get; }
        public IAudioService Audio { get; }
    }
}
