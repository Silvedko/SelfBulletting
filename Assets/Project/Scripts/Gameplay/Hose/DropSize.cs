using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Ball;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// How big the drops are: a share of R0 at the full ball that shrinks with the ball, though never below the smallest
    /// drop. The drop feels the fires with a slightly bigger sphere, and its splash reaches a few radii around it.
    /// Everything here is in meters.
    /// </summary>
    public sealed class DropSize
    {
        private readonly HoseSettings _settings;
        private readonly BallLimits _limits;

        public DropSize(HoseSettings settings, BallLimits limits)
        {
            _settings = settings;
            _limits = limits;
        }

        /// <param name="ballRadiusPercent">The ball size right now, % of R0.</param>
        public float GetRadius(float ballRadiusPercent)
        {
            float shrunkPercent = _settings.DropRadiusPercent * ballRadiusPercent / BallLimits.FullSizePercent;
            return _limits.ToWorldRadius(Mathf.Max(shrunkPercent, _settings.MinDropRadiusPercent));
        }

        public float GetTouchRadius(float dropRadius) => dropRadius * _settings.TouchRadiusScale;

        public float GetSplashRadius(float dropRadius) => dropRadius * _settings.SplashRadiusScale;
    }
}
