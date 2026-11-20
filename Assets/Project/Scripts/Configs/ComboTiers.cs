using System.Collections.Generic;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// Reads a list of combo tiers: the multiplier and the level of the highest tier a number of obstacles reaches.
    /// Shared by the shot combo and the Hose bonus for fires put out together.
    /// </summary>
    public static class ComboTiers
    {
        public const float NoMultiplier = 1f;

        /// <returns>The multiplier of the highest tier reached; 1 below the first tier.</returns>
        public static float GetMultiplier(IReadOnlyList<ComboTier> tiers, int obstacles)
        {
            float multiplier = NoMultiplier;
            int reachedTier = 0;

            foreach (ComboTier tier in tiers)
            {
                if (obstacles >= tier.MinObstacles && tier.MinObstacles > reachedTier)
                {
                    reachedTier = tier.MinObstacles;
                    multiplier = tier.Multiplier;
                }
            }

            return multiplier;
        }

        /// <returns>How many tiers are reached: 0 — none, 1 — the first one, and so on.</returns>
        public static int GetLevel(IReadOnlyList<ComboTier> tiers, int obstacles)
        {
            int level = 0;
            foreach (ComboTier tier in tiers)
            {
                if (obstacles >= tier.MinObstacles)
                {
                    level++;
                }
            }

            return level;
        }
    }
}
