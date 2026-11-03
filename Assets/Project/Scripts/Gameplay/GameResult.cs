using SelfBulletting.Gameplay.Score;

namespace SelfBulletting.Gameplay
{
    public sealed class GameResult
    {
        private GameResult(bool isWin, ScoreSummary score)
        {
            IsWin = isWin;
            Score = score;
        }

        public bool IsWin { get; }
        public ScoreSummary Score { get; }

        /// <summary>
        /// The ball reached the finish.
        /// </summary>
        public static GameResult Finish(ScoreSummary score) => new GameResult(true, score);

        /// <summary>
        /// The path is still blocked and the ball has no shots left.
        /// </summary>
        public static GameResult Lose(ScoreSummary score) => new GameResult(false, score);
    }
}
