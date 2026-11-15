using UnityEngine;

namespace SelfBulletting.Gameplay.Shot
{
    /// <summary>
    /// Where a lobbed shot starts, flies and lands. It flies over the obstacles, so it has no obstacle of its own:
    /// the infection starts at the landing point.
    /// </summary>
    public readonly struct LobTarget
    {
        public LobTarget(ArcPath arc, Vector3 impactPoint)
        {
            Arc = arc;
            ImpactPoint = impactPoint;
        }

        /// <summary>The flight of the shot ball centre: from in front of the player ball to touching the ground.</summary>
        public ArcPath Arc { get; }

        public Vector3 LaunchCenter => Arc.Start;
        public Vector3 LandingCenter => Arc.End;

        /// <summary>Centre of the infection: the shot ball centre at the moment it lands.</summary>
        public Vector3 ImpactPoint { get; }
    }
}
