using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Aim
{
    /// <summary>
    /// Ping-pong value of the aim scale: from 0 up to the limit and back, always at the same speed
    /// (0 to 1 takes one half period), so a cut scale simply bounces earlier.
    /// </summary>
    public sealed class AimScale
    {
        private readonly float _halfPeriodSeconds;

        public AimScale(AimSettings settings)
        {
            _halfPeriodSeconds = settings.HalfPeriodSeconds;
        }

        public float Evaluate(float elapsedSeconds, float limit)
        {
            if (limit <= 0f)
            {
                return 0f;
            }

            return Mathf.PingPong(elapsedSeconds / _halfPeriodSeconds, limit);
        }
    }
}
