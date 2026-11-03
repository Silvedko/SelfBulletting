using NUnit.Framework;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Score;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class ScoreCounterTests
    {
        private const float Tolerance = 0.0001f;

        private ScoreCounter _counter;

        [SetUp]
        public void SetUp()
        {
            var tiers = new[] { new ComboTier(3, 1.5f), new ComboTier(6, 2f), new ComboTier(10, 3f) };
            _counter = new ScoreCounter(new ScoreSettings(10, tiers, pointsPerRemainingPercent: 30));
        }

        [Test]
        public void AddShot_BelowFirstTier_HasNoCombo()
        {
            ShotScore score = _counter.AddShot(2);

            Assert.AreEqual(20, score.BasePoints);
            Assert.AreEqual(0, score.ComboBonus);
            Assert.IsFalse(score.IsCombo);
        }

        [Test]
        public void AddShot_FromThreeObstacles_AppliesFirstTier()
        {
            ShotScore score = _counter.AddShot(3);

            Assert.AreEqual(30, score.BasePoints);
            Assert.AreEqual(15, score.ComboBonus);
            Assert.AreEqual(1.5f, score.Multiplier, Tolerance);
        }

        [Test]
        public void AddShot_AboveAllTiers_AppliesHighestTier()
        {
            ShotScore score = _counter.AddShot(12);

            Assert.AreEqual(120, score.BasePoints);
            Assert.AreEqual(240, score.ComboBonus);
        }

        [Test]
        public void Summary_AccumulatesAllShots()
        {
            _counter.AddShot(2);
            _counter.AddShot(3);

            ScoreSummary summary = _counter.Summary;
            Assert.AreEqual(5, summary.DestroyedObstacles);
            Assert.AreEqual(50, summary.BasePoints);
            Assert.AreEqual(15, summary.ComboBonus);
            Assert.AreEqual(65, summary.Total);
        }

        [Test]
        public void GetMultiplier_UnsortedTiers_StillPicksHighestReachedTier()
        {
            var tiers = new[] { new ComboTier(10, 3f), new ComboTier(3, 1.5f), new ComboTier(6, 2f) };
            var counter = new ScoreCounter(new ScoreSettings(10, tiers, pointsPerRemainingPercent: 30));

            Assert.AreEqual(2f, counter.GetMultiplier(7), Tolerance);
            Assert.AreEqual(2, counter.GetComboLevel(7));
        }

        [TestCase(2, 0)]
        [TestCase(3, 1)]
        [TestCase(6, 2)]
        [TestCase(25, 3)]
        public void AddShot_ReportsReachedComboLevel(int destroyedObstacles, int expectedLevel)
        {
            Assert.AreEqual(expectedLevel, _counter.AddShot(destroyedObstacles).ComboLevel);
        }

        [Test]
        public void AwardHealthBonus_PaysForEveryRemainingPercentAndAddsToTotal()
        {
            _counter.AddShot(3);

            int bonus = _counter.AwardHealthBonus(25f);

            Assert.AreEqual(750, bonus);
            Assert.AreEqual(750, _counter.Summary.HealthBonus);
            Assert.AreEqual(30 + 15 + 750, _counter.Summary.Total);
        }

        [Test]
        public void AwardHealthBonus_CalledAgain_ReplacesBonus()
        {
            _counter.AwardHealthBonus(25f);
            _counter.AwardHealthBonus(0f);

            Assert.AreEqual(0, _counter.Summary.HealthBonus);
        }
    }
}
