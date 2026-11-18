using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// Points of one game mode: for every put-out obstacle and, at the finish, for what is left of the ball.
    /// </summary>
    [Serializable]
    public sealed class ScoreSettings
    {
        [SerializeField, Min(0)] private int _pointsPerObstacle = 10;

        [Tooltip("Bonus at the finish for every percent of the ball left above the critical size. " +
                 "A shot earns about 5–26 points per percent it spends (small single hits up to big x3 combos), " +
                 "so 30 makes every percent saved worth more than spending it on extra obstacles.")]
        [SerializeField, Min(0)] private int _pointsPerRemainingPercent = 30;

        public ScoreSettings()
        {
        }

        public ScoreSettings(int pointsPerObstacle, int pointsPerRemainingPercent)
        {
            _pointsPerObstacle = pointsPerObstacle;
            _pointsPerRemainingPercent = pointsPerRemainingPercent;
        }

        public int PointsPerObstacle => _pointsPerObstacle;
        public int PointsPerRemainingPercent => _pointsPerRemainingPercent;
    }
}
