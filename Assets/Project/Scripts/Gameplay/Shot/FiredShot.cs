using UnityEngine;

namespace SelfBulletting.Gameplay.Shot
{
    /// <summary>
    /// A released shot: what it costs and where it goes. Classic shots roll to an obstacle (<see cref="Target"/>);
    /// shots of the aim modes are lobbed over the obstacles (<see cref="Lob"/>).
    /// </summary>
    public readonly struct FiredShot
    {
        public FiredShot(ShotRequest request, ShotTarget target)
        {
            Request = request;
            Target = target;
            Lob = default;
            IsLob = false;
        }

        public FiredShot(ShotRequest request, LobTarget lob)
        {
            Request = request;
            Target = default;
            Lob = lob;
            IsLob = true;
        }

        public ShotRequest Request { get; }

        /// <summary>Where a rolled shot touches its obstacle. Empty for a lobbed shot.</summary>
        public ShotTarget Target { get; }

        /// <summary>The flight of a lobbed shot. Empty for a rolled shot.</summary>
        public LobTarget Lob { get; }

        public bool IsLob { get; }

        /// <summary>Where the infection wave starts.</summary>
        public Vector3 ImpactPoint => IsLob ? Lob.ImpactPoint : Target.ImpactPoint;

        /// <summary>The obstacle the shot touched: it always explodes. None for a lobbed shot.</summary>
        public Collider DirectHit => IsLob ? null : Target.Obstacle;
    }
}
