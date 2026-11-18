using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Aim
{
    /// <summary>
    /// The two sweeps of the Arrow mode. First the arrow swings from one edge of its sector to the other and back,
    /// starting at the left edge (the lower yaw). Then the landing point runs along the arrow from the shortest range to
    /// the longest and back.
    /// </summary>
    public sealed class ArrowSwing
    {
        private const float HalfWidthPerWidth = 0.5f;

        private readonly ArrowSettings _settings;

        public ArrowSwing(ArrowSettings settings)
        {
            _settings = settings;
        }

        public float GetYawDegrees(float elapsedSeconds)
        {
            float share = Mathf.PingPong(elapsedSeconds / _settings.SwingHalfPeriodSeconds, 1f);
            float firstEdge = _settings.SectorCenterDegrees - _settings.SectorWidthDegrees * HalfWidthPerWidth;
            return firstEdge + _settings.SectorWidthDegrees * share;
        }

        /// <summary>Flat unit direction of the arrow.</summary>
        public Vector3 GetDirection(float elapsedSeconds) =>
            Quaternion.Euler(0f, GetYawDegrees(elapsedSeconds), 0f) * Vector3.forward;

        /// <summary>Where the landing point is along the arrow: 0 — the shortest range, 1 — the longest.</summary>
        /// <param name="elapsedSeconds">Time since the direction was fixed.</param>
        public float GetRangeShare(float elapsedSeconds) =>
            Mathf.PingPong(elapsedSeconds / _settings.RangeHalfPeriodSeconds, 1f);
    }
}
