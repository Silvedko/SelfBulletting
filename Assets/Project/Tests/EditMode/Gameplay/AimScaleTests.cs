using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Aim;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class AimScaleTests
    {
        private const float Tolerance = 0.0001f;

        private AimScale _scale;

        [SetUp]
        public void SetUp()
        {
            _scale = new AimScale(new AimSettings(3f));
        }

        [TestCase(0f, 0f)]
        [TestCase(1.5f, 0.5f)]
        [TestCase(3f, 1f)]
        [TestCase(4.5f, 0.5f)]
        [TestCase(6f, 0f)]
        public void Evaluate_FullScale_GoesUpInThreeSecondsAndBack(float elapsedSeconds, float expected)
        {
            Assert.AreEqual(expected, _scale.Evaluate(elapsedSeconds, 1f), Tolerance);
        }

        [TestCase(0.75f, 0.25f)]
        [TestCase(1.5f, 0.5f)]
        [TestCase(3f, 0f)]
        public void Evaluate_CutScale_KeepsSpeedAndBouncesAtLimit(float elapsedSeconds, float expected)
        {
            Assert.AreEqual(expected, _scale.Evaluate(elapsedSeconds, 0.5f), Tolerance);
        }

        [Test]
        public void Evaluate_ZeroLimit_StaysAtZero()
        {
            Assert.AreEqual(0f, _scale.Evaluate(2f, 0f), Tolerance);
        }
    }
}
