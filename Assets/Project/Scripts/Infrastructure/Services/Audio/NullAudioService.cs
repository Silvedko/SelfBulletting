namespace SelfBulletting.Infrastructure.Services.Audio
{
    public sealed class NullAudioService : IAudioService
    {
        public void Play(SoundId sound)
        {
        }

        public void StartLoop(SoundId sound)
        {
        }

        public void StopLoop(SoundId sound)
        {
        }
    }
}
