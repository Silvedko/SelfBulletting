using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Shot;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// The numbers from the spec: R0 = 1.5 m, shots cost 10–40%, damage radius 30–200% of R0, the minimum shot ball is 20% of R0.
    /// The Hose drains 2% a second down to the same critical size as the shooting modes; its drops are 8% of R0 at the
    /// full ball and never below 4%. 8 drops a second and frames of 1/16 s keep the drop pace exact in floats.
    /// </summary>
    internal static class TestSettings
    {
        public const float StartRadius = 1.5f;
        public const float MinShootPercent = 10f;
        public const float MaxShootPercent = 40f;
        public const float DamageRadiusMinPercent = 30f;
        public const float DamageRadiusMaxPercent = 200f;
        public const float ShotStartRadiusPercent = 20f;

        public const float DrainPercentPerSecond = 2f;
        public const float DropsPerSecond = 8f;
        public const float DropSpeed = 15f;
        public const float DropRadiusPercent = 8f;
        public const float MinDropRadiusPercent = 4f;
        public const float TouchRadiusScale = 1.5f;
        public const float SplashRadiusScale = 2.5f;
        public const int PreviewSegments = 12;
        public const float Frame = 1f / 16f;

        public static BallSettings Ball() => new BallSettings(StartRadius);

        public static ShotSettings Shot() =>
            new ShotSettings(MinShootPercent, MaxShootPercent, DamageRadiusMinPercent, DamageRadiusMaxPercent, ShotStartRadiusPercent);

        /// <summary>Every mode, the Hose too: the minimum shot of the shooting modes is the critical size.</summary>
        public static BallLimits Limits() => new BallLimits(StartRadius, MinShootPercent);

        public static ShotEconomy Economy() => new ShotEconomy(Limits(), Shot());

        public static HoseSettings Hose(int hitsToExtinguish = 1, int maxDropsInFlight = 32) =>
            new HoseSettings(MinShootPercent, DrainPercentPerSecond, DropsPerSecond, DropSpeed, DropRadiusPercent,
                MinDropRadiusPercent, TouchRadiusScale, SplashRadiusScale, hitsToExtinguish, maxDropsInFlight,
                PreviewSegments);
    }
}
