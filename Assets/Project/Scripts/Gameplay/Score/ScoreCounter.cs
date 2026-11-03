using System;
using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Score
{
    /// <summary>
    /// Points for destroyed obstacles. A shot that destroys enough obstacles gets the multiplier of the highest tier it reaches.
    /// </summary>
    public sealed class ScoreCounter
    {
        private const float NoComboMultiplier = 1f;

        private readonly ScoreSettings _settings;

        private int _destroyedObstacles;
        private int _basePoints;
        private int _comboBonus;

        public ScoreCounter(ScoreSettings settings)
        {
            _settings = settings;
        }

        public int PointsPerObstacle => _settings.PointsPerObstacle;
        public ScoreSummary Summary => new ScoreSummary(_destroyedObstacles, _basePoints, _comboBonus);

        public ShotScore AddShot(int destroyedObstacles)
        {
            if (destroyedObstacles < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(destroyedObstacles), destroyedObstacles, "Cannot destroy a negative number of obstacles.");
            }

            float multiplier = GetMultiplier(destroyedObstacles);
            int basePoints = destroyedObstacles * _settings.PointsPerObstacle;
            int comboBonus = Mathf.RoundToInt(basePoints * (multiplier - NoComboMultiplier));

            _destroyedObstacles += destroyedObstacles;
            _basePoints += basePoints;
            _comboBonus += comboBonus;

            return new ShotScore(destroyedObstacles, basePoints, comboBonus, multiplier);
        }

        public float GetMultiplier(int destroyedObstacles)
        {
            float multiplier = NoComboMultiplier;
            int reachedTier = 0;

            foreach (ComboTier tier in _settings.ComboTiers)
            {
                if (destroyedObstacles >= tier.MinObstacles && tier.MinObstacles > reachedTier)
                {
                    reachedTier = tier.MinObstacles;
                    multiplier = tier.Multiplier;
                }
            }

            return multiplier;
        }
    }
}
