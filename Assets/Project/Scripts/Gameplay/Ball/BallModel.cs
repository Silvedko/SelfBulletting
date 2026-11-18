using System;
using UnityEngine;

namespace SelfBulletting.Gameplay.Ball
{
    /// <summary>
    /// Size of the player ball in percent of R0. Starts at 100% and never gets below the critical size of the mode.
    /// </summary>
    public sealed class BallModel
    {
        private readonly BallLimits _limits;

        public BallModel(BallLimits limits)
        {
            _limits = limits;
            RadiusPercent = BallLimits.FullSizePercent;
        }

        public event Action RadiusChanged;

        public float RadiusPercent { get; private set; }
        public float WorldRadius => _limits.ToWorldRadius(RadiusPercent);

        /// <summary>What the ball can still spend, % of R0.</summary>
        public float Remainder => _limits.GetRemainder(RadiusPercent);

        /// <summary>1 at full size, 0 at the critical size.</summary>
        public float Health => _limits.GetHealth(RadiusPercent);

        public bool HasRemainder => _limits.HasRemainder(RadiusPercent);

        public void Spend(float costPercent)
        {
            if (costPercent < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(costPercent), costPercent, "A shot cannot cost less than zero.");
            }

            RadiusPercent = Mathf.Max(RadiusPercent - costPercent, _limits.CriticalPercent);
            RadiusChanged?.Invoke();
        }
    }
}
