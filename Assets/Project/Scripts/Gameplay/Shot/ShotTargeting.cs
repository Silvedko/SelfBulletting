using SelfBulletting.Gameplay.Scanning;
using UnityEngine;

namespace SelfBulletting.Gameplay.Shot
{
    /// <summary>
    /// Finds what a shot of the current size really touches. The shot rolls straight along the path from the front of
    /// the player ball, so a bigger shot may touch an obstacle that a thinner one passes by, and the thinner one rolls on
    /// to whatever is next on its line. The target is found again whenever the shot size changes.
    /// If nothing lies on the straight line at all, the shot rolls towards the obstacle that blocks the ball's corridor
    /// and touches it, so a shot is never wasted.
    /// </summary>
    public sealed class ShotTargeting
    {
        private readonly PathScanner _scanner;
        private readonly Vector3 _ballGroundPoint;
        private readonly Vector3 _direction;
        private readonly float _pathLength;

        public ShotTargeting(PathScanner scanner, Vector3 ballGroundPoint, Vector3 finishGroundPoint)
        {
            _scanner = scanner;
            _ballGroundPoint = ballGroundPoint;

            Vector3 flatOffset = finishGroundPoint - ballGroundPoint;
            flatOffset.y = 0f;
            _pathLength = flatOffset.magnitude;
            _direction = flatOffset.normalized;
        }

        /// <param name="ballRadius">Player ball radius once the shot has left it, meters.</param>
        /// <param name="shotRadius">Shot ball radius, meters.</param>
        /// <param name="blocker">The first obstacle in the ball's corridor: the fallback target.</param>
        public ShotTarget Find(float ballRadius, float shotRadius, RaycastHit blocker)
        {
            Vector3 launchCenter = _ballGroundPoint + _direction * (ballRadius + shotRadius) + Vector3.up * shotRadius;
            float distanceToFinish = Mathf.Max(0f, _pathLength - ballRadius - shotRadius);

            if (TryRoll(launchCenter, _direction, distanceToFinish, shotRadius, out ShotTarget target))
            {
                return target;
            }

            return RollTowardsBlocker(launchCenter, shotRadius, blocker);
        }

        private ShotTarget RollTowardsBlocker(Vector3 launchCenter, float shotRadius, RaycastHit blocker)
        {
            Vector3 flatOffset = blocker.point - launchCenter;
            flatOffset.y = 0f;
            float distance = flatOffset.magnitude;
            Vector3 direction = distance > Mathf.Epsilon ? flatOffset / distance : _direction;

            if (TryRoll(launchCenter, direction, distance, shotRadius, out ShotTarget target))
            {
                return target;
            }

            // The blocker surface lies on the way, so this is only a guard: stop the shot right next to the blocker.
            Vector3 contactCenter = launchCenter + direction * Mathf.Max(0f, distance - shotRadius);
            return new ShotTarget(launchCenter, contactCenter, blocker.point, blocker.collider);
        }

        private bool TryRoll(Vector3 launchCenter, Vector3 direction, float distance, float shotRadius, out ShotTarget target)
        {
            if (_scanner.TryFindFirstObstacle(launchCenter, launchCenter + direction * distance, shotRadius, out RaycastHit touch))
            {
                target = new ShotTarget(launchCenter, launchCenter + direction * touch.distance, touch.point, touch.collider);
                return true;
            }

            target = default;
            return false;
        }
    }
}
