using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Editor.LevelGeneration
{
    /// <summary>
    /// Waypoints of a curved path: evenly spaced along the straight line from the start to the finish, each pushed
    /// sideways by a random offset to alternating sides, so the curve snakes across the field. The same seed always
    /// gives the same waypoints.
    /// </summary>
    public static class PathLayout
    {
        public static List<Vector3> Generate(PathLayoutParameters parameters, Vector3 start, Vector3 finish)
        {
            var random = new System.Random(parameters.Seed);
            var waypoints = new List<Vector3>(parameters.BendCount);

            Vector3 line = finish - start;
            line.y = 0f;
            Vector3 side = Vector3.Cross(Vector3.up, line.normalized);
            float firstSide = random.NextDouble() < 0.5 ? -1f : 1f;

            for (int bend = 1; bend <= parameters.BendCount; bend++)
            {
                float along = (float)bend / (parameters.BendCount + 1);
                float offset = Mathf.Lerp(parameters.MinOffset, parameters.MaxOffset, (float)random.NextDouble());
                float sign = bend % 2 == 1 ? firstSide : -firstSide;
                Vector3 waypoint = Vector3.Lerp(start, finish, along) + side * (offset * sign);
                waypoints.Add(ClampToArea(waypoint, parameters.Area, parameters.Margin));
            }

            return waypoints;
        }

        private static Vector3 ClampToArea(Vector3 point, Rect area, float margin)
        {
            float x = Mathf.Clamp(point.x, area.xMin + margin, area.xMax - margin);
            float z = Mathf.Clamp(point.z, area.yMin + margin, area.yMax - margin);
            return new Vector3(x, point.y, z);
        }
    }
}
