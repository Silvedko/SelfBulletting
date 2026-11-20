using SelfBulletting.Gameplay.Modes;

namespace SelfBulletting.Infrastructure.Services.Bonus
{
    /// <summary>
    /// Bonuses of every game mode, read from the mode's config. A mode without a bonus answers with no tiers reached.
    /// </summary>
    public interface IBonusService
    {
        /// <summary>The mode pays a bonus for fires put out together.</summary>
        bool HasExtinguishBonus(GameMode mode);

        /// <summary>Fires that go out within this many seconds of the first one count as put out together.</summary>
        float GetExtinguishWindowSeconds(GameMode mode);

        /// <summary>The bonus tier <paramref name="fires"/> fires put out together reach in the mode.</summary>
        BonusTier GetExtinguishBonus(GameMode mode, int fires);
    }
}
