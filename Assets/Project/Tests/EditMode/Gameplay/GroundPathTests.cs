using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Gameplay.Paths;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class GroundPathTests
    {
        private const float Tolerance = 0.001f;
        private const float Spacing = 0.5f;

        [Test]
        public void Straight_HasTwoPointsAndTheirDistance()
        {
            GroundPath path = GroundPath.Straight(Vector3.zero, new Vector3(3f, 0f, 4f));

            Assert.AreEqual(2, path.Points.Count);
            Assert.AreEqual(5f, path.Length, Tolerance);
            Assert.AreEqual(new Vector3(0.6f, 0f, 0.8f), path.FinishDirection);
        }

        [Test]
        public void GetPoint_WalksAlongThePathAndClampsToItsEnds()
        {
            GroundPath path = GroundPath.Straight(Vector3.zero, new Vector3(0f, 0f, 10f));

            Assert.AreEqual(4f, path.GetPoint(4f).z, Tolerance);
            Assert.AreEqual(Vector3.zero, path.GetPoint(-1f));
            Assert.AreEqual(path.Finish, path.GetPoint(100f));
        }

        [Test]
        public void Smooth_PassesThroughEveryControlPoint()
        {
            Vector3[] controls = { Vector3.zero, new Vector3(4f, 0f, 6f), new Vector3(0f, 0f, 12f), new Vector3(6f, 0f, 20f) };

            GroundPath path = GroundPath.Smooth(controls, Spacing);

            Assert.AreEqual(controls[0], path.Start);
            Assert.AreEqual(controls[3], path.Finish);
            foreach (Vector3 control in controls)
            {
                Assert.Less(DistanceToPolyline(path.Points, control), Tolerance);
            }
        }

        [Test]
        public void Smooth_KeepsPointsCloseAndBendsAwayFromTheStraightLine()
        {
            Vector3[] controls = { Vector3.zero, new Vector3(4f, 0f, 6f), new Vector3(0f, 0f, 12f) };

            GroundPath path = GroundPath.Smooth(controls, Spacing);

            for (int i = 1; i < path.Points.Count; i++)
            {
                Assert.LessOrEqual(Vector3.Distance(path.Points[i - 1], path.Points[i]), Spacing * 2f);
            }

            Assert.Greater(path.Length, 12f);
            Assert.Greater(path.GetPoint(path.Length * 0.5f).x, 3f);
        }

        [Test]
        public void Smooth_NeverOvershootsBetweenCloseAndFarPoints()
        {
            // A centripetal spline does not loop past the near point when the next one is far away.
            Vector3[] controls = { Vector3.zero, new Vector3(0f, 0f, 1f), new Vector3(10f, 0f, 1f) };

            GroundPath path = GroundPath.Smooth(controls, Spacing);

            foreach (Vector3 point in path.Points)
            {
                Assert.LessOrEqual(point.z, 1.5f);
                Assert.GreaterOrEqual(point.x, -0.5f);
            }
        }

        [Test]
        public void Smooth_OnlyStartAndFinish_IsStraight()
        {
            GroundPath path = GroundPath.Smooth(new[] { Vector3.zero, new Vector3(0f, 0f, 8f) }, Spacing);

            Assert.AreEqual(8f, path.Length, Tolerance);
            foreach (Vector3 point in path.Points)
            {
                Assert.AreEqual(0f, point.x, Tolerance);
            }
        }

        [Test]
        public void Smooth_FinishDirection_FollowsTheLastPieceOfTheCurve()
        {
            Vector3[] controls = { Vector3.zero, new Vector3(0f, 0f, 10f), new Vector3(10f, 0f, 10f) };

            GroundPath path = GroundPath.Smooth(controls, Spacing);

            Assert.Greater(path.FinishDirection.x, 0.9f);
        }

        [Test]
        public void FlatDistanceTo_MeasuresToTheNearestSegmentOnTheGround()
        {
            GroundPath path = GroundPath.Straight(Vector3.zero, new Vector3(0f, 0f, 10f));

            Assert.AreEqual(2f, path.FlatDistanceTo(new Vector3(2f, 5f, 4f)), Tolerance);
            Assert.AreEqual(5f, path.FlatDistanceTo(new Vector3(3f, 0f, 14f)), Tolerance);
        }

        private static float DistanceToPolyline(IReadOnlyList<Vector3> points, Vector3 target)
        {
            float best = float.MaxValue;
            foreach (Vector3 point in points)
            {
                best = Mathf.Min(best, Vector3.Distance(point, target));
            }

            return best;
        }
    }
}
