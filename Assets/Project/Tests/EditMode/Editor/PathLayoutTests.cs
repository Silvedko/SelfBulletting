using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Editor.LevelGeneration;
using UnityEngine;

namespace SelfBulletting.Tests.Editor
{
    public sealed class PathLayoutTests
    {
        private const float MinOffset = 2.5f;
        private const float MaxOffset = 4.5f;
        private const float Margin = 2f;
        private static readonly Rect Area = new Rect(0f, 0f, 12f, 24f);
        private static readonly Vector3 Start = new Vector3(2f, 0f, 2f);
        private static readonly Vector3 Finish = new Vector3(10f, 0f, 22f);

        [Test]
        public void Generate_SameSeed_GivesSamePath()
        {
            CollectionAssert.AreEqual(Generate(7, 3), Generate(7, 3));
        }

        [Test]
        public void Generate_DifferentSeed_GivesDifferentPath()
        {
            CollectionAssert.AreNotEqual(Generate(7, 3), Generate(8, 3));
        }

        [TestCase(1)]
        [TestCase(3)]
        public void Generate_GivesOneWaypointPerBend(int bendCount)
        {
            Assert.AreEqual(bendCount, Generate(7, bendCount).Count);
        }

        [Test]
        public void Generate_StaysInsideAreaMinusMargin()
        {
            for (int seed = 0; seed < 20; seed++)
            {
                foreach (Vector3 waypoint in Generate(seed, 4))
                {
                    Assert.GreaterOrEqual(waypoint.x, Area.xMin + Margin);
                    Assert.LessOrEqual(waypoint.x, Area.xMax - Margin);
                    Assert.GreaterOrEqual(waypoint.z, Area.yMin + Margin);
                    Assert.LessOrEqual(waypoint.z, Area.yMax - Margin);
                }
            }
        }

        [Test]
        public void Generate_BendsToAlternatingSidesOfTheStraightLine()
        {
            // Wide area and no margin, so no waypoint gets clamped back towards the line.
            var parameters = new PathLayoutParameters(7, 4, MinOffset, MaxOffset, new Rect(-100f, -100f, 200f, 200f), 0f);
            List<Vector3> waypoints = PathLayout.Generate(parameters, Start, Finish);

            for (int i = 1; i < waypoints.Count; i++)
            {
                Assert.AreNotEqual(Mathf.Sign(SideOf(waypoints[i - 1])), Mathf.Sign(SideOf(waypoints[i])));
            }
        }

        private static List<Vector3> Generate(int seed, int bendCount) =>
            PathLayout.Generate(new PathLayoutParameters(seed, bendCount, MinOffset, MaxOffset, Area, Margin), Start, Finish);

        // Signed distance from the straight line start → finish; positive is to its right.
        private static float SideOf(Vector3 point)
        {
            Vector3 line = (Finish - Start).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, line);
            return Vector3.Dot(point - Start, right);
        }
    }
}
