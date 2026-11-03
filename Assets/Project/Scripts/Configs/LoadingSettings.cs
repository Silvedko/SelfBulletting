using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class LoadingSettings
    {
        [Tooltip("The splash stays at least this long so it does not flash.")]
        [SerializeField, Min(0f)] private float _minDurationSeconds = 1f;

        public float MinDurationSeconds => _minDurationSeconds;
    }
}
