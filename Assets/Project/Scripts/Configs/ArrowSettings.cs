using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// Arrow mode: the sector the arrow swings in and how fast, then how fast the landing point runs along the arrow.
    /// Angles are the world yaw: 0° looks along +Z (up the screen), 90° along +X (to the right).
    /// </summary>
    [Serializable]
    public sealed class ArrowSettings
    {
        [Tooltip("Middle of the sector, degrees of world yaw. 45° points from the bottom-left corner into the field.")]
        [SerializeField, Range(-180f, 180f)] private float _sectorCenterDegrees = 45f;

        [Tooltip("Width of the sector, degrees.")]
        [SerializeField, Range(1f, 360f)] private float _sectorWidthDegrees = 90f;

        [Tooltip("Seconds for the arrow to swing from one edge of the sector to the other.")]
        [SerializeField, Min(0.1f)] private float _swingHalfPeriodSeconds = 1.2f;

        [Tooltip("Seconds for the landing point to run along the arrow from the shortest range to the longest.")]
        [SerializeField, Min(0.1f)] private float _rangeHalfPeriodSeconds = 1.5f;

        public ArrowSettings()
        {
        }

        public ArrowSettings(float sectorCenterDegrees, float sectorWidthDegrees, float swingHalfPeriodSeconds,
            float rangeHalfPeriodSeconds)
        {
            _sectorCenterDegrees = sectorCenterDegrees;
            _sectorWidthDegrees = sectorWidthDegrees;
            _swingHalfPeriodSeconds = swingHalfPeriodSeconds;
            _rangeHalfPeriodSeconds = rangeHalfPeriodSeconds;
        }

        public float SectorCenterDegrees => _sectorCenterDegrees;
        public float SectorWidthDegrees => _sectorWidthDegrees;
        public float SwingHalfPeriodSeconds => _swingHalfPeriodSeconds;
        public float RangeHalfPeriodSeconds => _rangeHalfPeriodSeconds;
    }
}
