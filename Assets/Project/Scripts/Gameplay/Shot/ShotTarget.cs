using UnityEngine;

namespace SelfBulletting.Gameplay.Shot
{
    /// <summary>
    /// Where a shot of a given size starts, where it touches an obstacle and which obstacle that is.
    /// </summary>
    public readonly struct ShotTarget
    {
        public ShotTarget(Vector3 launchCenter, Vector3 contactCenter, Vector3 impactPoint, Collider obstacle)
        {
            LaunchCenter = launchCenter;
            ContactCenter = contactCenter;
            ImpactPoint = impactPoint;
            Obstacle = obstacle;
        }

        /// <summary>Centre of the shot ball in front of the player ball, before it rolls.</summary>
        public Vector3 LaunchCenter { get; }

        /// <summary>Centre of the shot ball at the moment it touches the obstacle.</summary>
        public Vector3 ContactCenter { get; }

        /// <summary>The touch point on the obstacle surface: the infection wave starts here.</summary>
        public Vector3 ImpactPoint { get; }

        public Collider Obstacle { get; }
    }
}
