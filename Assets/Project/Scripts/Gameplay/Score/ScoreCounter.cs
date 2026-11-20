using System;
using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Score
{
    /// <summary>
    /// Points for destroyed obstacles, with the multiplier of the highest combo tier a shot reaches,
    /// plus a bonus at the finish for every percent of the ball that is left above the critical size.
    /// A mode without combo tiers counts every obstacle at face value. The Hose instead adds a bonus for fires put out
    /// together; it goes to the same combo line of the summary.
    /// </summary>
    public sealed class ScoreCounter
    {
        private readonly ScoreSettings _settings;
        private readonly ComboSettings _combo;
        private readonly bool _hasExtinguishBonus;

        private int _destroyedObstacles;
        private int _basePoints;
        private int _comboBonus;
        private int _healthBonus;

        /// <param name="hasExtinguishBonus">The mode pays a bonus for fires put out together, so the summary keeps the
        /// combo line even without combo tiers.</param>
        public ScoreCounter(ScoreSettings settings, ComboSettings combo, bool hasExtinguishBonus = false)
        {
            _settings = settings;
            _combo = combo ?? ComboSettings.None;
            _hasExtinguishBonus = hasExtinguishBonus;
        }

        public int PointsPerObstacle => _settings.PointsPerObstacle;

        public ScoreSummary Summary =>
            new ScoreSummary(_destroyedObstacles, _basePoints, _comboBonus, _healthBonus,
                _combo.HasTiers || _hasExtinguishBonus);

        public ShotScore AddShot(int destroyedObstacles)
        {
            if (destroyedObstacles < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(destroyedObstacles), destroyedObstacles, "Cannot destroy a negative number of obstacles.");
            }

            float multiplier = GetMultiplier(destroyedObstacles);
            int basePoints = destroyedObstacles * _settings.PointsPerObstacle;
            int comboBonus = Mathf.RoundToInt(basePoints * (multiplier - ComboTiers.NoMultiplier));

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

        /// <summary>
        /// The Hose put out <paramref name="fires"/> fires together. Their base points are already counted by
        /// <see cref="AddObstacle"/>, so only what the multiplier adds on top goes to the combo bonus.
        /// </summary>
        /// <returns>The bonus.</returns>
        public int AddExtinguishBonus(int fires, float multiplier)
        {
            if (fires < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fires), fires, "Cannot put out a negative number of fires.");
            }

            int bonus = Mathf.RoundToInt(fires * _settings.PointsPerObstacle *
                Mathf.Max(0f, multiplier - ComboTiers.NoMultiplier));
            _comboBonus += bonus;
            return bonus;
        }

        /// <param name="remainderPercent">What is left of the ball above the critical size, % of R0.</param>
        /// <returns>The bonus. Awarded once, at the finish; a second call replaces it.</returns>
        public int AwardHealthBonus(float remainderPercent)
        {
            _healthBonus = Mathf.RoundToInt(Mathf.Max(0f, remainderPercent) * _settings.PointsPerRemainingPercent);
            return _healthBonus;
        }

        public float GetMultiplier(int destroyedObstacles) => ComboTiers.GetMultiplier(_combo.Tiers, destroyedObstacles);

        /// <summary>How many combo tiers the shot reached: 0 — none, 1 — the first one, and so on.</summary>
        public int GetComboLevel(int destroyedObstacles) => ComboTiers.GetLevel(_combo.Tiers, destroyedObstacles);
    }
}
