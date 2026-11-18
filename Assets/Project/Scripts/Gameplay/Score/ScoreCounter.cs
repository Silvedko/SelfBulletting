using System;
using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Score
{
    /// <summary>
    /// Points for destroyed obstacles, with the multiplier of the highest combo tier a shot reaches,
    /// plus a bonus at the finish for every percent of the ball that is left above the critical size.
    /// A mode without combo tiers counts every obstacle at face value.
    /// </summary>
    public sealed class ScoreCounter
    {
        private const float NoComboMultiplier = 1f;

        private readonly ScoreSettings _settings;
        private readonly ComboSettings _combo;

        private int _destroyedObstacles;
        private int _basePoints;
        private int _comboBonus;
        private int _healthBonus;

        public ScoreCounter(ScoreSettings settings, ComboSettings combo)
        {
            _settings = settings;
            _combo = combo ?? ComboSettings.None;
        }

        public int PointsPerObstacle => _settings.PointsPerObstacle;

        public ScoreSummary Summary =>
            new ScoreSummary(_destroyedObstacles, _basePoints, _comboBonus, _healthBonus, _combo.HasTiers);

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

            return new ShotScore(destroyedObstacles, basePoints, comboBonus, multiplier, GetComboLevel(destroyedObstacles));
        }

        /// <summary>The Hose puts the fires out one by one: every one counts on its own, never towards a combo.</summary>
        /// <returns>The points for this obstacle.</returns>
        public int AddObstacle()
        {
            _destroyedObstacles++;
            _basePoints += _settings.PointsPerObstacle;
            return _settings.PointsPerObstacle;
        }

        /// <param name="remainderPercent">What is left of the ball above the critical size, % of R0.</param>
        /// <returns>The bonus. Awarded once, at the finish; a second call replaces it.</returns>
        public int AwardHealthBonus(float remainderPercent)
        {
            _healthBonus = Mathf.RoundToInt(Mathf.Max(0f, remainderPercent) * _settings.PointsPerRemainingPercent);
            return _healthBonus;
        }

        public float GetMultiplier(int destroyedObstacles)
        {
            float multiplier = NoComboMultiplier;
            int reachedTier = 0;

            foreach (ComboTier tier in _combo.Tiers)
            {
                if (destroyedObstacles >= tier.MinObstacles && tier.MinObstacles > reachedTier)
                {
                    reachedTier = tier.MinObstacles;
                    multiplier = tier.Multiplier;
                }
            }

            return multiplier;
        }

        /// <summary>How many combo tiers the shot reached: 0 — none, 1 — the first one, and so on.</summary>
        public int GetComboLevel(int destroyedObstacles)
        {
            int level = 0;
            foreach (ComboTier tier in _combo.Tiers)
            {
                if (destroyedObstacles >= tier.MinObstacles)
                {
                    level++;
                }
            }

            return level;
        }
    }
}
