using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Ball;
using UnityEngine;

namespace SelfBulletting.Gameplay.Shot
{
    /// <summary>
    /// Prices and power of shots. Sizes are percents of the start radius R0 unless a name says "World" (meters).
    /// The ball never gets below the critical size, which in the shooting modes equals the cost of the minimum shot.
    /// </summary>
    public sealed class ShotEconomy
    {
        private const float Tolerance = 0.001f;

        private readonly BallLimits _limits;
        private readonly ShotSettings _shot;

        /// <param name="limits">Built with the cost of the minimum shot as the critical size.</param>
        public ShotEconomy(BallLimits limits, ShotSettings shot)
        {
            _limits = limits;
            _shot = shot;
        }

        public float ShotSpeed => _shot.ShotSpeed;

        public float ToWorldRadius(float percent) => _limits.ToWorldRadius(percent);

        /// <summary>What the ball can still spend without getting below the critical size.</summary>
        public float GetRemainder(float radiusPercent) => _limits.GetRemainder(radiusPercent);

        public ShotMode GetShotMode(float radiusPercent)
        {
            if (!_limits.HasRemainder(radiusPercent))
            {
                return ShotMode.None;
            }

            return GetRemainder(radiusPercent) > _shot.MinShootPercent + Tolerance ? ShotMode.Aimed : ShotMode.Instant;
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

        /// <summary>
        /// The shot ball starts at its own radius for the minimum shot and grows exactly by as much as the player ball shrinks.
        /// </summary>
        public float GetShotWorldRadius(float costPercent) =>
            ToWorldRadius(_shot.ShotStartRadiusPercent + costPercent - _shot.MinShootPercent);

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

        /// <summary>
        /// Arrow mode: a shot without the scale, with a fixed cost and a fixed damage radius. The cost is never below the
        /// minimum shot and never above what the ball can spend, so with less left the shot takes everything above the
        /// critical size.
        /// </summary>
        /// <param name="costPercent">The fixed cost, % of R0.</param>
        /// <param name="damageRadiusPercent">The fixed damage radius, % of R0.</param>
        public ShotRequest CreateFixedShot(float radiusPercent, float costPercent, float damageRadiusPercent)
        {
            float cost = Mathf.Min(Mathf.Max(costPercent, _shot.MinShootPercent), GetRemainder(radiusPercent));
            return new ShotRequest(cost, ToWorldRadius(damageRadiusPercent), 0f);
        }

        /// <summary>The last shot without the scale: everything above the critical size, with the minimum damage radius.</summary>
        public ShotRequest CreateInstantShot(float radiusPercent) =>
            new ShotRequest(GetRemainder(radiusPercent), GetDamageWorldRadius(0f), 0f);
    }
}
