using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Paths;
using SelfBulletting.Gameplay.Scanning;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// Real physics: the path bends from the origin up +Z, right along +X and up again. Boxes are 1 m cubes on the ground.
    /// </summary>
    public sealed class CurvedPathScanTests
    {
        private const int DefaultLayerMask = 1;
        private const float BallRadius = 0.5f;
        private static readonly Vector3 Origin = new Vector3(-5000f, 0f, -5000f);

        private readonly List<GameObject> _created = new List<GameObject>();
        private PathScanner _scanner;
        private GroundPath _path;

        [SetUp]
        public void SetUp()
        {
            _scanner = new PathScanner(new PhysicsSettings(DefaultLayerMask, 64));
            _path = GroundPath.Smooth(new[]
            {
                Origin,
                Origin + new Vector3(0f, 0f, 6f),
                Origin + new Vector3(8f, 0f, 10f),
                Origin + new Vector3(8f, 0f, 18f)
            }, 0.5f);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        [Test]
        public void IsClear_ObstaclesOnTheStraightLineButOffTheCurve_OnlyTheCurveIsClear()
        {
            // A quarter and three quarters of the way along the straight line start → finish (8, 18).
            CreateBox(new Vector3(2f, 0.5f, 4.5f));
            CreateBox(new Vector3(6f, 0.5f, 13.5f));
            GroundPath straight = GroundPath.Straight(Origin, Origin + new Vector3(8f, 0f, 18f));

            Assert.IsFalse(_scanner.IsClear(straight, BallRadius));
            Assert.IsTrue(_scanner.IsClear(_path, BallRadius));
        }

        [Test]
        public void TryFindFirstObstacle_ObstacleOnTheBend_IsFound()
        {
            Collider onBend = CreateBox(new Vector3(8f, 0.5f, 10f));

            Assert.IsTrue(_scanner.TryFindFirstObstacle(_path, BallRadius, out RaycastHit hit));
            Assert.AreSame(onBend, hit.collider);
        }

        [Test]
        public void TryFindFirstObstacle_SeveralOnTheCurve_ReturnsTheFirstAlongThePath()
        {
            // Created out of order: the one that comes first along the path wins, not the first created.
            CreateBox(new Vector3(8f, 0.5f, 16f));
            Collider first = CreateBox(new Vector3(0f, 0.5f, 4f));
            CreateBox(new Vector3(8f, 0.5f, 12f));

            Assert.IsTrue(_scanner.TryFindFirstObstacle(_path, BallRadius, out RaycastHit hit));
            Assert.AreSame(first, hit.collider);
        }

        private Collider CreateBox(Vector3 offset)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.position = Origin + offset;
            _created.Add(box);
            Physics.SyncTransforms();
            return box.GetComponent<Collider>();
        }
    }
}
