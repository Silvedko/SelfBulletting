namespace SelfBulletting.Infrastructure.Services.Audio
{
    /// <summary>
    /// The slot for sound. Gameplay already calls it; a real implementation replaces
    /// <see cref="NullAudioService"/> in the bootstrap without touching gameplay code.
    /// </summary>
    public interface IAudioService
    {
        void Play(SoundId sound);

        void StartLoop(SoundId sound);

        void StopLoop(SoundId sound);
    }
}
