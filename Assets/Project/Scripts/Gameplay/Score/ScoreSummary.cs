namespace SelfBulletting.Gameplay.Score
{
    public readonly struct ScoreSummary
    {
        public ScoreSummary(int destroyedObstacles, int basePoints, int comboBonus)
        {
            DestroyedObstacles = destroyedObstacles;
            BasePoints = basePoints;
            ComboBonus = comboBonus;
        }

        public static ScoreSummary Empty => default;

        public int DestroyedObstacles { get; }
        public int BasePoints { get; }
        public int ComboBonus { get; }
        public int Total => BasePoints + ComboBonus;
    }
}
