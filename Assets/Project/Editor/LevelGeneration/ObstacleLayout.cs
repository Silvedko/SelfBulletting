using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Editor.LevelGeneration
{
    /// <summary>
    /// Jittered grid of obstacle positions: one obstacle per cell, shifted randomly inside its cell,
    /// with clear zones around the start and the finish. The same seed always gives the same layout.
    /// </summary>
    public static class ObstacleLayout
    {
        public static List<Vector3> Generate(ObstacleLayoutParameters parameters, Vector3 start, Vector3 finish)
        {
            var random = new System.Random(parameters.Seed);
            var positions = new List<Vector3>();
            Rect area = parameters.Area;
            float halfCell = parameters.CellSize * 0.5f;

            for (float z = area.yMin + halfCell; z < area.yMax; z += parameters.CellSize)
            {
                for (float x = area.xMin + halfCell; x < area.xMax; x += parameters.CellSize)
                {
                    var position = new Vector3(
                        x + NextJitter(random, parameters.Jitter),
                        0f,
                        z + NextJitter(random, parameters.Jitter));

                    if (IsInside(position, start, parameters.StartClearRadius) || IsInside(position, finish, parameters.FinishClearRadius))
                    {
                        continue;
                    }

                    positions.Add(position);
                }
            }

            return positions;
        }

        private static float NextJitter(System.Random random, float jitter) => ((float)random.NextDouble() * 2f - 1f) * jitter;

        private static bool IsInside(Vector3 position, Vector3 center, float radius)
        {
            Vector2 flatOffset = new Vector2(position.x - center.x, position.z - center.z);
            return flatOffset.sqrMagnitude < radius * radius;
        }
    }
}
