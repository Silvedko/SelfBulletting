using System;
using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Gameplay.Paths
{
    /// <summary>
    /// The path the ball takes from the start to the finish, as a polyline of ground points. Classic uses a straight
    /// path of two points; the aim modes use a smooth curve through the level's waypoints. The corridor check, the strip
    /// on the ground and the hops to the door all follow these points.
    /// </summary>
    public sealed class GroundPath
    {
        private readonly Vector3[] _points;
        private readonly float[] _distances;

        private GroundPath(IReadOnlyList<Vector3> points)
        {
            if (points.Count < 2)
            {
                throw new ArgumentException("A path needs at least two points.", nameof(points));
            }

            _points = new Vector3[points.Count];
            _distances = new float[points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                _points[i] = points[i];
                _distances[i] = i == 0 ? 0f : _distances[i - 1] + Vector3.Distance(points[i - 1], points[i]);
            }
        }

        public IReadOnlyList<Vector3> Points => _points;
        public float Length => _distances[_distances.Length - 1];
        public Vector3 Start => _points[0];
        public Vector3 Finish => _points[_points.Length - 1];

        /// <summary>Flat unit direction of the path where it reaches the finish.</summary>
        public Vector3 FinishDirection => Flat(_points[_points.Length - 1] - _points[_points.Length - 2]);

        public static GroundPath Straight(Vector3 from, Vector3 to) => new GroundPath(new[] { from, to });

        /// <param name="controlPoints">Start, waypoints, finish. With only the start and the finish the path is straight.</param>
        /// <param name="spacing">Rough distance between the polyline points, meters.</param>
        public static GroundPath Smooth(IReadOnlyList<Vector3> controlPoints, float spacing)
        {
            var points = new List<Vector3>();
            CatmullRomSpline.Sample(controlPoints, spacing, points);
            return points.Count >= 2 ? new GroundPath(points) : Straight(controlPoints[0], controlPoints[controlPoints.Count - 1]);
        }

        /// <param name="distance">Distance along the path from the start, clamped to the path.</param>
        public Vector3 GetPoint(float distance)
        {
            if (distance <= 0f)
            {
                return Start;
            }

            for (int i = 1; i < _points.Length; i++)
            {
                if (distance <= _distances[i])
                {
                    float segment = _distances[i] - _distances[i - 1];
                    float share = segment > Mathf.Epsilon ? (distance - _distances[i - 1]) / segment : 1f;
                    return Vector3.Lerp(_points[i - 1], _points[i], share);
                }
            }

            return Finish;
        }

        /// <summary>Distance on the ground (XZ) from the point to the nearest point of the path.</summary>
        public float FlatDistanceTo(Vector3 point)
        {
            var target = new Vector2(point.x, point.z);
            float nearest = float.MaxValue;
            for (int i = 1; i < _points.Length; i++)
            {
                var from = new Vector2(_points[i - 1].x, _points[i - 1].z);
                var to = new Vector2(_points[i].x, _points[i].z);
                Vector2 segment = to - from;
                float lengthSquared = segment.sqrMagnitude;
                float share = lengthSquared > Mathf.Epsilon ? Mathf.Clamp01(Vector2.Dot(target - from, segment) / lengthSquared) : 0f;
                nearest = Mathf.Min(nearest, Vector2.Distance(target, from + segment * share));
            }

            return nearest;
        }

        private static Vector3 Flat(Vector3 vector)
        {
            vector.y = 0f;
            return vector.normalized;
        }
    }
}
