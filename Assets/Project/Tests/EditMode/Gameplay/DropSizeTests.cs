using NUnit.Framework;
using SelfBulletting.Gameplay.Hose;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class DropSizeTests
    {
        private const float Tolerance = 0.0001f;

        private DropSize _size;

        [SetUp]
        public void SetUp()
        {
            _size = new DropSize(TestSettings.Hose(), TestSettings.Limits());
        }

        [Test]
        public void GetRadius_FullBall_IsDropRadiusPercentOfR0()
        {
            Assert.AreEqual(0.12f, _size.GetRadius(100f), Tolerance);
        }

        [Test]
        public void GetRadius_ShrinksWithTheBall()
        {
            Assert.AreEqual(0.09f, _size.GetRadius(75f), Tolerance);
        }

        // 8% at the full ball reaches the smallest drop, 4%, at half the ball and stays there down to the critical size.
        [TestCase(50f)]
        [TestCase(30f)]
        [TestCase(TestSettings.MinShootPercent)]
        public void GetRadius_NeverBelowTheSmallestDrop(float ballRadiusPercent)
        {
            Assert.AreEqual(0.06f, _size.GetRadius(ballRadiusPercent), Tolerance);
        }

        [Test]
        public void TouchAndSplash_AreAFewDropRadii()
        {
            Assert.AreEqual(0.18f, _size.GetTouchRadius(0.12f), Tolerance);
            Assert.AreEqual(0.3f, _size.GetSplashRadius(0.12f), Tolerance);
        }
    }
}
