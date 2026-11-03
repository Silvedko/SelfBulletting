using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SelfBulletting.Infrastructure.Services.Audio
{
    /// <summary>
    /// Every sound of the game (<see cref="SoundId"/> → clip and playback settings) and the size of the voice pool.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(SoundLibrary), menuName = "SelfBulletting/Sound Library")]
    public sealed class SoundLibrary : ScriptableObject
    {
        [Tooltip("AudioSources for one-shot sounds. When all of them are busy, the oldest sound is cut.")]
        [SerializeField, Min(1)] private int _voiceCount = 12;

        [Tooltip("Fade at the start and the end of a looping sound, so it never clicks.")]
        [SerializeField, Min(0f)] private float _loopFadeSeconds = 0.08f;

        [Tooltip("Optional mixer group for every sound. Empty = straight to the AudioListener.")]
        [SerializeField] private AudioMixerGroup _output;

        [SerializeField] private SoundEntry[] _sounds = Array.Empty<SoundEntry>();

        public int VoiceCount => _voiceCount;
        public float LoopFadeSeconds => _loopFadeSeconds;
        public AudioMixerGroup Output => _output;
        public IReadOnlyList<SoundEntry> Sounds => _sounds;
    }
}
