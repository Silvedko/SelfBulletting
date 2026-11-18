using NUnit.Framework;
using SelfBulletting.Gameplay.Ball;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class BallLimitsTests
    {
        private const float Tolerance = 0.0001f;

        [TestCase(100f, 1f)]
        [TestCase(55f, 0.5f)]
        [TestCase(10f, 0f)]
        public void GetHealth_GoesFromFullSizeToCriticalSize(float radiusPercent, float expectedHealth)
        {
            Assert.AreEqual(expectedHealth, TestSettings.Limits().GetHealth(radiusPercent), Tolerance);
        }

        [TestCase(35f, 25f)]
        [TestCase(10f, 0f)]
        [TestCase(5f, 0f)]
        public void GetRemainder_IsWhatIsLeftAboveCriticalSize(float radiusPercent, float expectedRemainder)
        {
            Assert.AreEqual(expectedRemainder, TestSettings.Limits().GetRemainder(radiusPercent), Tolerance);
        }

        [Test]
        public void HasRemainder_FalseOnlyAtCriticalSize()
        {
            BallLimits limits = TestSettings.Limits();

            Assert.IsFalse(limits.HasRemainder(10f));
            Assert.IsTrue(limits.HasRemainder(10.5f));
        }
    }
}
