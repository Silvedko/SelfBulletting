using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// A shot that destroys at least <see cref="MinObstacles"/> obstacles multiplies its points by <see cref="Multiplier"/>.
    /// </summary>
    [Serializable]
    public struct ComboTier
    {
        [SerializeField, Min(2)] private int _minObstacles;
        [SerializeField, Min(1f)] private float _multiplier;

        public ComboTier(int minObstacles, float multiplier)
        {
            _minObstacles = minObstacles;
            _multiplier = multiplier;
        }

        public int MinObstacles => _minObstacles;
        public float Multiplier => _multiplier;
    }
}
