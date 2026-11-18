using NUnit.Framework;
using SelfBulletting.Gameplay.Hose;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class HitCounterTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void AddHit_OneHitToExtinguish_PutsTheFireOutAtOnce()
        {
            var hits = new HitCounter<string>(1);

            Assert.AreEqual(1f, hits.AddHit("fire"), Tolerance);
        }

        [Test]
        public void AddHit_SeveralHitsNeeded_RaisesTheShareStepByStep()
        {
            var hits = new HitCounter<string>(3);

            Assert.AreEqual(1f / 3f, hits.AddHit("fire"), Tolerance);
            Assert.AreEqual(2f / 3f, hits.AddHit("fire"), Tolerance);
            Assert.AreEqual(1f, hits.AddHit("fire"), Tolerance);
        }

        [Test]
        public void AddHit_HitsOnOneFireNeverCountForAnother()
        {
            var hits = new HitCounter<string>(2);

            hits.AddHit("near");

            Assert.AreEqual(0.5f, hits.AddHit("far"), Tolerance);
        }

        [Test]
        public void AddHit_FireAlreadyOut_StaysOut()
        {
            var hits = new HitCounter<string>(1);
            hits.AddHit("fire");

            Assert.AreEqual(1f, hits.AddHit("fire"), Tolerance);
        }
    }
}
