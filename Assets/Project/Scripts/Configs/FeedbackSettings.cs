using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class FeedbackSettings
    {
        [Tooltip("When the previewed ball size drops to this % of R0, the ball and the path pulse and the phone vibrates.")]
        [SerializeField] private float _warningPercent = 15f;

        [SerializeField, Min(1)] private int _vibrationPulseMs = 40;

        [Tooltip("Pause between pulses right at the warning threshold.")]
        [SerializeField, Min(1)] private int _vibrationIntervalMaxMs = 400;

        [Tooltip("Pause between pulses at the critical size.")]
        [SerializeField, Min(1)] private int _vibrationIntervalMinMs = 120;

        public FeedbackSettings()
        {
        }

        public FeedbackSettings(float warningPercent, int vibrationPulseMs, int vibrationIntervalMaxMs, int vibrationIntervalMinMs)
        {
            _warningPercent = warningPercent;
            _vibrationPulseMs = vibrationPulseMs;
            _vibrationIntervalMaxMs = vibrationIntervalMaxMs;
            _vibrationIntervalMinMs = vibrationIntervalMinMs;
        }

        public float WarningPercent => _warningPercent;
        public int VibrationPulseMs => _vibrationPulseMs;
        public int VibrationIntervalMaxMs => _vibrationIntervalMaxMs;
        public int VibrationIntervalMinMs => _vibrationIntervalMinMs;

        public void Validate(float criticalPercent)
        {
            _warningPercent = Mathf.Clamp(_warningPercent, criticalPercent, 100f);
            _vibrationIntervalMinMs = Mathf.Min(_vibrationIntervalMinMs, _vibrationIntervalMaxMs);
        }
    }
}
