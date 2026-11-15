using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// The lobbed shot of the aim modes: how far it can land from the ball and how high its arc goes.
    /// </summary>
    [Serializable]
    public sealed class LobSettings
    {
        [Tooltip("Shortest landing distance from the ball centre, meters.")]
        [SerializeField, Min(0f)] private float _minRange = 3f;

        [Tooltip("Longest landing distance from the ball centre, meters. The field edge may cut it shorter.")]
        [SerializeField, Min(0f)] private float _maxRange = 24f;

        [Tooltip("Arc apex above the straight line from launch to landing, per meter of flight.")]
        [SerializeField, Min(0f)] private float _apexHeightPerMeter = 0.25f;

        [Tooltip("The arc is never lower than this, meters.")]
        [SerializeField, Min(0f)] private float _minApexHeight = 1f;

        public LobSettings()
        {
        }

        public LobSettings(float minRange, float maxRange, float apexHeightPerMeter, float minApexHeight)
        {
            _minRange = minRange;
            _maxRange = maxRange;
            _apexHeightPerMeter = apexHeightPerMeter;
            _minApexHeight = minApexHeight;
            Validate();
        }

        public float MinRange => _minRange;
        public float MaxRange => _maxRange;
        public float ApexHeightPerMeter => _apexHeightPerMeter;
        public float MinApexHeight => _minApexHeight;

        public void Validate()
        {
            _maxRange = Mathf.Max(_minRange, _maxRange);
        }
    }
}
