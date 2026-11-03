using System;
using NUnit.Framework;
using SelfBulletting.Gameplay.Levels;
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
            var progress = new LevelProgress(_save, 3);

            Assert.AreEqual(0, progress.CurrentIndex);
            Assert.AreEqual(1, progress.CurrentNumber);
        }

        [Test]
        public void CompleteCurrent_MovesToNextLevel()
        {
            var progress = new LevelProgress(_save, 3);

            progress.CompleteCurrent();

            Assert.AreEqual(1, progress.CurrentIndex);
            Assert.AreEqual(1, _save.LevelIndex);
        }

        [Test]
        public void CompleteCurrent_AfterLastLevel_StartsOverFromFirst()
        {
            _save.SetLevelIndex(2);
            var progress = new LevelProgress(_save, 3);

            progress.CompleteCurrent();

            Assert.AreEqual(0, progress.CurrentIndex);
        }

        [TestCase(5, 2)]
        [TestCase(-1, 2)]
        public void SavedIndexOutsideCatalog_WrapsIntoIt(int savedIndex, int expectedIndex)
        {
            _save.SetLevelIndex(savedIndex);

            Assert.AreEqual(expectedIndex, new LevelProgress(_save, 3).CurrentIndex);
        }

        [Test]
        public void EmptyCatalog_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LevelProgress(_save, 0));
        }

        private sealed class MemorySaveService : ISaveService
        {
            public int TotalScore { get; private set; }
            public int LevelIndex { get; private set; }

            public void AddToTotalScore(int points)
            {
                TotalScore += points;
            }

            public void SetLevelIndex(int index)
            {
                LevelIndex = index;
            }
        }
    }
}
