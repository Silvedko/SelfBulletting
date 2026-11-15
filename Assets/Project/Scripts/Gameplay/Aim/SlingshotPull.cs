using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Aim
{
    /// <summary>
    /// Reads the slingshot pull: the finger goes down anywhere and is pulled back. A pull inside the dead zone is no
    /// aim (releasing there cancels the shot); past it the pull length maps to the range share, full at the full pull.
    /// </summary>
    public sealed class SlingshotPull
    {
        private readonly SlingshotSettings _settings;

        public SlingshotPull(SlingshotSettings settings)
        {
            _settings = settings;
        }

        /// <param name="pressPoint">Where the finger went down, screen pixels.</param>
        /// <param name="currentPoint">Where the finger is now, screen pixels.</param>
        /// <param name="screenHeight">Screen height, pixels: pulls are measured as a share of it.</param>
        /// <param name="rangeShare">0 — the shortest range (just past the dead zone), 1 — the longest.</param>
        /// <returns>False while the pull is inside the dead zone.</returns>
        public bool TryGetRangeShare(Vector2 pressPoint, Vector2 currentPoint, float screenHeight, out float rangeShare)
        {
            float pull = screenHeight > 0f ? Vector2.Distance(pressPoint, currentPoint) / screenHeight : 0f;
            if (pull < _settings.DeadZoneScreenShare)
            {
                rangeShare = 0f;
                return false;
            }

            rangeShare = Mathf.InverseLerp(_settings.DeadZoneScreenShare, _settings.FullPullScreenShare, pull);
            return true;
        }

        /// <summary>The shot flies opposite to the pull: from where the finger is now towards where it went down.</summary>
        /// <param name="pressGroundPoint">The press point projected on the ground.</param>
        /// <param name="currentGroundPoint">The current finger point projected on the ground.</param>
        public static bool TryGetDirection(Vector3 pressGroundPoint, Vector3 currentGroundPoint, out Vector3 direction)
        {
            direction = pressGroundPoint - currentGroundPoint;
            direction.y = 0f;
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            direction.Normalize();
            return true;
        }
    }
}
