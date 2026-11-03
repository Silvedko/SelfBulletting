using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class FinishSettings
    {
        [Tooltip("The door opens when the ball is this close, meters.")]
        [SerializeField, Min(0f)] private float _doorOpenDistance = 5f;

        [SerializeField, Min(0.1f)] private float _hopLength = 1.5f;
        [SerializeField, Min(0f)] private float _hopHeight = 0.6f;
        [SerializeField, Min(0.05f)] private float _hopDurationSeconds = 0.25f;

        public float DoorOpenDistance => _doorOpenDistance;
        public float HopLength => _hopLength;
        public float HopHeight => _hopHeight;
        public float HopDurationSeconds => _hopDurationSeconds;
    }
}
