using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// The ball stands at (2, 0, 2) in a 12 × 24 m field, like in the levels. Ranges are 3–24 m.
    /// </summary>
    public sealed class LobTargetingTests
    {
        private const float Tolerance = 0.001f;
        private const float BallRadius = 1f;
        private const float ShotRadius = 0.3f;
        private static readonly Vector3 Ball = new Vector3(2f, 0f, 2f);
        private static readonly Rect Field = new Rect(0f, 0f, 12f, 24f);

        private LobTargeting _targeting;

        [SetUp]
        public void SetUp()
        {
            _targeting = new LobTargeting(new LobSettings(3f, 24f, 0.25f, 1f), Ball, Field);
        }

        [TestCase(0f, 3f)]
        [TestCase(0.5f, 13.5f)]
        [TestCase(1f, 24f)]
        [TestCase(2f, 24f)]
        public void GetRange_MapsShareToRange(float share, float expected)
        {
            Assert.AreEqual(expected, _targeting.GetRange(share), Tolerance);
        }

        [Test]
        public void Find_LandsAlongTheDirectionAtTheRange()
        {
            LobTarget target = _targeting.Find(Vector3.forward, 0.25f, BallRadius, ShotRadius);

            Assert.AreEqual(Ball.x, target.LandingCenter.x, Tolerance);
            Assert.AreEqual(Ball.z + 8.25f, target.LandingCenter.z, Tolerance);
            Assert.AreEqual(ShotRadius, target.LandingCenter.y, Tolerance);
            Assert.AreEqual(target.LandingCenter, target.ImpactPoint);
        }

        [Test]
        public void Find_LaunchesTheShotInFrontOfTheBallOnTheGround()
        {
            LobTarget target = _targeting.Find(Vector3.right, 0.5f, BallRadius, ShotRadius);

            Assert.AreEqual(Ball.x + BallRadius + ShotRadius, target.LaunchCenter.x, Tolerance);
            Assert.AreEqual(ShotRadius, target.LaunchCenter.y, Tolerance);
        }

        [Test]
        public void Find_NeverLandsPastTheFieldEdge()
        {
            LobTarget right = _targeting.Find(Vector3.right, 1f, BallRadius, ShotRadius);
            Vector3 diagonal = new Vector3(1f, 0f, 1f).normalized;
            LobTarget corner = _targeting.Find(diagonal, 1f, BallRadius, ShotRadius);

            Assert.AreEqual(Field.xMax, right.LandingCenter.x, Tolerance);
            Assert.LessOrEqual(corner.LandingCenter.x, Field.xMax + Tolerance);
            Assert.LessOrEqual(corner.LandingCenter.z, Field.yMax + Tolerance);
        }

        [Test]
        public void Find_NeverLandsBeforeTheLaunchPoint()
        {
            // Looking out of the field: no room at all, so the shot still lands just past the shot in front of the ball.
            LobTarget target = _targeting.Find(Vector3.back, 1f, BallRadius, ShotRadius);

            float launchDistance = Vector3.Distance(Flat(target.LaunchCenter), Flat(Ball));
            float landingDistance = Vector3.Distance(Flat(target.LandingCenter), Flat(Ball));
            Assert.Greater(landingDistance, launchDistance + ShotRadius);
        }

        [Test]
        public void Find_ArcGetsHigherWithDistanceButNotBelowTheMinimum()
        {
            LobTarget near = _targeting.Find(Vector3.forward, 0f, BallRadius, ShotRadius);
            LobTarget far = _targeting.Find(Vector3.forward, 0.75f, BallRadius, ShotRadius);

            Assert.AreEqual(1f, near.Arc.ApexHeight, Tolerance);
            Assert.AreEqual(18.75f * 0.25f, far.Arc.ApexHeight, Tolerance);
        }

        [Test]
        public void ArcPath_StartsAndEndsOnItsPointsAndPeaksInTheMiddle()
        {
            var arc = new ArcPath(Vector3.zero, new Vector3(0f, 0f, 10f), 2f);

            Assert.AreEqual(Vector3.zero, arc.Evaluate(0f));
            Assert.AreEqual(new Vector3(0f, 0f, 10f), arc.Evaluate(1f));
            Assert.AreEqual(2f, arc.Evaluate(0.5f).y, Tolerance);
            Assert.Less(arc.Evaluate(0.25f).y, 2f);
            Assert.AreEqual(10f, arc.HorizontalLength, Tolerance);
        }

        [Test]
        public void FiredShot_Lobbed_HasNoDirectHitAndStartsTheWaveAtTheLanding()
        {
            LobTarget target = _targeting.Find(Vector3.forward, 0.5f, BallRadius, ShotRadius);

            var shot = new FiredShot(new ShotRequest(10f, 1f, 0.5f), target);

            Assert.IsTrue(shot.IsLob);
            Assert.IsNull(shot.DirectHit);
            Assert.AreEqual(target.ImpactPoint, shot.ImpactPoint);
        }

        private static Vector3 Flat(Vector3 point) => new Vector3(point.x, 0f, point.z);
    }
}
