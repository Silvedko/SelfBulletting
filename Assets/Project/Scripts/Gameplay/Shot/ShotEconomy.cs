using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Shot
{
    /// <summary>
    /// Prices and power of shots. Sizes are percents of the start radius R0 unless a name says "World" (meters).
    /// The ball never gets below the critical size, which equals the cost of the minimum shot.
    /// </summary>
    public sealed class ShotEconomy
    {
        private const float FullSizePercent = 100f;
        private const float Tolerance = 0.001f;

        private readonly ShotSettings _shot;
        private readonly float _startRadius;

        public ShotEconomy(BallSettings ball, ShotSettings shot)
        {
            _startRadius = ball.StartRadius;
            _shot = shot;
        }

        public float CriticalPercent => _shot.MinShootPercent;
        public float ShotSpeed => _shot.ShotSpeed;
        public float ShotWorldRadius => ToWorldRadius(_shot.ShotRadiusPercent);

        public float ToWorldRadius(float percent) => _startRadius * percent / FullSizePercent;

        /// <summary>What the ball can still spend without getting below the critical size.</summary>
        public float GetRemainder(float radiusPercent) => Mathf.Max(0f, radiusPercent - _shot.MinShootPercent);

        /// <summary>The remainder as a share of the full ball: 1 at full size, 0 at the critical size.</summary>
        public float GetHealth(float radiusPercent) =>
            Mathf.Clamp01(GetRemainder(radiusPercent) / (FullSizePercent - _shot.MinShootPercent));

        public ShotMode GetShotMode(float radiusPercent)
        {
            float remainder = GetRemainder(radiusPercent);
            if (remainder <= Tolerance)
            {
                return ShotMode.None;
            }

            return remainder > _shot.MinShootPercent + Tolerance ? ShotMode.Aimed : ShotMode.Instant;
        }

        /// <summary>
        /// Upper bound of the aim scale: 1 while the ball can afford the maximum shot, less when only part of the range is affordable.
        /// </summary>
        public float GetScaleLimit(float radiusPercent)
        {
            float affordableAboveMinimumShot = GetRemainder(radiusPercent) - _shot.MinShootPercent;
            return Mathf.Clamp01(affordableAboveMinimumShot / (_shot.MaxShootPercent - _shot.MinShootPercent));
        }

        public float GetCostPercent(float scale) => Mathf.Lerp(_shot.MinShootPercent, _shot.MaxShootPercent, scale);

        public float GetDamageWorldRadius(float scale) =>
            ToWorldRadius(Mathf.Lerp(_shot.DamageRadiusMinPercent, _shot.DamageRadiusMaxPercent, scale));

        public float GetPreviewRadiusPercent(float radiusPercent, float scale) =>
            Mathf.Max(radiusPercent - GetCostPercent(scale), _shot.MinShootPercent);

        public ShotRequest CreateAimedShot(float radiusPercent, float scale)
        {
            float clampedScale = Mathf.Clamp(scale, 0f, GetScaleLimit(radiusPercent));
            float cost = Mathf.Min(GetCostPercent(clampedScale), GetRemainder(radiusPercent));
            return new ShotRequest(cost, GetDamageWorldRadius(clampedScale), clampedScale);
        }

        /// <summary>The last shot without the scale: everything above the critical size, with the minimum damage radius.</summary>
        public ShotRequest CreateInstantShot(float radiusPercent) =>
            new ShotRequest(GetRemainder(radiusPercent), GetDamageWorldRadius(0f), 0f);
    }
}
