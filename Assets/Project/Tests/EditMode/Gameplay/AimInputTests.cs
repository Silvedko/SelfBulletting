using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Aim;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// The slingshot pull and the two sweeps of the arrow. The screen is 1000 px high: the dead zone is 50 px, the full pull
    /// 400 px. The arrow swings 0°–90° in 1.2 s, then the landing point runs the range in 1.5 s.
    /// </summary>
    public sealed class AimInputTests
    {
        private const float Tolerance = 0.001f;
        private const float ScreenHeight = 1000f;
        private static readonly Vector2 Press = new Vector2(500f, 500f);

        private SlingshotPull _pull;
        private ArrowSwing _swing;

        [SetUp]
        public void SetUp()
        {
            _pull = new SlingshotPull(new SlingshotSettings(0.05f, 0.4f));
            _swing = new ArrowSwing(new ArrowSettings(45f, 90f, 1.2f, 1.5f));
        }

        [TestCase(0f)]
        [TestCase(49f)]
        public void Pull_InsideDeadZone_IsNoAim(float pullPixels)
        {
            Assert.IsFalse(_pull.TryGetRangeShare(Press, Press + Vector2.down * pullPixels, ScreenHeight, out _));
        }

        [TestCase(50f, 0f)]
        [TestCase(225f, 0.5f)]
        [TestCase(400f, 1f)]
        [TestCase(700f, 1f)]
        public void Pull_PastDeadZone_MapsLengthToRangeShare(float pullPixels, float expectedShare)
        {
            Assert.IsTrue(_pull.TryGetRangeShare(Press, Press + Vector2.down * pullPixels, ScreenHeight, out float share));
            Assert.AreEqual(expectedShare, share, Tolerance);
        }

        [Test]
        public void Pull_ShotFliesOppositeToThePull()
        {
            Vector3 press = new Vector3(5f, 0f, 5f);
            Vector3 pulledBackAndLeft = new Vector3(3f, 0f, 3f);

            Assert.IsTrue(SlingshotPull.TryGetDirection(press, pulledBackAndLeft, out Vector3 direction));
            Assert.AreEqual(new Vector3(1f, 0f, 1f).normalized, direction);
        }

        [Test]
        public void Pull_NoDistanceOnTheGround_HasNoDirection()
        {
            Assert.IsFalse(SlingshotPull.TryGetDirection(Vector3.one, Vector3.one, out _));
        }

        [TestCase(0f, 0f)]
        [TestCase(0.6f, 45f)]
        [TestCase(1.2f, 90f)]
        [TestCase(1.8f, 45f)]
        [TestCase(2.4f, 0f)]
        public void Arrow_SwingsAcrossTheSectorAndBack(float seconds, float expectedYaw)
        {
            Assert.AreEqual(expectedYaw, _swing.GetYawDegrees(seconds), Tolerance);
        }

        [Test]
        public void Arrow_YawZeroLooksUpTheScreenAndNinetyToTheRight()
        {
            Vector3 up = _swing.GetDirection(0f);
            Vector3 right = _swing.GetDirection(1.2f);

            Assert.AreEqual(1f, up.z, Tolerance);
            Assert.AreEqual(1f, right.x, Tolerance);
        }

        [TestCase(0f, 0f)]
        [TestCase(0.75f, 0.5f)]
        [TestCase(1.5f, 1f)]
        [TestCase(2.25f, 0.5f)]
        [TestCase(3f, 0f)]
        public void Arrow_LandingPointRunsAlongTheArrowAndBack(float seconds, float expectedShare)
        {
            Assert.AreEqual(expectedShare, _swing.GetRangeShare(seconds), Tolerance);
        }

        [TestCase(0f, 1f)]
        [TestCase(0.25f, 0.75f)]
        [TestCase(1f, 0f)]
        public void Arrow_ShotShrinksWithRange(float rangeShare, float expectedScale)
        {
            Assert.AreEqual(expectedScale, ArrowSwing.GetSizeScale(rangeShare), Tolerance);
        }

        [Test]
        public void Arrow_NearRangeOnASmallBall_GivesTheBiggestAffordableShot()
        {
            // r = 35% with shots of 10–40%: the scale is cut at 0.5, so the nearest range asks for 1 and gets 0.5.
            var economy = TestSettings.Economy();

            var nearest = economy.CreateAimedShot(35f, ArrowSwing.GetSizeScale(0f));
            var farthest = economy.CreateAimedShot(35f, ArrowSwing.GetSizeScale(1f));

            Assert.AreEqual(0.5f, nearest.Scale, Tolerance);
            Assert.AreEqual(TestSettings.MinShootPercent, farthest.CostPercent, Tolerance);
        }
    }
}
