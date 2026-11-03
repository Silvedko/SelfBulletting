using System;
using UnityEngine;

namespace SelfBulletting.Views.Common
{
    /// <summary>
    /// How a view pulses while the previewed ball size is in the warning zone.
    /// </summary>
    [Serializable]
    public sealed class DangerPulseSettings
    {
        [Tooltip("Pulses per second right after the preview enters the warning zone.")]
        [SerializeField, Min(0.1f)] private float _calmPulsesPerSecond = 2f;

        [Tooltip("Pulses per second at the critical size.")]
        [SerializeField, Min(0.1f)] private float _criticalPulsesPerSecond = 6f;

        [Tooltip("Extra size at the peak of a pulse, share of the size.")]
        [SerializeField, Range(0f, 1f)] private float _sizeAmplitude = 0.08f;

        [Tooltip("Colour at the peak of a pulse.")]
        [SerializeField] private Color _flashColor = new Color(1f, 0.3f, 0.25f);

        public float SizeAmplitude => _sizeAmplitude;
        public Color FlashColor => _flashColor;

        public float GetPulsesPerSecond(float dangerLevel) =>
            Mathf.Lerp(_calmPulsesPerSecond, _criticalPulsesPerSecond, dangerLevel);
    }
}
