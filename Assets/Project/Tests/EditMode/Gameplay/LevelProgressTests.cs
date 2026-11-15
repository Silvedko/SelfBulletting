using System;
using System.Collections.Generic;
using NUnit.Framework;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Modes;
using SelfBulletting.Infrastructure.Services.Save;

namespace SelfBulletting.Tests.Gameplay
{
    public sealed class LevelProgressTests
    {
        private MemorySaveService _save;

        [SetUp]
        public void SetUp()
        {
            _save = new MemorySaveService();
        }

        [Test]
        public void NewPlayer_StartsAtFirstLevel()
        {
            var progress = new LevelProgress(_save, GameMode.Classic, 3);

            Assert.AreEqual(0, progress.CurrentIndex);
            Assert.AreEqual(1, progress.CurrentNumber);
        }

        [Test]
        public void CompleteCurrent_MovesToNextLevel()
        {
            var progress = new LevelProgress(_save, GameMode.Classic, 3);

            progress.CompleteCurrent();

            Assert.AreEqual(1, progress.CurrentIndex);
            Assert.AreEqual(1, _save.GetLevelIndex(GameMode.Classic));
        }

        [Test]
        public void CompleteCurrent_AfterLastLevel_StartsOverFromFirst()
        {
            _save.SetLevelIndex(GameMode.Classic, 2);
            var progress = new LevelProgress(_save, GameMode.Classic, 3);

            progress.CompleteCurrent();

            Assert.AreEqual(0, progress.CurrentIndex);
        }

        [TestCase(5, 2)]
        [TestCase(-1, 2)]
        public void SavedIndexOutsideCatalog_WrapsIntoIt(int savedIndex, int expectedIndex)
        {
            _save.SetLevelIndex(GameMode.Classic, savedIndex);

            Assert.AreEqual(expectedIndex, new LevelProgress(_save, GameMode.Classic, 3).CurrentIndex);
        }

        [Test]
        public void CompleteCurrent_MovesOnlyItsOwnMode()
        {
            var slingshot = new LevelProgress(_save, GameMode.Slingshot, 3);
            var arrow = new LevelProgress(_save, GameMode.Arrow, 3);

            slingshot.CompleteCurrent();
            slingshot.CompleteCurrent();

            Assert.AreEqual(2, slingshot.CurrentIndex);
            Assert.AreEqual(0, arrow.CurrentIndex);
            Assert.AreEqual(0, _save.GetLevelIndex(GameMode.Classic));
        }

        [Test]
        public void SingleLevelCatalog_StaysOnFirstLevelAfterWins()
        {
            var classic = new LevelProgress(_save, GameMode.Classic, 1);

            classic.CompleteCurrent();
            classic.CompleteCurrent();

            Assert.AreEqual(0, classic.CurrentIndex);
        }

        [Test]
        public void EmptyCatalog_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LevelProgress(_save, GameMode.Classic, 0));
        }

        private sealed class MemorySaveService : ISaveService
        {
            private readonly Dictionary<GameMode, int> _scores = new Dictionary<GameMode, int>();
            private readonly Dictionary<GameMode, int> _levels = new Dictionary<GameMode, int>();

            public int GetTotalScore(GameMode mode) => _scores.TryGetValue(mode, out int score) ? score : 0;

            public int GetLevelIndex(GameMode mode) => _levels.TryGetValue(mode, out int index) ? index : 0;

            public void AddToTotalScore(GameMode mode, int points)
            {
                _scores[mode] = GetTotalScore(mode) + points;
            }

            public void SetLevelIndex(GameMode mode, int index)
            {
                _levels[mode] = index;
            }
        }
    }
}
