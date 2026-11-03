using UnityEngine;

namespace SelfBulletting.Infrastructure.Services.Audio
{
    /// <summary>
    /// One AudioSource of the one-shot pool and the sound it is playing.
    /// </summary>
    public sealed class SoundVoice
    {
        private readonly AudioSource _source;

        public SoundVoice(AudioSource source)
        {
            _source = source;
        }

        public SoundId Sound { get; private set; }
        public float StartTime { get; private set; }
        public bool IsPlaying => _source.isPlaying;

        public bool IsPlayingSound(SoundId sound) => IsPlaying && Sound == sound;

        public void Play(SoundEntry entry, float pitch, float time)
        {
            Sound = entry.Id;
            StartTime = time;

            _source.Stop();
            _source.clip = entry.Clip;
            _source.volume = entry.Volume;
            _source.pitch = pitch;
            _source.Play();
        }
    }
}
