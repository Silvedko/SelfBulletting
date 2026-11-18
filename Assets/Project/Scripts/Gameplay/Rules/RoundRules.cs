using SelfBulletting.Gameplay.Ball;

namespace SelfBulletting.Gameplay.Rules
{
    /// <summary>
    /// Decides the round state before every turn: a clear path wins at any ball size,
    /// a blocked path with nothing left above the critical size loses.
    /// </summary>
    public sealed class RoundRules
    {
        private readonly BallLimits _limits;

        public RoundRules(BallLimits limits)
        {
            _limits = limits;
        }

        public RoundOutcome Evaluate(bool isPathClear, float radiusPercent)
        {
            if (isPathClear)
            {
                return RoundOutcome.Win;
            }

            return _limits.HasRemainder(radiusPercent) ? RoundOutcome.Continue : RoundOutcome.Lose;
        }
    }
}
