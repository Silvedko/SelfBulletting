using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class BallSettings
    {
        [Tooltip("R0, the player ball radius at start, in meters. All percents are relative to it.")]
        [SerializeField, Min(0.1f)] private float _startRadius = 1.5f;

        public BallSettings()
        {
        }

        public BallSettings(float startRadius)
        {
            _startRadius = startRadius;
        }

        public float StartRadius => _startRadius;
    }
}
