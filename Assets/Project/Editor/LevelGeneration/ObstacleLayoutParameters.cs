using UnityEngine;

namespace SelfBulletting.Editor.LevelGeneration
{
    public readonly struct ObstacleLayoutParameters
    {
        public ObstacleLayoutParameters(int seed, Rect area, float cellSize, float jitter, float startClearRadius, float finishClearRadius)
        {
            Seed = seed;
            Area = area;
            CellSize = cellSize;
            Jitter = jitter;
            StartClearRadius = startClearRadius;
            FinishClearRadius = finishClearRadius;
        }

        public int Seed { get; }

        /// <summary>Ground rectangle: x is the world X, y is the world Z.</summary>
        public Rect Area { get; }

        public float CellSize { get; }
        public float Jitter { get; }
        public float StartClearRadius { get; }
        public float FinishClearRadius { get; }
    }
}
