using System;
using UnityEngine;

namespace SelfBulletting.Infrastructure.Services.Audio
{
    /// <summary>
    /// One sound of the <see cref="SoundLibrary"/>: the clip and how it plays.
    /// </summary>
    [Serializable]
    public sealed class SoundEntry
    {
        [SerializeField] private SoundId _id;
        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        [Tooltip("Random pitch offset (±) on every play, so a sound repeated many times does not sound mechanical.")]
        [SerializeField, Range(0f, 0.5f)] private float _pitchVariation;

        [Tooltip("How many copies of this sound may play at once. Extra plays are skipped.")]
        [SerializeField, Min(1)] private int _maxVoices = 1;

        [Tooltip("A play closer than this to the previous one of the same sound is skipped.")]
        [SerializeField, Min(0f)] private float _minIntervalSeconds;

        public SoundId Id => _id;
        public AudioClip Clip => _clip;
        public float Volume => _volume;
        public float PitchVariation => _pitchVariation;
        public int MaxVoices => _maxVoices;
        public float MinIntervalSeconds => _minIntervalSeconds;
    }
}
