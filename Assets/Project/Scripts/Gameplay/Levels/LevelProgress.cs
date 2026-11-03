using System;
using SelfBulletting.Infrastructure.Services.Save;

namespace SelfBulletting.Gameplay.Levels
{
    /// <summary>
    /// Which level the player is on. A win moves to the next level; after the last one the game starts over from the first.
    /// A loss keeps the level, so Retry replays it.
    /// </summary>
    public sealed class LevelProgress
    {
        private readonly ISaveService _save;
        private readonly int _levelCount;

        public LevelProgress(ISaveService save, int levelCount)
        {
            if (levelCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelCount), levelCount, "The game needs at least one level.");
            }

            _save = save;
            _levelCount = levelCount;
        }

        /// <summary>Always a valid index, even if the saved one points past a catalog that got shorter.</summary>
        public int CurrentIndex => Wrap(_save.LevelIndex);

        public int CurrentNumber => CurrentIndex + 1;

        public void CompleteCurrent()
        {
            _save.SetLevelIndex(Wrap(CurrentIndex + 1));
        }

        private int Wrap(int index) => (index % _levelCount + _levelCount) % _levelCount;
    }
}
