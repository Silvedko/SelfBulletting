namespace SelfBulletting.Gameplay.Score
{
    public readonly struct ShotScore
    {
        public ShotScore(int destroyedObstacles, int basePoints, int comboBonus, float multiplier, int comboLevel)
        {
            DestroyedObstacles = destroyedObstacles;
            BasePoints = basePoints;
            ComboBonus = comboBonus;
            Multiplier = multiplier;
            ComboLevel = comboLevel;
        }

        public int DestroyedObstacles { get; }
        public int BasePoints { get; }
        public int ComboBonus { get; }
        public float Multiplier { get; }

        /// <summary>0 — no combo, 1 — the first tier, and so on up to the number of tiers.</summary>
        public int ComboLevel { get; }

        public bool IsCombo => ComboLevel > 0;
    }
}
