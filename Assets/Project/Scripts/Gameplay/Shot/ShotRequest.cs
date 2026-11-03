namespace SelfBulletting.Gameplay.Shot
{
    public readonly struct ShotRequest
    {
        public ShotRequest(float costPercent, float damageWorldRadius, float scale)
        {
            CostPercent = costPercent;
            DamageWorldRadius = damageWorldRadius;
            Scale = scale;
        }

        /// <summary>How much of the ball the shot takes, % of R0.</summary>
        public float CostPercent { get; }

        /// <summary>Infection radius around the impact point, meters.</summary>
        public float DamageWorldRadius { get; }

        /// <summary>Aim scale value the shot was released at, 0..1.</summary>
        public float Scale { get; }
    }
}
