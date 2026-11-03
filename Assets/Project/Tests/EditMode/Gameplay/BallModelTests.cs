using System;
using NUnit.Framework;
using SelfBulletting.Gameplay.Ball;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class BallModelTests
    {
        private const float Tolerance = 0.0001f;

        private BallModel _ball;

        [SetUp]
        public void SetUp()
        {
            _ball = new BallModel(TestSettings.Economy());
        }

        [Test]
        public void NewBall_HasFullSize()
        {
            Assert.AreEqual(100f, _ball.RadiusPercent, Tolerance);
            Assert.AreEqual(TestSettings.StartRadius, _ball.WorldRadius, Tolerance);
        }

        [Test]
        public void Spend_ShrinksBallAndRaisesEvent()
        {
            int raised = 0;
            _ball.RadiusChanged += () => raised++;

            _ball.Spend(25f);

            Assert.AreEqual(75f, _ball.RadiusPercent, Tolerance);
            Assert.AreEqual(1, raised);
        }

        [Test]
        public void Spend_NeverGoesBelowCriticalSize()
        {
            _ball.Spend(500f);

            Assert.AreEqual(TestSettings.MinShootPercent, _ball.RadiusPercent, Tolerance);
        }

        [Test]
        public void Spend_NegativeCost_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _ball.Spend(-1f));
        }
    }
}
