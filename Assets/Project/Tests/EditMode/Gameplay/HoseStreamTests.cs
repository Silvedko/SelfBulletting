using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Hose;
using SelfBulletting.Gameplay.Rules;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// The water on an empty field: pouring drains the ball for real and launches the drops along the aim.
    /// </summary>
    public sealed class HoseStreamTests
    {
        private const float Tolerance = 0.0001f;
        private const float HalfRange = 0.5f;
        private const int FramesPerSecond = 16;

        private BallModel _ball;
        private HoseStream _stream;
        private RoundRules _rules;

        [SetUp]
        public void SetUp()
        {
            HoseSettings settings = TestSettings.Hose();
            BallLimits limits = TestSettings.Limits();
            var size = new DropSize(settings, limits);
            var targeting = new LobTargeting(new LobSettings(3f, 24f, 0.25f, 1f), Vector3.zero, new Rect(-50f, -50f, 100f, 100f));

            _ball = new BallModel(limits);
            _stream = new HoseStream(new HoseFlow(settings), new HoseDrops(settings, size, new EmptyField()), size, targeting,
                _ball);
            _rules = new RoundRules(limits);
        }

        [Test]
        public void Pour_DrainsTheBallWhileTheWaterFlows()
        {
            for (int i = 0; i < FramesPerSecond; i++)
            {
                _stream.Pour(Vector3.forward, HalfRange, TestSettings.Frame, out _);
            }

            Assert.AreEqual(100f - TestSettings.DrainPercentPerSecond, _ball.RadiusPercent, Tolerance);
        }

        // (100 − 10) / 2 = 45 s of water: halfway through the health in the HUD is half full.
        [Test]
        public void Pour_HealthFallsWithTheWater()
        {
            for (int i = 0; i < FramesPerSecond * 45 / 2; i++)
            {
                _stream.Pour(Vector3.forward, HalfRange, TestSettings.Frame, out _);
            }

            Assert.AreEqual(55f, _ball.RadiusPercent, Tolerance);
            Assert.AreEqual(0.5f, _ball.Health, Tolerance);
        }

        [Test]
        public void Pour_DrainsDownToTheCriticalSizeOfEveryMode_WhereTheHealthIsZero()
        {
            _stream.Pour(Vector3.forward, HalfRange, 1000f, out _);

            Assert.AreEqual(TestSettings.MinShootPercent, _ball.RadiusPercent, Tolerance);
            Assert.AreEqual(0f, _ball.Health, Tolerance);
            Assert.IsFalse(_ball.HasRemainder);
        }

        [Test]
        public void Pour_WaterRunsOut_BlockedPathLoses_ClearPathWins()
        {
            Assert.AreEqual(RoundOutcome.Continue, _rules.Evaluate(false, _ball.RadiusPercent));

            _stream.Pour(Vector3.forward, HalfRange, 1000f, out _);

            Assert.AreEqual(RoundOutcome.Lose, _rules.Evaluate(false, _ball.RadiusPercent));
            Assert.AreEqual(RoundOutcome.Win, _rules.Evaluate(true, _ball.RadiusPercent));
        }

        [Test]
        public void Pour_NearlyEmptyBall_StillPoursTheSmallestDrop()
        {
            _ball.Spend(85f);

            _stream.Pour(Vector3.forward, HalfRange, TestSettings.Frame, out float dropRadius);

            Assert.AreEqual(1, _stream.InFlight.Length);
            Assert.AreEqual(0.06f, dropRadius, Tolerance);
        }

        [Test]
        public void Pour_LaunchesTheDropAlongTheAimAtOnce()
        {
            ArcPath arc = _stream.Pour(Vector3.right, HalfRange, TestSettings.Frame, out float dropRadius);

            Assert.AreEqual(1, _stream.InFlight.Length);
            Assert.Greater(arc.End.x, arc.Start.x);
            Assert.AreEqual(0f, arc.End.z, Tolerance);
            Assert.AreEqual(0.12f, dropRadius, Tolerance);
        }

        [Test]
        public void Stop_LeavesTheDropsInTheAir()
        {
            _stream.Pour(Vector3.forward, HalfRange, TestSettings.Frame, out _);

            _stream.Stop();

            Assert.IsFalse(_stream.IsFlowing);
            Assert.IsFalse(_stream.IsEmpty);
        }

        [Test]
        public void Fly_DropsLandOnTheEmptyField()
        {
            var impacts = new List<DropImpact>();
            _stream.Pour(Vector3.forward, HalfRange, TestSettings.Frame, out _);
            _stream.Stop();

            _stream.Fly(10f, impacts);

            Assert.AreEqual(1, impacts.Count);
            Assert.IsTrue(impacts[0].IsOnGround);
            Assert.IsTrue(_stream.IsEmpty);
        }

        private sealed class EmptyField : IObstacleSweep
        {
            public bool TrySweep(Vector3 from, Vector3 to, float radius, out Vector3 contactCenter, out Collider obstacle)
            {
                contactCenter = to;
                obstacle = null;
                return false;
            }
        }
    }
}
