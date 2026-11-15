using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Gameplay.Paths
{
    /// <summary>
    /// Centripetal Catmull-Rom spline: a smooth curve through every control point that never loops or overshoots
    /// between close points. The ends get mirrored phantom points, so the curve starts and ends on the first and last point.
    /// </summary>
    public static class CatmullRomSpline
    {
        private const float Alpha = 0.5f;
        private const float MinSegmentLength = 0.0001f;

        /// <summary>
        /// Fills <paramref name="results"/> with points along the curve, no farther apart than about <paramref name="spacing"/>.
        /// The control points themselves are on the result.
        /// </summary>
        public static void Sample(IReadOnlyList<Vector3> controlPoints, float spacing, List<Vector3> results)
        {
            results.Clear();
            List<Vector3> points = WithoutDuplicates(controlPoints);
            if (points.Count == 0)
            {
                return;
            }

            results.Add(points[0]);
            for (int i = 0; i < points.Count - 1; i++)
            {
                Vector3 p0 = i > 0 ? points[i - 1] : Mirror(points[i + 1], points[i]);
                Vector3 p1 = points[i];
                Vector3 p2 = points[i + 1];
                Vector3 p3 = i + 2 < points.Count ? points[i + 2] : Mirror(points[i], points[i + 1]);

                int steps = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(p1, p2) / spacing));
                for (int step = 1; step <= steps; step++)
                {
                    results.Add(step == steps ? p2 : Evaluate(p0, p1, p2, p3, (float)step / steps));
                }
            }
        }

        /// <param name="share">0 — <paramref name="p1"/>, 1 — <paramref name="p2"/>.</param>
        public static Vector3 Evaluate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float share)
        {
            float t0 = 0f;
            float t1 = NextKnot(t0, p0, p1);
            float t2 = NextKnot(t1, p1, p2);
            float t3 = NextKnot(t2, p2, p3);
            float t = Mathf.Lerp(t1, t2, share);

            Vector3 a1 = Blend(p0, p1, t0, t1, t);
            Vector3 a2 = Blend(p1, p2, t1, t2, t);
            Vector3 a3 = Blend(p2, p3, t2, t3, t);
            Vector3 b1 = Blend(a1, a2, t0, t2, t);
            Vector3 b2 = Blend(a2, a3, t1, t3, t);
            return Blend(b1, b2, t1, t2, t);
        }

        private static float NextKnot(float knot, Vector3 from, Vector3 to) =>
            knot + Mathf.Max(Mathf.Pow(Vector3.Distance(from, to), Alpha), MinSegmentLength);

        private static Vector3 Blend(Vector3 from, Vector3 to, float fromKnot, float toKnot, float t) =>
            (toKnot - t) / (toKnot - fromKnot) * from + (t - fromKnot) / (toKnot - fromKnot) * to;

        // The phantom point behind an end of the curve: the neighbour reflected through the end.
        private static Vector3 Mirror(Vector3 neighbour, Vector3 end) => end + (end - neighbour);

        private static List<Vector3> WithoutDuplicates(IReadOnlyList<Vector3> controlPoints)
        {
            var points = new List<Vector3>(controlPoints.Count);
            foreach (Vector3 point in controlPoints)
            {
                if (points.Count == 0 || Vector3.Distance(points[points.Count - 1], point) > MinSegmentLength)
                {
                    points.Add(point);
                }
            }

            return points;
        }
    }
}
