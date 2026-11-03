using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Editor.LevelGeneration;
using UnityEngine;

namespace SelfBulletting.Tests.Editor
{
    public sealed class ObstacleLayoutTests
    {
        private const float Jitter = 0.3f;
        private const float StartClear = 2.5f;
        private const float FinishClear = 3f;
        private static readonly Rect Area = new Rect(0f, 0f, 12f, 24f);
        private static readonly Vector3 Start = new Vector3(2f, 0f, 2f);
        private static readonly Vector3 Finish = new Vector3(10f, 0f, 22f);

        [Test]
        public void Generate_SameSeed_GivesSameLayout()
        {
            CollectionAssert.AreEqual(Generate(7), Generate(7));
        }

        [Test]
        public void Generate_DifferentSeed_GivesDifferentLayout()
        {
            CollectionAssert.AreNotEqual(Generate(7), Generate(8));
        }

        [Test]
        public void Generate_KeepsStartAndFinishClear()
        {
            foreach (Vector3 position in Generate(7))
            {
                Assert.GreaterOrEqual(FlatDistance(position, Start), StartClear);
                Assert.GreaterOrEqual(FlatDistance(position, Finish), FinishClear);
            }
        }

        [Test]
        public void Generate_StaysInsideAreaPlusJitter()
        {
            List<Vector3> positions = Generate(7);

            Assert.Greater(positions.Count, 0);
            foreach (Vector3 position in positions)
            {
                Assert.That(position.x, Is.InRange(Area.xMin - Jitter, Area.xMax + Jitter));
                Assert.That(position.z, Is.InRange(Area.yMin - Jitter, Area.yMax + Jitter));
            }
        }

        private static List<Vector3> Generate(int seed) =>
            ObstacleLayout.Generate(new ObstacleLayoutParameters(seed, Area, 0.9f, Jitter, StartClear, FinishClear), Start, Finish);

        private static float FlatDistance(Vector3 a, Vector3 b) => Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
}
