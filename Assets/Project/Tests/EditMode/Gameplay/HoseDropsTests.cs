using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Gameplay.Hose;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// A drop flies a 15 m arc with a 2 m apex at 15 m/s, so the flight takes a second. Fires are a fake wall across
    /// the field: the arc clears a 1.4 m fire at the apex but touches it on the way down.
    /// </summary>
    public sealed class HoseDropsTests
    {
        private const float Tolerance = 0.0001f;
        private const float DropRadius = 0.12f;
        private const float FireHeight = 1.4f;
        private const float ApexHeight = 2f;
        private const float FarAway = 1000f;
        private const int MaxFrames = 64;

        private static readonly ArcPath _arc = new ArcPath(new Vector3(0f, DropRadius, 0f), new Vector3(0f, DropRadius, 15f), ApexHeight);

        private readonly List<DropImpact> _impacts = new List<DropImpact>();

        [Test]
        public void Tick_FlightTakesHorizontalLengthOverSpeed()
        {
            HoseDrops drops = CreateDrops(new WallSweep(FarAway, FireHeight));
            drops.TryLaunch(_arc, DropRadius);

            drops.Tick(0.5f, _impacts);
            Assert.AreEqual(0, _impacts.Count);
            Assert.AreEqual(_arc.Evaluate(0.5f).z, drops.InFlight[0].Center.z, Tolerance);

            drops.Tick(0.5f, _impacts);
            Assert.AreEqual(1, _impacts.Count);
            Assert.AreEqual(0, drops.Count);
        }

        [Test]
        public void Tick_NothingTouched_LandsOnTheGroundAtTheEndOfTheArc()
        {
            HoseDrops drops = CreateDrops(new WallSweep(FarAway, FireHeight));
            drops.TryLaunch(_arc, DropRadius);

            DropImpact impact = FlyUntilImpact(drops);

            Assert.IsTrue(impact.IsOnGround);
            Assert.AreEqual(_arc.End.z, impact.Center.z, Tolerance);
        }

        [Test]
        public void Tick_StopsOnTheFirstFireItTouchesOnTheWayDown()
        {
            HoseDrops drops = CreateDrops(new WallSweep(12f, FireHeight));
            drops.TryLaunch(_arc, DropRadius);

            DropImpact impact = FlyUntilImpact(drops);

            Assert.IsFalse(impact.IsOnGround);
            Assert.AreEqual(12f, impact.Center.z, Tolerance);
            Assert.AreEqual(DropRadius, impact.DropRadius, Tolerance);
        }

        [Test]
        public void Tick_FliesOverAFireUnderTheApex()
        {
            HoseDrops drops = CreateDrops(new WallSweep(7.5f, FireHeight));
            drops.TryLaunch(_arc, DropRadius);

            Assert.IsTrue(FlyUntilImpact(drops).IsOnGround);
        }

        [Test]
        public void Tick_FeelsTheFiresWithTheTouchSphere()
        {
            var sweep = new WallSweep(FarAway, FireHeight);
            HoseDrops drops = CreateDrops(sweep);
            drops.TryLaunch(_arc, DropRadius);

            drops.Tick(TestSettings.Frame, _impacts);

            Assert.AreEqual(DropRadius * TestSettings.TouchRadiusScale, sweep.LastRadius, Tolerance);
        }

        [Test]
        public void TryLaunch_FullPool_LaunchesNothing()
        {
            HoseDrops drops = CreateDrops(new WallSweep(FarAway, FireHeight), maxDropsInFlight: 2);

            Assert.IsTrue(drops.TryLaunch(_arc, DropRadius));
            Assert.IsTrue(drops.TryLaunch(_arc, DropRadius));
            Assert.IsFalse(drops.TryLaunch(_arc, DropRadius));
            Assert.AreEqual(2, drops.Count);
        }

        [Test]
        public void ArcProbe_FindsTheFireTheNextDropWillHit()
        {
            DropImpact impact = ArcProbe.Find(_arc, DropRadius, DropRadius * TestSettings.TouchRadiusScale,
                TestSettings.PreviewSegments, new WallSweep(12f, FireHeight));

            Assert.IsFalse(impact.IsOnGround);
            Assert.AreEqual(12f, impact.Center.z, Tolerance);
        }

        [Test]
        public void ArcProbe_NothingOnTheArc_LandsAtTheEnd()
        {
            DropImpact impact = ArcProbe.Find(_arc, DropRadius, DropRadius * TestSettings.TouchRadiusScale,
                TestSettings.PreviewSegments, new WallSweep(FarAway, FireHeight));

            Assert.IsTrue(impact.IsOnGround);
            Assert.AreEqual(_arc.End, impact.Center);
        }

        [Test]
        public void ArcPath_Direction_RisesThenLevelsThenFalls()
        {
            Assert.Greater(_arc.GetDirection(0f).y, 0f);
            Assert.AreEqual(0f, _arc.GetDirection(0.5f).y, Tolerance);
            Assert.Less(_arc.GetDirection(1f).y, 0f);
            Assert.AreEqual(1f, _arc.GetDirection(0.3f).magnitude, Tolerance);
        }

        private static HoseDrops CreateDrops(IObstacleSweep sweep, int maxDropsInFlight = 32) =>
            new HoseDrops(TestSettings.Hose(maxDropsInFlight: maxDropsInFlight),
                new DropSize(TestSettings.Hose(), TestSettings.Limits()), sweep);

        private DropImpact FlyUntilImpact(HoseDrops drops)
        {
            for (int i = 0; i < MaxFrames; i++)
            {
                drops.Tick(TestSettings.Frame, _impacts);
                if (_impacts.Count > 0)
                {
                    return _impacts[0];
                }
            }

            Assert.Fail("The drop never hit anything.");
            return default;
        }

        /// <summary>A fire as wide as the field at <c>z</c>: touched when the sphere crosses it below its top.</summary>
        private sealed class WallSweep : IObstacleSweep
        {
            private readonly float _z;
            private readonly float _height;

            public WallSweep(float z, float height)
            {
                _z = z;
                _height = height;
            }

            public float LastRadius { get; private set; }

            public bool TrySweep(Vector3 from, Vector3 to, float radius, out Vector3 contactCenter, out Collider obstacle)
            {
                LastRadius = radius;
                obstacle = null;

                if (from.z < _z && to.z >= _z)
                {
                    Vector3 crossing = Vector3.Lerp(from, to, (_z - from.z) / (to.z - from.z));
                    if (crossing.y - radius < _height)
                    {
                        contactCenter = crossing;
                        return true;
                    }
                }

                contactCenter = to;
                return false;
            }
        }
    }
}
