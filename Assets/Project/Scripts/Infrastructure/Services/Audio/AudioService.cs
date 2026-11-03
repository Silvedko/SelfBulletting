using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace SelfBulletting.Infrastructure.Services.Audio
{
    /// <summary>
    /// Plays the sounds of a <see cref="SoundLibrary"/>. One-shots go through a small pool of AudioSources and are
    /// limited per sound, so a wave of explosions does not stack dozens of copies; the looping sound has its own source.
    /// The sources live under the Boot scene, which is never unloaded, so the service never points into a content scene.
    /// </summary>
    public sealed class AudioService : IAudioService, IDisposable
    {
        private const float BasePitch = 1f;

        private readonly Dictionary<SoundId, SoundEntry> _entries = new Dictionary<SoundId, SoundEntry>();
        private readonly Dictionary<SoundId, float> _lastPlayTimes = new Dictionary<SoundId, float>();
        private readonly Random _random = new Random();
        private readonly GameObject _root;
        private readonly SoundVoice[] _voices;
        private readonly AudioSource _loopSource;
        private readonly float _loopFadeSeconds;
        private readonly TweenCallback _stopLoopSource;

        private SoundId? _loopSound;

        public AudioService(SoundLibrary library, Transform parent)
        {
            _root = new GameObject(nameof(AudioService));
            _root.transform.SetParent(parent, false);
            _loopFadeSeconds = library.LoopFadeSeconds;

            foreach (SoundEntry entry in library.Sounds)
            {
                if (entry.Clip != null)
                {
                    _entries[entry.Id] = entry;
                }
            }

            _voices = new SoundVoice[library.VoiceCount];
            for (int i = 0; i < _voices.Length; i++)
            {
                _voices[i] = new SoundVoice(CreateSource(library.Output, isLooping: false));
            }

            _loopSource = CreateSource(library.Output, isLooping: true);
            _stopLoopSource = _loopSource.Stop;
        }

        public void Play(SoundId sound)
        {
            float now = Time.unscaledTime;
            if (!_entries.TryGetValue(sound, out SoundEntry entry) || !CanPlay(entry, now))
            {
                return;
            }

            _lastPlayTimes[sound] = now;
            PickVoice().Play(entry, GetPitch(entry), now);
        }

        public void StartLoop(SoundId sound)
        {
            if (_loopSound == sound || !_entries.TryGetValue(sound, out SoundEntry entry))
            {
                return;
            }

            _loopSound = sound;
            _loopSource.DOKill();
            _loopSource.clip = entry.Clip;
            _loopSource.volume = 0f;
            _loopSource.Play();
            FadeLoop(entry.Volume);
        }

        public void StopLoop(SoundId sound)
        {
            if (_loopSound != sound)
            {
                return;
            }

            _loopSound = null;
            _loopSource.DOKill();
            FadeLoop(0f).OnComplete(_stopLoopSource);
        }

        public void Dispose()
        {
            if (_root != null)
            {
                Object.Destroy(_root);
            }
        }

        private bool CanPlay(SoundEntry entry, float now)
        {
            bool isTooSoon = _lastPlayTimes.TryGetValue(entry.Id, out float lastTime)
                && now - lastTime < entry.MinIntervalSeconds;

            return !isTooSoon && CountPlaying(entry.Id) < entry.MaxVoices;
        }

        private int CountPlaying(SoundId sound)
        {
            int count = 0;
            foreach (SoundVoice voice in _voices)
            {
                if (voice.IsPlayingSound(sound))
                {
                    count++;
                }
            }

            return count;
        }

        /// <summary>A free voice, or the one that started first when every voice is busy.</summary>
        private SoundVoice PickVoice()
        {
            SoundVoice oldest = _voices[0];
            foreach (SoundVoice voice in _voices)
            {
                if (!voice.IsPlaying)
                {
                    return voice;
                }

                if (voice.StartTime < oldest.StartTime)
                {
                    oldest = voice;
                }
            }

            return oldest;
        }

        private float GetPitch(SoundEntry entry) =>
            BasePitch + Mathf.Lerp(-entry.PitchVariation, entry.PitchVariation, (float)_random.NextDouble());

        private Tween FadeLoop(float volume) =>
            _loopSource.DOFade(volume, _loopFadeSeconds).SetUpdate(true).SetLink(_root);

        private AudioSource CreateSource(AudioMixerGroup output, bool isLooping)
        {
            var source = _root.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = isLooping;
            source.outputAudioMixerGroup = output;
            return source;
        }
    }
}
