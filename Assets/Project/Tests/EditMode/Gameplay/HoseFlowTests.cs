using NUnit.Framework;
using SelfBulletting.Gameplay.Hose;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class HoseFlowTests
    {
        private const float Tolerance = 0.0001f;
        private const float PlentyLeft = 100f;
        private const int FramesPerSecond = 16;

        private HoseFlow _flow;

        [SetUp]
        public void SetUp()
        {
            _flow = new HoseFlow(TestSettings.Hose());
        }

        [Test]
        public void Tick_WhileFlowing_DrainsDrainPercentPerSecond()
        {
            _flow.Start();

            float spent = 0f;
            for (int i = 0; i < FramesPerSecond; i++)
            {
                _flow.Tick(TestSettings.Frame, PlentyLeft, out float cost);
                spent += cost;
            }

            Assert.AreEqual(TestSettings.DrainPercentPerSecond, spent, Tolerance);
        }

        [Test]
        public void Tick_NeverDrainsMoreThanTheRemainder()
        {
            _flow.Start();

            _flow.Tick(1f, 0.5f, out float cost);

            Assert.AreEqual(0.5f, cost, Tolerance);
        }

        [Test]
        public void Tick_Stopped_DrainsNothingAndDropsNothing()
        {
            int drops = _flow.Tick(1f, PlentyLeft, out float cost);

            Assert.AreEqual(0, drops);
            Assert.AreEqual(0f, cost);
        }

        [Test]
        public void Start_FirstDropLeavesAtOnce()
        {
            _flow.Start();

            Assert.AreEqual(1, _flow.Tick(TestSettings.Frame, PlentyLeft, out _));
        }

        [Test]
        public void Tick_AfterTheFirstDrop_KeepsDropsPerSecondPace()
        {
            _flow.Start();

            int drops = 0;
            for (int i = 0; i < FramesPerSecond; i++)
            {
                drops += _flow.Tick(TestSettings.Frame, PlentyLeft, out _);
            }

            Assert.AreEqual(1 + (int)TestSettings.DropsPerSecond, drops);
        }

        [Test]
        public void Start_WhileFlowing_KeepsThePace()
        {
            _flow.Start();
            _flow.Tick(TestSettings.Frame, PlentyLeft, out _);

            _flow.Start();

            Assert.AreEqual(0, _flow.Tick(TestSettings.Frame / 2f, PlentyLeft, out _));
        }

        [Test]
        public void StopThenStart_FirstDropLeavesAtOnceAgain()
        {
            _flow.Start();
            _flow.Tick(TestSettings.Frame, PlentyLeft, out _);
            _flow.Stop();

            _flow.Start();

            Assert.AreEqual(1, _flow.Tick(TestSettings.Frame / 2f, PlentyLeft, out _));
        }
    }
}
