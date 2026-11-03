using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Feedback;
using SelfBulletting.Infrastructure.Services.Audio;
using SelfBulletting.Infrastructure.Services.Vibration;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class DangerFeedbackTests
    {
        private const float Tolerance = 0.0001f;
        private const float Frame = 0.02f;

        private CountingVibrationService _vibration;
        private DangerFeedback _feedback;

        [SetUp]
        public void SetUp()
        {
            _vibration = new CountingVibrationService();
            var settings = new FeedbackSettings(warningPercent: 15f, vibrationPulseMs: 40, vibrationIntervalMaxMs: 400, vibrationIntervalMinMs: 120);
            _feedback = new DangerFeedback(settings, TestSettings.Shot(), _vibration, new NullAudioService());
        }

        [TestCase(20f, 0f)]
        [TestCase(15f, 0f)]
        [TestCase(12.5f, 0.5f)]
        [TestCase(10f, 1f)]
        public void GetDangerLevel_GrowsFromWarningToCriticalSize(float previewPercent, float expected)
        {
            Assert.AreEqual(expected, _feedback.GetDangerLevel(previewPercent), Tolerance);
        }

        [Test]
        public void Tick_EnteringWarningZone_PulsesAtOnce()
        {
            _feedback.Tick(12.5f, Frame);

            Assert.AreEqual(1, _vibration.Pulses);
        }

        [Test]
        public void Tick_InsideWarningZone_PulsesAfterInterval()
        {
            _feedback.Tick(12.5f, Frame);

            // Halfway between warning and critical the interval is 260 ms.
            _feedback.Tick(12.5f, 0.25f);
            Assert.AreEqual(1, _vibration.Pulses);

            _feedback.Tick(12.5f, Frame);
            Assert.AreEqual(2, _vibration.Pulses);
        }

        [Test]
        public void Tick_LeavingAndReenteringWarningZone_PulsesAtOnceAgain()
        {
            _feedback.Tick(12.5f, Frame);
            _feedback.Tick(30f, Frame);
            _feedback.Tick(12.5f, Frame);

            Assert.AreEqual(2, _vibration.Pulses);
        }

        private sealed class CountingVibrationService : IVibrationService
        {
            public int Pulses { get; private set; }

            public void Pulse(int milliseconds)
            {
                Pulses++;
            }
        }
    }
}
