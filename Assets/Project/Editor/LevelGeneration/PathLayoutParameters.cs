using UnityEngine;

namespace SelfBulletting.Editor.LevelGeneration
{
    public readonly struct PathLayoutParameters
    {
        public PathLayoutParameters(int seed, int bendCount, float minOffset, float maxOffset, Rect area, float margin)
        {
            Seed = seed;
            BendCount = bendCount;
            MinOffset = minOffset;
            MaxOffset = maxOffset;
            Area = area;
            Margin = margin;
        }

        public int Seed { get; }

        /// <summary>How many waypoints the curve bends through between the start and the finish.</summary>
        public int BendCount { get; }

        /// <summary>How far a waypoint moves sideways from the straight line start → finish, meters.</summary>
        public float MinOffset { get; }

        public float MaxOffset { get; }

        /// <summary>Ground rectangle: x is the world X, y is the world Z.</summary>
        public Rect Area { get; }

        /// <summary>Waypoints keep this far from the area edges, meters.</summary>
        public float Margin { get; }
    }
}
