namespace SelfBulletting.Gameplay.Score
{
    public readonly struct ScoreSummary
    {
        public ScoreSummary(int destroyedObstacles, int basePoints, int comboBonus, int healthBonus, bool hasCombo)
        {
            DestroyedObstacles = destroyedObstacles;
            BasePoints = basePoints;
            ComboBonus = comboBonus;
            HealthBonus = healthBonus;
            HasCombo = hasCombo;
        }

        public static ScoreSummary Empty => default;

        public int DestroyedObstacles { get; }
        public int BasePoints { get; }
        public int ComboBonus { get; }

        /// <summary>Points for what was left of the ball at the finish. Zero after a loss.</summary>
        public int HealthBonus { get; }

        /// <summary>False in a mode without combo (the Hose): the final screen leaves the combo line out.</summary>
        public bool HasCombo { get; }

        public int Total => BasePoints + ComboBonus + HealthBonus;
    }
}
