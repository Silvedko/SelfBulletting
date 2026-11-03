using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Shot;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// The numbers from the spec: R0 = 1.5 m, shots cost 10–40%, damage radius 30–200% of R0.
    /// </summary>
    internal static class TestSettings
    {
        public const float StartRadius = 1.5f;
        public const float MinShootPercent = 10f;
        public const float MaxShootPercent = 40f;
        public const float DamageRadiusMinPercent = 30f;
        public const float DamageRadiusMaxPercent = 200f;

        public static BallSettings Ball() => new BallSettings(StartRadius);

        public static ShotSettings Shot() =>
            new ShotSettings(MinShootPercent, MaxShootPercent, DamageRadiusMinPercent, DamageRadiusMaxPercent);

        public static ShotEconomy Economy() => new ShotEconomy(Ball(), Shot());
    }
}
