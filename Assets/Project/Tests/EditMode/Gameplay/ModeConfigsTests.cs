using System;
using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Configs.Modes;
using SelfBulletting.Gameplay.Modes;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class ModeConfigsTests
    {
        private const float Tolerance = 0.0001f;
        private const string ModeConfigsFolder = "Assets/Project/Configs/Modes";

        private readonly List<ScriptableObject> _created = new List<ScriptableObject>();

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
        public void Get_ReturnsTheConfigOfEveryMode()
        {
            var configs = new ModeConfigs(CreateAll());

            foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
            {
                Assert.AreEqual(mode, configs.Get(mode).Mode);
            }
        }

        [Test]
        public void Constructor_ModeWithoutConfig_Throws()
        {
            ModeConfig[] withoutHose = { Create<ClassicModeConfig>(), Create<SlingshotModeConfig>(), Create<ArrowModeConfig>() };

            Assert.Throws<ArgumentException>(() => new ModeConfigs(withoutHose));
        }

        [Test]
        public void Constructor_TwoConfigsForOneMode_Throws()
        {
            var configs = new List<ModeConfig>(CreateAll()) { Create<HoseModeConfig>() };

            Assert.Throws<ArgumentException>(() => new ModeConfigs(configs));
        }

        [Test]
        public void ShootingModes_CriticalSizeIsTheMinimumShot_AndTheyHaveCombo()
        {
            var classic = Create<ClassicModeConfig>();

            Assert.AreEqual(classic.Shot.MinShootPercent, classic.CriticalPercent, Tolerance);
            Assert.IsTrue(classic.Combo.HasTiers);
        }

        [Test]
        public void Hose_LosesAtTheCriticalSizeOfTheShootingModes_AndItHasNoCombo()
        {
            var hose = Create<HoseModeConfig>();

            Assert.AreEqual(hose.Hose.CriticalPercent, hose.CriticalPercent, Tolerance);
            Assert.AreEqual(Create<ClassicModeConfig>().CriticalPercent, hose.CriticalPercent, Tolerance);
            Assert.IsFalse(hose.Combo.HasTiers);
        }

        [Test]
        public void Constructor_ModesWithDifferentCriticalSizes_Throws()
        {
            var hose = Create<HoseModeConfig>();
            var serialized = new SerializedObject(hose);
            serialized.FindProperty("_hose._criticalPercent").floatValue = 60f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            ModeConfig[] configs = { Create<ClassicModeConfig>(), Create<SlingshotModeConfig>(), Create<ArrowModeConfig>(), hose };

            Assert.Throws<ArgumentException>(() => new ModeConfigs(configs));
        }

        [Test]
        public void GameAssets_EveryModeLosesAtTheSameCriticalSize()
        {
            var configs = new List<ModeConfig>();
            foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(ModeConfig)}", new[] { ModeConfigsFolder }))
            {
                configs.Add(AssetDatabase.LoadAssetAtPath<ModeConfig>(AssetDatabase.GUIDToAssetPath(guid)));
            }

            Assert.DoesNotThrow(() => new ModeConfigs(configs));
        }

        private ModeConfig[] CreateAll() => new ModeConfig[]
        {
            Create<ClassicModeConfig>(), Create<SlingshotModeConfig>(), Create<ArrowModeConfig>(), Create<HoseModeConfig>()
        };

        private T Create<T>() where T : ModeConfig
        {
            var config = ScriptableObject.CreateInstance<T>();
            _created.Add(config);
            return config;
        }
    }
}
