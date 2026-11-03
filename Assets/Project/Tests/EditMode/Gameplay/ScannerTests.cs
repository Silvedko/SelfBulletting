using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Scanning;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// Real physics queries against temporary boxes, placed far from anything in the open scene.
    /// </summary>
    public sealed class ScannerTests
    {
        private const int DefaultLayerMask = 1;
        private const float Radius = 0.4f;
        private static readonly Vector3 Origin = new Vector3(5000f, 0f, 5000f);

        private readonly List<GameObject> _created = new List<GameObject>();
        private PhysicsSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _settings = new PhysicsSettings(DefaultLayerMask, 64);
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
        public void TryFindFirstObstacle_ReturnsNearestOfSeveralHits()
        {
            CreateBox(new Vector3(0f, 0f, 10f));
            Collider nearest = CreateBox(new Vector3(0f, 0f, 5f));
            CreateBox(new Vector3(0f, 0f, 15f));

            bool found = new PathScanner(_settings).TryFindFirstObstacle(Origin, Origin + Vector3.forward * 20f, Radius, out RaycastHit hit);

            Assert.IsTrue(found);
            Assert.AreSame(nearest, hit.collider);
        }

        [Test]
        public void IsClear_NothingOnPath_ReturnsTrue()
        {
            CreateBox(new Vector3(5f, 0f, 10f));

            Assert.IsTrue(new PathScanner(_settings).IsClear(Origin, Origin + Vector3.forward * 20f, Radius));
        }

        [Test]
        public void TryFindFirstObstacle_OverlappingAtStart_ReportsRealContactPoint()
        {
            CreateBox(Vector3.zero);

            bool found = new PathScanner(_settings).TryFindFirstObstacle(Origin, Origin + Vector3.forward * 20f, Radius, out RaycastHit hit);

            Assert.IsTrue(found);
            Assert.Less(Vector3.Distance(hit.point, Origin), 1f);
        }

        [Test]
        public void FindInRadius_ReturnsCollidersNearestFirst()
        {
            Collider far = CreateBox(new Vector3(3f, 0f, 0f));
            Collider near = CreateBox(new Vector3(1f, 0f, 0f));
            Collider middle = CreateBox(new Vector3(2f, 0f, 0f));
            var results = new List<Collider>();

            int count = new ObstacleScanner(_settings).FindInRadius(Origin, 4f, results);

            Assert.AreEqual(3, count);
            CollectionAssert.AreEqual(new[] { near, middle, far }, results);
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
