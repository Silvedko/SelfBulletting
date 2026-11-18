using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// Where a drop ends: on the first fire it touched in flight, or on the ground at the end of its arc.
    /// The drop splashes there.
    /// </summary>
    public readonly struct DropImpact
    {
        private DropImpact(Vector3 center, float dropRadius, Collider touched, bool isOnGround)
        {
            Center = center;
            DropRadius = dropRadius;
            Touched = touched;
            IsOnGround = isOnGround;
        }

        /// <summary>The drop centre at the moment it hits.</summary>
        public Vector3 Center { get; }

        /// <summary>Meters: the splash grows with it.</summary>
        public float DropRadius { get; }

        /// <summary>The obstacle collider the drop touched; null on the ground.</summary>
        public Collider Touched { get; }

        public bool IsOnGround { get; }

        public static DropImpact OnObstacle(Vector3 center, float dropRadius, Collider touched) =>
            new DropImpact(center, dropRadius, touched, false);

        public static DropImpact OnGround(Vector3 center, float dropRadius) =>
            new DropImpact(center, dropRadius, null, true);
    }
}
