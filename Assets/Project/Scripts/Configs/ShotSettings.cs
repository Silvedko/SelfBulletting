using System;
using UnityEngine;

namespace SelfBulletting.Configs
{
    [Serializable]
    public sealed class ShotSettings
    {
        private const float LowestPercent = 1f;
        private const float HighestPercent = 99f;

        [Tooltip("Cost of the minimum shot and the critical ball size, % of R0.")]
        [SerializeField] private float _minShootPercent = 10f;

        [Tooltip("Cost of the maximum shot, % of R0. Must be above the minimum and below 100.")]
        [SerializeField] private float _maxShootPercent = 40f;

        [Tooltip("Damage radius at the minimum of the aim scale, % of R0.")]
        [SerializeField, Min(0f)] private float _damageRadiusMinPercent = 30f;

        [Tooltip("Damage radius at the maximum of the aim scale, % of R0.")]
        [SerializeField, Min(0f)] private float _damageRadiusMaxPercent = 200f;

        [Tooltip("Shot ball radius at the minimum of the aim scale, % of R0. While aiming it grows by exactly as much as the player ball shrinks. Not below the minimum shot cost.")]
        [SerializeField] private float _shotStartRadiusPercent = 20f;

        [Tooltip("Shot flight speed, m/s.")]
        [SerializeField, Min(0.1f)] private float _shotSpeed = 15f;

        public ShotSettings()
        {
        }

        public ShotSettings(float minShootPercent, float maxShootPercent, float damageRadiusMinPercent, float damageRadiusMaxPercent,
            float shotStartRadiusPercent)
        {
            _minShootPercent = minShootPercent;
            _maxShootPercent = maxShootPercent;
            _damageRadiusMinPercent = damageRadiusMinPercent;
            _damageRadiusMaxPercent = damageRadiusMaxPercent;
            _shotStartRadiusPercent = shotStartRadiusPercent;
            Validate();
        }

        public float MinShootPercent => _minShootPercent;
        public float MaxShootPercent => _maxShootPercent;
        public float DamageRadiusMinPercent => _damageRadiusMinPercent;
        public float DamageRadiusMaxPercent => _damageRadiusMaxPercent;
        public float ShotStartRadiusPercent => _shotStartRadiusPercent;
        public float ShotSpeed => _shotSpeed;

        public void Validate()
        {
            _minShootPercent = Mathf.Clamp(_minShootPercent, LowestPercent, HighestPercent - 1f);
            _maxShootPercent = Mathf.Clamp(_maxShootPercent, _minShootPercent + 1f, HighestPercent);
            _damageRadiusMaxPercent = Mathf.Max(_damageRadiusMinPercent, _damageRadiusMaxPercent);
            _shotStartRadiusPercent = Mathf.Max(_shotStartRadiusPercent, _minShootPercent);
        }
    }
}
