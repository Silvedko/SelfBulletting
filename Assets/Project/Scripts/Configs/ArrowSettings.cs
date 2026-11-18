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

        [Tooltip("Every Arrow shot costs this much, % of R0, at any range. Not below the minimum shot; with less left " +
            "the shot takes everything above the critical size.")]
        [SerializeField, Min(0f)] private float _shotCostPercent = 15f;

        [Tooltip("Damage radius of every Arrow shot, % of R0, at any range. The last shot keeps the minimum radius.")]
        [SerializeField, Min(0f)] private float _damageRadiusPercent = 100f;

        public ArrowSettings()
        {
        }

        public ArrowSettings(float sectorCenterDegrees, float sectorWidthDegrees, float swingHalfPeriodSeconds,
            float rangeHalfPeriodSeconds, float shotCostPercent, float damageRadiusPercent)
        {
            _sectorCenterDegrees = sectorCenterDegrees;
            _sectorWidthDegrees = sectorWidthDegrees;
            _swingHalfPeriodSeconds = swingHalfPeriodSeconds;
            _rangeHalfPeriodSeconds = rangeHalfPeriodSeconds;
            _shotCostPercent = shotCostPercent;
            _damageRadiusPercent = damageRadiusPercent;
        }

        public float SectorCenterDegrees => _sectorCenterDegrees;
        public float SectorWidthDegrees => _sectorWidthDegrees;
        public float SwingHalfPeriodSeconds => _swingHalfPeriodSeconds;
        public float RangeHalfPeriodSeconds => _rangeHalfPeriodSeconds;
        public float ShotCostPercent => _shotCostPercent;
        public float DamageRadiusPercent => _damageRadiusPercent;
    }
}
