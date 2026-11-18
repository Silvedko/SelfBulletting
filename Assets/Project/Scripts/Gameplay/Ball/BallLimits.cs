using UnityEngine;

namespace SelfBulletting.Gameplay.Ball
{
    /// <summary>
    /// How big the ball can be in one game mode: it starts at the full size, R0, and never gets below the critical size
    /// of the mode. Sizes are percents of R0 unless a name says "World" (meters).
    /// </summary>
    public sealed class BallLimits
    {
        public const float FullSizePercent = 100f;

        private const float Tolerance = 0.001f;

        private readonly float _startRadius;

        /// <param name="startRadius">R0, meters.</param>
        /// <param name="criticalPercent">The smallest the ball gets, % of R0.</param>
        public BallLimits(float startRadius, float criticalPercent)
        {
            _startRadius = startRadius;
            CriticalPercent = criticalPercent;
        }

        public float CriticalPercent { get; }

        public float ToWorldRadius(float percent) => _startRadius * percent / FullSizePercent;

        /// <summary>What the ball can still spend without getting below the critical size.</summary>
        public float GetRemainder(float radiusPercent) => Mathf.Max(0f, radiusPercent - CriticalPercent);

        /// <summary>The remainder as a share of the full ball: 1 at full size, 0 at the critical size.</summary>
        public float GetHealth(float radiusPercent) =>
            Mathf.Clamp01(GetRemainder(radiusPercent) / (FullSizePercent - CriticalPercent));

        /// <summary>False once the ball is at the critical size and has nothing left to spend.</summary>
        public bool HasRemainder(float radiusPercent) => GetRemainder(radiusPercent) > Tolerance;
    }
}
