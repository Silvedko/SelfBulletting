using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Aim;
using SelfBulletting.Gameplay.Hose;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Infrastructure.Services.Input;
using UnityEngine;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// The screen is 1000 px high and maps 100 px to a meter of ground, screen up is world +Z.
    /// The dead zone is 5% (50 px), the full pull 40% (400 px).
    /// </summary>
    public sealed class HoseAimTests
    {
        private const float Tolerance = 0.0001f;
        private static readonly Vector2 _pressPoint = new Vector2(500f, 500f);

        private FakeInput _input;
        private HoseAim _aim;

        [SetUp]
        public void SetUp()
        {
            _input = new FakeInput();
            _aim = new HoseAim(_input, new FlatScreen(), new SlingshotPull(new SlingshotSettings(0.05f, 0.4f)));
        }

        [Test]
        public void TryAim_FingerUp_NoWater()
        {
            Assert.IsFalse(_aim.TryAim(out _, out _));
        }

        [Test]
        public void TryAim_FingerDownBeforeTheTurn_NoWater()
        {
            _input.IsPressed = true;
            _input.Position = _pressPoint + Vector2.down * 300f;

            Assert.IsFalse(_aim.TryAim(out _, out _));
        }

        [Test]
        public void TryAim_InsideTheDeadZone_NoWater()
        {
            Press();
            _input.Position = _pressPoint + Vector2.down * 20f;

            Assert.IsFalse(_aim.TryAim(out _, out _));
        }

        [Test]
        public void TryAim_PulledBack_WaterFliesTheOppositeWay()
        {
            Press();
            _input.Position = _pressPoint + Vector2.down * 225f;

            Assert.IsTrue(_aim.TryAim(out Vector3 direction, out float rangeShare));
            Assert.AreEqual(0.5f, rangeShare, Tolerance);
            Assert.AreEqual(1f, direction.z, Tolerance);
        }

        [Test]
        public void TryAim_FingerLifted_StopsTheWater()
        {
            Press();
            _input.Position = _pressPoint + Vector2.down * 225f;
            _aim.TryAim(out _, out _);

            _input.IsPressed = false;

            Assert.IsFalse(_aim.TryAim(out _, out _));
        }

        private void Press()
        {
            _input.IsPressed = true;
            _input.WasPressedThisFrame = true;
            _input.Position = _pressPoint;
            _aim.TryAim(out _, out _);
            _input.WasPressedThisFrame = false;
        }

        private sealed class FakeInput : IInputService
        {
            public bool IsPressed { get; set; }
            public bool WasPressedThisFrame { get; set; }
            public bool WasReleasedThisFrame { get; set; }
            public Vector2 Position { get; set; }
        }

        private sealed class FlatScreen : IScreenToGround
        {
            private const float PixelsPerMeter = 100f;

            public float ScreenHeight => 1000f;

            public bool TryGetGroundPoint(Vector2 screenPoint, out Vector3 groundPoint)
            {
                groundPoint = new Vector3(screenPoint.x, 0f, screenPoint.y) / PixelsPerMeter;
                return true;
            }
        }
    }
}
