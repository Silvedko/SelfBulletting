using SelfBulletting.Configs;
using SelfBulletting.Infrastructure.Services.Audio;
using SelfBulletting.Infrastructure.Services.Vibration;
using UnityEngine;

namespace SelfBulletting.Gameplay.Feedback
{
    /// <summary>
    /// Warns while the previewed ball size is in the warning zone: vibration pulses get more frequent
    /// towards the critical size. The danger level (0..1) also drives the pulsing of the ball and the path.
    /// </summary>
    public sealed class DangerFeedback
    {
        private const float MillisecondsPerSecond = 1000f;

        private readonly FeedbackSettings _feedback;
        private readonly float _criticalPercent;
        private readonly IVibrationService _vibration;
        private readonly IAudioService _audio;

        private float _secondsSincePulse;
        private bool _isWarning;

        public DangerFeedback(FeedbackSettings feedback, ShotSettings shot, IVibrationService vibration, IAudioService audio)
        {
            _feedback = feedback;
            _criticalPercent = shot.MinShootPercent;
            _vibration = vibration;
            _audio = audio;
        }

        /// <summary>0 above the warning threshold, 1 at the critical size.</summary>
        public float GetDangerLevel(float previewPercent)
        {
            if (previewPercent >= _feedback.WarningPercent)
            {
                return 0f;
            }

            return Mathf.InverseLerp(_feedback.WarningPercent, _criticalPercent, previewPercent);
        }

        public float GetPulseIntervalSeconds(float dangerLevel) =>
            Mathf.Lerp(_feedback.VibrationIntervalMaxMs, _feedback.VibrationIntervalMinMs, dangerLevel) / MillisecondsPerSecond;

        /// <summary>Call every frame while aiming. Returns the current danger level.</summary>
        public float Tick(float previewPercent, float deltaTime)
        {
            float dangerLevel = GetDangerLevel(previewPercent);
            if (dangerLevel <= 0f)
            {
                Reset();
                return 0f;
            }

            if (!_isWarning)
            {
                _isWarning = true;
                _audio.Play(SoundId.Warning);
                Pulse();
                return dangerLevel;
            }

            _secondsSincePulse += deltaTime;
            if (_secondsSincePulse >= GetPulseIntervalSeconds(dangerLevel))
            {
                Pulse();
            }

            return dangerLevel;
        }

        public void Reset()
        {
            _isWarning = false;
            _secondsSincePulse = 0f;
        }

        private void Pulse()
        {
            _secondsSincePulse = 0f;
            _vibration.Pulse(_feedback.VibrationPulseMs);
        }
    }
}
