using NUnit.Framework;
using SelfBulletting.Gameplay.Shot;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class ShotEconomyTests
    {
        private const float Tolerance = 0.0001f;

        private ShotEconomy _economy;

        [SetUp]
        public void SetUp()
        {
            _economy = TestSettings.Economy();
        }

        [TestCase(0f, 10f)]
        [TestCase(0.5f, 25f)]
        [TestCase(1f, 40f)]
        public void GetCostPercent_InterpolatesBetweenMinAndMaxShot(float scale, float expectedCost)
        {
            Assert.AreEqual(expectedCost, _economy.GetCostPercent(scale), Tolerance);
        }

        [TestCase(0f, 0.45f)]
        [TestCase(1f, 3f)]
        public void GetDamageWorldRadius_ConvertsPercentOfStartRadiusToMeters(float scale, float expectedRadius)
        {
            Assert.AreEqual(expectedRadius, _economy.GetDamageWorldRadius(scale), Tolerance);
        }

        [TestCase(100f, 1f)]
        [TestCase(50f, 1f)]
        [TestCase(35f, 0.5f)]
        [TestCase(20f, 0f)]
        [TestCase(15f, 0f)]
        public void GetScaleLimit_IsCutToWhatTheBallCanAfford(float radiusPercent, float expectedLimit)
        {
            Assert.AreEqual(expectedLimit, _economy.GetScaleLimit(radiusPercent), Tolerance);
        }

        [TestCase(10f, ShotMode.None)]
        [TestCase(15f, ShotMode.Instant)]
        [TestCase(20f, ShotMode.Instant)]
        [TestCase(21f, ShotMode.Aimed)]
        [TestCase(100f, ShotMode.Aimed)]
        public void GetShotMode_DependsOnRemainderAboveCriticalSize(float radiusPercent, ShotMode expectedMode)
        {
            Assert.AreEqual(expectedMode, _economy.GetShotMode(radiusPercent));
        }

        [Test]
        public void CreateAimedShot_AtCutLimit_SpendsEverythingAboveCriticalSize()
        {
            ShotRequest shot = _economy.CreateAimedShot(35f, 0.5f);

            Assert.AreEqual(25f, shot.CostPercent, Tolerance);
            Assert.AreEqual(1.725f, shot.DamageWorldRadius, Tolerance);
        }

        [Test]
        public void CreateAimedShot_AboveLimit_IsClampedToLimit()
        {
            ShotRequest shot = _economy.CreateAimedShot(35f, 1f);

            Assert.AreEqual(0.5f, shot.Scale, Tolerance);
            Assert.AreEqual(25f, shot.CostPercent, Tolerance);
        }

        [Test]
        public void CreateInstantShot_SpendsRemainderWithMinimumDamage()
        {
            ShotRequest shot = _economy.CreateInstantShot(17f);

            Assert.AreEqual(7f, shot.CostPercent, Tolerance);
            Assert.AreEqual(0.45f, shot.DamageWorldRadius, Tolerance);
        }

        // The minimum shot (10%) is a 20% ball; every percent the player ball loses above that adds a percent to the shot.
        [TestCase(10f, 0.3f)]
        [TestCase(25f, 0.525f)]
        [TestCase(40f, 0.75f)]
        [TestCase(3f, 0.195f)]
        public void GetShotWorldRadius_StartsAtStartRadiusAndGrowsByHowMuchTheBallShrinks(float costPercent, float expectedRadius)
        {
            Assert.AreEqual(expectedRadius, _economy.GetShotWorldRadius(costPercent), Tolerance);
        }

        [TestCase(100f, 1f)]
        [TestCase(55f, 0.5f)]
        [TestCase(10f, 0f)]
        public void GetHealth_GoesFromFullSizeToCriticalSize(float radiusPercent, float expectedHealth)
        {
            Assert.AreEqual(expectedHealth, _economy.GetHealth(radiusPercent), Tolerance);
        }

        [Test]
        public void GetPreviewRadiusPercent_NeverGoesBelowCriticalSize()
        {
            Assert.AreEqual(10f, _economy.GetPreviewRadiusPercent(35f, 1f), Tolerance);
            Assert.AreEqual(75f, _economy.GetPreviewRadiusPercent(100f, 0.5f), Tolerance);
        }
    }
}
