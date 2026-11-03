using DG.Tweening;
using UnityEngine;

namespace SelfBulletting.Views.Common
{
    /// <summary>
    /// Warning pulse of the ball and the path: an endless yoyo tween whose speed follows the danger level.
    /// Both views get the level on the same frame, so they pulse in sync.
    /// </summary>
    public sealed class DangerPulse
    {
        // At time scale 1 one full pulse (up and back) lasts a second, so the time scale equals pulses per second.
        private const float HalfPulseSeconds = 0.5f;
        private const float Peak = 1f;

        private readonly DangerPulseSettings _settings;
        private readonly Tween _tween;

        private float _phase;

        public DangerPulse(DangerPulseSettings settings, GameObject owner)
        {
            _settings = settings;
            _tween = DOTween.To(() => _phase, value => _phase = value, Peak, HalfPulseSeconds)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false)
                .SetLink(owner)
                .Pause();
        }

        /// <summary>0 — at rest, 1 — at the peak of a pulse.</summary>
        public float Phase => _phase;

        public float SizeFactor => 1f + _settings.SizeAmplitude * _phase;

        public Color Tint(Color baseColor) => Color.Lerp(baseColor, _settings.FlashColor, _phase);

        /// <param name="dangerLevel">0 stops the pulse, 1 pulses at the critical rate.</param>
        public void SetLevel(float dangerLevel)
        {
            if (!_tween.IsActive())
            {
                return;
            }

            if (dangerLevel <= 0f)
            {
                Stop();
                return;
            }

            _tween.timeScale = _settings.GetPulsesPerSecond(dangerLevel);
            if (!_tween.IsPlaying())
            {
                _tween.Play();
            }
        }

        private void Stop()
        {
            if (_tween.IsPlaying() || _phase > 0f)
            {
                _tween.Rewind();
            }
        }
    }
}
