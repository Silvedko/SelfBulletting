using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// Real physics: the ball stands at the origin and the path runs along +Z. Boxes are 1 m cubes standing on the ground.
    /// </summary>
    public sealed class ShotTargetingTests
    {
        private const int DefaultLayerMask = 1;
        private const float BallRadius = 1f;
        private const float Tolerance = 0.01f;
        private static readonly Vector3 Origin = new Vector3(-5000f, 0f, 5000f);

        private readonly List<GameObject> _created = new List<GameObject>();
        private PathScanner _scanner;
        private ShotTargeting _targeting;

        [SetUp]
        public void SetUp()
        {
            _scanner = new PathScanner(new PhysicsSettings(DefaultLayerMask, 64));
            _targeting = new ShotTargeting(_scanner, Origin, Origin + Vector3.forward * 20f);
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
        public void Find_ThinShotMissingEdgeBlocker_RollsOnToObstacleOnItsLine()
        {
            Collider edge = CreateBox(new Vector3(0.9f, 0.5f, 8f));
            Collider farOnAxis = CreateBox(new Vector3(0f, 0.5f, 12f));
            RaycastHit blocker = FindCorridorBlocker();

            ShotTarget thin = _targeting.Find(BallRadius, 0.15f, blocker);
            ShotTarget big = _targeting.Find(BallRadius, 0.6f, blocker);

            Assert.AreSame(edge, blocker.collider);
            Assert.AreSame(farOnAxis, thin.Obstacle);
            Assert.AreEqual(11.5f, thin.ImpactPoint.z - Origin.z, Tolerance);
            Assert.AreSame(edge, big.Obstacle);
        }

        [Test]
        public void Find_NothingOnThinShotLine_RollsTowardsBlockerAndTouchesIt()
        {
            Collider edge = CreateBox(new Vector3(0.9f, 0.5f, 8f));
            RaycastHit blocker = FindCorridorBlocker();
            const float shotRadius = 0.15f;

            ShotTarget target = _targeting.Find(BallRadius, shotRadius, blocker);

            Assert.AreSame(edge, target.Obstacle);
            Assert.Greater(target.ContactCenter.x - Origin.x, 0f);
            Assert.AreEqual(shotRadius, Vector3.Distance(target.ContactCenter, target.ImpactPoint), Tolerance);
        }

        [Test]
        public void Find_BigShot_TouchesEdgeBlockerEarlierAtAnotherPoint()
        {
            Collider edge = CreateBox(new Vector3(0.9f, 0.5f, 8f));
            RaycastHit blocker = FindCorridorBlocker();

            ShotTarget thin = _targeting.Find(BallRadius, 0.15f, blocker);
            ShotTarget big = _targeting.Find(BallRadius, 0.6f, blocker);

            Assert.AreSame(edge, big.Obstacle);
            Assert.Less(big.ContactCenter.z, thin.ContactCenter.z);
            Assert.AreNotEqual(thin.ImpactPoint, big.ImpactPoint);
        }

        [Test]
        public void Find_ObstacleOnAxisBeforeBlocker_IsTouchedFirst()
        {
            CreateBox(new Vector3(0.9f, 0.5f, 8f));
            Collider onAxis = CreateBox(new Vector3(0f, 0.5f, 6f));
            RaycastHit blocker = FindCorridorBlocker();

            ShotTarget target = _targeting.Find(BallRadius, 0.15f, blocker);

            Assert.AreSame(onAxis, target.Obstacle);
            Assert.AreEqual(5.5f, target.ImpactPoint.z - Origin.z, Tolerance);
        }

        [Test]
        public void Find_LaunchesShotInFrontOfBallOnTheGround()
        {
            CreateBox(new Vector3(0f, 0.5f, 8f));
            RaycastHit blocker = FindCorridorBlocker();

            ShotTarget target = _targeting.Find(BallRadius, 0.3f, blocker);

            Assert.AreEqual(1.3f, target.LaunchCenter.z - Origin.z, Tolerance);
            Assert.AreEqual(0.3f, target.LaunchCenter.y - Origin.y, Tolerance);
        }

        private RaycastHit FindCorridorBlocker()
        {
            Vector3 ballCenter = Origin + Vector3.up * BallRadius;
            Assert.IsTrue(_scanner.TryFindFirstObstacle(ballCenter, ballCenter + Vector3.forward * 20f, BallRadius, out RaycastHit blocker));
            return blocker;
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
