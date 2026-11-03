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

        [Tooltip("Bonus at the finish for every percent of the ball left above the critical size. " +
                 "A shot earns about 5–26 points per percent it spends (small single hits up to big x3 combos), " +
                 "so 30 makes every percent saved worth more than spending it on extra obstacles.")]
        [SerializeField, Min(0)] private int _pointsPerRemainingPercent = 30;

        public ScoreSettings()
        {
        }

        public ScoreSettings(int pointsPerObstacle, IEnumerable<ComboTier> comboTiers, int pointsPerRemainingPercent)
        {
            _pointsPerObstacle = pointsPerObstacle;
            _comboTiers = new List<ComboTier>(comboTiers);
            _pointsPerRemainingPercent = pointsPerRemainingPercent;
        }

        public int PointsPerObstacle => _pointsPerObstacle;
        public IReadOnlyList<ComboTier> ComboTiers => _comboTiers;
        public int PointsPerRemainingPercent => _pointsPerRemainingPercent;

        public void Validate()
        {
            _comboTiers.Sort((left, right) => left.MinObstacles.CompareTo(right.MinObstacles));
        }
    }
}
