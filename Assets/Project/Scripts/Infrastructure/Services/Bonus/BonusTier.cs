namespace SelfBulletting.Infrastructure.Services.Bonus
{
    /// <summary>The bonus tier a burst reached: its multiplier and how many tiers it passed.</summary>
    public readonly struct BonusTier
    {
        public BonusTier(float multiplier, int level)
        {
            Multiplier = multiplier;
            Level = level;
        }

        public static BonusTier None => new BonusTier(1f, 0);

        public float Multiplier { get; }

        /// <summary>0 — no tier reached, 1 — the first one, and so on up to the number of tiers.</summary>
        public int Level { get; }

        public bool IsReached => Level > 0;
    }
}
