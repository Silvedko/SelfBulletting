using System;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Ball
{
    /// <summary>
    /// Size of the player ball in percent of R0. Starts at 100% and never gets below the critical size.
    /// </summary>
    public sealed class BallModel
    {
        private const float FullSizePercent = 100f;

        private readonly ShotEconomy _economy;

        public BallModel(ShotEconomy economy)
        {
            _economy = economy;
            RadiusPercent = FullSizePercent;
        }

        public event Action RadiusChanged;

        public float RadiusPercent { get; private set; }
        public float WorldRadius => _economy.ToWorldRadius(RadiusPercent);

        public void Spend(float costPercent)
        {
            if (costPercent < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(costPercent), costPercent, "A shot cannot cost less than zero.");
            }

            RadiusPercent = Mathf.Max(RadiusPercent - costPercent, _economy.CriticalPercent);
            RadiusChanged?.Invoke();
        }
    }
}
