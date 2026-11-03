namespace SelfBulletting.Gameplay.Score
{
    public readonly struct ShotScore
    {
        public ShotScore(int destroyedObstacles, int basePoints, int comboBonus, float multiplier)
        {
            DestroyedObstacles = destroyedObstacles;
            BasePoints = basePoints;
            ComboBonus = comboBonus;
            Multiplier = multiplier;
        }

        public int DestroyedObstacles { get; }
        public int BasePoints { get; }
        public int ComboBonus { get; }
        public float Multiplier { get; }
        public bool IsCombo => ComboBonus > 0;
    }
}
