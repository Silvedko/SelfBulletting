namespace SelfBulletting.Gameplay.Score
{
    public readonly struct ScoreSummary
    {
        public ScoreSummary(int destroyedObstacles, int basePoints, int comboBonus, int healthBonus)
        {
            DestroyedObstacles = destroyedObstacles;
            BasePoints = basePoints;
            ComboBonus = comboBonus;
            HealthBonus = healthBonus;
        }

        public static ScoreSummary Empty => default;

        public int DestroyedObstacles { get; }
        public int BasePoints { get; }
        public int ComboBonus { get; }

        /// <summary>Points for what was left of the ball at the finish. Zero after a loss.</summary>
        public int HealthBonus { get; }

        public int Total => BasePoints + ComboBonus + HealthBonus;
    }
}
