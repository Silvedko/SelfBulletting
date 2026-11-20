using System;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Modes;

namespace SelfBulletting.Infrastructure.Services.Bonus
{
    /// <summary>
    /// Reads the bonuses from the config of the mode, so gameplay never looks into the configs for them itself.
    /// </summary>
    public sealed class BonusService : IBonusService
    {
        private readonly ModeConfigs _modes;

        public BonusService(ModeConfigs modes)
        {
            _modes = modes ?? throw new ArgumentNullException(nameof(modes));
        }

        public bool HasExtinguishBonus(GameMode mode) => GetSettings(mode).HasTiers;

        public float GetExtinguishWindowSeconds(GameMode mode) => GetSettings(mode).WindowSeconds;

        public BonusTier GetExtinguishBonus(GameMode mode, int fires)
        {
            ExtinguishBonusSettings settings = GetSettings(mode);
            int level = ComboTiers.GetLevel(settings.Tiers, fires);
            return level == 0 ? BonusTier.None : new BonusTier(ComboTiers.GetMultiplier(settings.Tiers, fires), level);
        }

        private ExtinguishBonusSettings GetSettings(GameMode mode) => _modes.Get(mode).ExtinguishBonus;
    }
}
