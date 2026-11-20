using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Configs.Modes;
using SelfBulletting.Gameplay.Hose;
using SelfBulletting.Gameplay.Modes;
using SelfBulletting.Infrastructure.Services.Bonus;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SelfBulletting.Tests.Gameplay
{
    /// <summary>
    /// The Hose bonus for fires put out together, with the default configs: a 300 ms window and the tiers
    /// 2 fires ×1.5, 3 fires ×2, 5 fires ×3.
    /// </summary>
    public sealed class ExtinguishBonusTests
    {
        private const float Tolerance = 0.0001f;
        private const float Window = 0.3f;
        private const float InsideWindow = 0.1f;

        private readonly List<ScriptableObject> _created = new List<ScriptableObject>();

        private BonusService _bonus;

        [SetUp]
        public void SetUp()
        {
            var configs = new ModeConfig[]
            {
                Create<ClassicModeConfig>(), Create<SlingshotModeConfig>(), Create<ArrowModeConfig>(), Create<HoseModeConfig>()
            };
            _bonus = new BonusService(new ModeConfigs(configs));
        }

        [TearDown]
        public void TearDown()
        {
            foreach (ScriptableObject created in _created)
            {
                Object.DestroyImmediate(created);
            }

            _created.Clear();
        }

        [Test]
        public void Service_OnlyTheHoseHasTheBonus()
        {
            Assert.IsTrue(_bonus.HasExtinguishBonus(GameMode.Hose));
            Assert.IsFalse(_bonus.HasExtinguishBonus(GameMode.Classic));
            Assert.IsFalse(_bonus.HasExtinguishBonus(GameMode.Slingshot));
            Assert.IsFalse(_bonus.HasExtinguishBonus(GameMode.Arrow));
        }

        [Test]
        public void Service_ReadsTheWindowOfTheMode()
        {
            Assert.AreEqual(Window, _bonus.GetExtinguishWindowSeconds(GameMode.Hose), Tolerance);
        }

        [TestCase(1, 1f, 0)]
        [TestCase(2, 1.5f, 1)]
        [TestCase(4, 2f, 2)]
        [TestCase(7, 3f, 3)]
        public void Service_AppliesTheHighestTierReached(int fires, float multiplier, int level)
        {
            BonusTier tier = _bonus.GetExtinguishBonus(GameMode.Hose, fires);

            Assert.AreEqual(multiplier, tier.Multiplier, Tolerance);
            Assert.AreEqual(level, tier.Level);
        }

        [Test]
        public void Service_ModeWithoutTheBonus_ReachesNoTier()
        {
            Assert.IsFalse(_bonus.GetExtinguishBonus(GameMode.Classic, 10).IsReached);
        }

        [Test]
        public void Burst_TwoFiresInsideTheWindow_PaysOnceTheWindowHasPassed()
        {
            var burst = new ExtinguishBurst(_bonus, GameMode.Hose);
            burst.Add(0f, new Vector3(0f, 0f, 0f));
            burst.Add(InsideWindow, new Vector3(2f, 0f, 4f));

            Assert.IsFalse(burst.TryClose(InsideWindow, out _));
            Assert.IsTrue(burst.TryClose(Window, out ExtinguishBurstResult result));
            Assert.AreEqual(2, result.Fires);
            Assert.AreEqual(1.5f, result.Tier.Multiplier, Tolerance);
            Assert.AreEqual(new Vector3(1f, 0f, 2f), result.Center);
        }

        [Test]
        public void Burst_PaysOnlyOnce()
        {
            var burst = new ExtinguishBurst(_bonus, GameMode.Hose);
            burst.Add(0f, Vector3.zero);
            burst.Add(0f, Vector3.zero);

            Assert.IsTrue(burst.TryClose(Window, out _));
            Assert.IsFalse(burst.TryClose(Window * 2f, out _));
            Assert.IsFalse(burst.TryCloseNow(out _));
        }

        [Test]
        public void Burst_SingleFire_PaysNothing()
        {
            var burst = new ExtinguishBurst(_bonus, GameMode.Hose);
            burst.Add(0f, Vector3.zero);

            Assert.IsFalse(burst.TryClose(Window, out _));
        }

        [Test]
        public void Burst_FireAfterTheWindow_StartsANewBurst()
        {
            var burst = new ExtinguishBurst(_bonus, GameMode.Hose);
            burst.Add(0f, Vector3.zero);
            Assert.IsFalse(burst.TryClose(Window, out _));

            burst.Add(Window, Vector3.zero);
            burst.Add(Window + InsideWindow, Vector3.zero);

            Assert.IsTrue(burst.TryClose(Window * 2f, out ExtinguishBurstResult result));
            Assert.AreEqual(2, result.Fires);
        }

        [Test]
        public void Burst_TurnOver_ClosesInsideTheWindow()
        {
            var burst = new ExtinguishBurst(_bonus, GameMode.Hose);
            burst.Add(0f, Vector3.zero);
            burst.Add(0f, Vector3.zero);
            burst.Add(0f, Vector3.zero);

            Assert.IsTrue(burst.TryCloseNow(out ExtinguishBurstResult result));
            Assert.AreEqual(3, result.Fires);
            Assert.AreEqual(2, result.Tier.Level);
        }

        [Test]
        public void Burst_ModeWithoutTheBonus_NeverPays()
        {
            var burst = new ExtinguishBurst(_bonus, GameMode.Classic);
            burst.Add(0f, Vector3.zero);
            burst.Add(0f, Vector3.zero);

            Assert.IsFalse(burst.TryClose(Window, out _));
            Assert.IsFalse(burst.TryCloseNow(out _));
        }

        private T Create<T>() where T : ModeConfig
        {
            var config = ScriptableObject.CreateInstance<T>();
            _created.Add(config);
            return config;
        }
    }
}
