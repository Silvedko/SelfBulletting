using System;
using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class ScoreSettings
    {
        [SerializeField, Min(0)] private int _pointsPerObstacle = 10;

        [Tooltip("The highest tier the shot reaches is applied.")]
        [SerializeField] private List<ComboTier> _comboTiers = new List<ComboTier>
        {
            new ComboTier(3, 1.5f),
            new ComboTier(6, 2f),
            new ComboTier(10, 3f)
        };

        public ScoreSettings()
        {
        }

        public ScoreSettings(int pointsPerObstacle, IEnumerable<ComboTier> comboTiers)
        {
            _pointsPerObstacle = pointsPerObstacle;
            _comboTiers = new List<ComboTier>(comboTiers);
        }

        public int PointsPerObstacle => _pointsPerObstacle;
        public IReadOnlyList<ComboTier> ComboTiers => _comboTiers;

        public void Validate()
        {
            _comboTiers.Sort((left, right) => left.MinObstacles.CompareTo(right.MinObstacles));
        }
    }
}
