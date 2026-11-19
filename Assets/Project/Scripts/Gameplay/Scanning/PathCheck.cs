using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Paths;
using UnityEngine;

namespace SelfBulletting.Gameplay.Scanning
{
    /// <summary>
    /// Is the path to the finish clear for the ball at its current size, i.e. does no obstacle stand on the strip as wide
    /// as the ball? The round asks before every turn; the Hose also
    /// asks while the water flows, so the stream stops as soon as the way opens.
    /// </summary>
    public sealed class PathCheck
    {
        private readonly GroundPath _path;
        private readonly PathScanner _scanner;
        private readonly BallModel _ball;

        public PathCheck(GroundPath path, PathScanner scanner, BallModel ball)
        {
            _path = path;
            _scanner = scanner;
            _ball = ball;
        }

        public bool IsClear() => !TryFindBlocker(out _);

        /// <param name="blocker">The first obstacle along the path.</param>
        public bool TryFindBlocker(out RaycastHit blocker) => _scanner.TryFindFirstObstacle(_path, _ball.WorldRadius, out blocker);
    }
}
