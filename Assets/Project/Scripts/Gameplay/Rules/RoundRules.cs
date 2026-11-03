using SelfBulletting.Gameplay.Shot;

namespace SelfBulletting.Gameplay.Rules
{
    /// <summary>
    /// Decides the round state before every shot: a clear path wins at any ball size,
    /// a blocked path with nothing left above the critical size loses.
    /// </summary>
    public sealed class RoundRules
    {
        private readonly ShotEconomy _economy;

        public RoundRules(ShotEconomy economy)
        {
            _economy = economy;
        }

        public RoundOutcome Evaluate(bool isPathClear, float radiusPercent)
        {
            if (isPathClear)
            {
                return RoundOutcome.Win;
            }

            return _economy.GetShotMode(radiusPercent) == ShotMode.None ? RoundOutcome.Lose : RoundOutcome.Continue;
        }
    }
}
