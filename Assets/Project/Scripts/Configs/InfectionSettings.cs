using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class InfectionSettings
    {
        [Tooltip("Speed of the infection wave from the impact point, m/s. Delay of each obstacle = distance / speed.")]
        [SerializeField, Min(0.1f)] private float _waveSpeed = 8f;

        [Tooltip("How long an infected obstacle glows before it explodes, seconds.")]
        [SerializeField, Min(0f)] private float _infectDurationSeconds = 0.15f;

        public float WaveSpeed => _waveSpeed;
        public float InfectDurationSeconds => _infectDurationSeconds;
    }
}
