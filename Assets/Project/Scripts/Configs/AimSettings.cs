using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class AimSettings
    {
        [Tooltip("Seconds for the aim scale to go from minimum to maximum (and the same back).")]
        [SerializeField, Min(0.1f)] private float _halfPeriodSeconds = 3f;

        public float HalfPeriodSeconds => _halfPeriodSeconds;
    }
}
