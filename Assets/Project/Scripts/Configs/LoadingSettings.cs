using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class LoadingSettings
    {
        [Tooltip("The splash stays at least this long so it does not flash.")]
        [SerializeField, Min(0f)] private float _minDurationSeconds = 1f;

        [Tooltip("The curtain fades in over the current scene before it is unloaded, seconds.")]
        [SerializeField, Min(0f)] private float _fadeInSeconds = 0.2f;

        [Tooltip("The curtain fades out over the freshly loaded scene, seconds.")]
        [SerializeField, Min(0f)] private float _fadeOutSeconds = 0.35f;

        public float MinDurationSeconds => _minDurationSeconds;
        public float FadeInSeconds => _fadeInSeconds;
        public float FadeOutSeconds => _fadeOutSeconds;
    }
}
