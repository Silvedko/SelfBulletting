using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class ApplicationSettings
    {
        [Tooltip("Mobile platforms run at 30 fps unless a target is set.")]
        [SerializeField, Min(30)] private int _targetFrameRate = 60;

        public int TargetFrameRate => _targetFrameRate;
    }
}
