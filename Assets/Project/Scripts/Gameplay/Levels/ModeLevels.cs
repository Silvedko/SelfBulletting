using System;
using System.Collections.Generic;
using SelfBulletting.Gameplay.Modes;
using SelfBulletting.Infrastructure.Services.Save;

namespace SelfBulletting.Gameplay.Levels
{
    /// <summary>
    /// Which levels every game mode plays and where the player is in each. Classic plays its own catalog (one level);
    /// Slingshot, Arrow and Hose share the catalog of curved levels, each with its own progress.
    /// </summary>
    public sealed class ModeLevels
    {
        private readonly LevelCatalog _classic;
        private readonly LevelCatalog _curved;
        private readonly Dictionary<GameMode, LevelProgress> _progress = new Dictionary<GameMode, LevelProgress>();

        public ModeLevels(LevelCatalog classic, LevelCatalog curved, ISaveService save)
        {
            _classic = classic != null ? classic : throw new ArgumentNullException(nameof(classic), "Assign the Classic level catalog.");
            _curved = curved != null ? curved : throw new ArgumentNullException(nameof(curved), "Assign the curved level catalog.");

            foreach (GameMode mode in Modes)
            {
                _progress[mode] = new LevelProgress(save, mode, GetCatalog(mode).Count);
            }
        }

        public static IReadOnlyList<GameMode> Modes { get; } = (GameMode[])Enum.GetValues(typeof(GameMode));

        public LevelProgress GetProgress(GameMode mode) => _progress[mode];

        public Level GetCurrentLevel(GameMode mode) => GetCatalog(mode).Get(GetProgress(mode).CurrentIndex);

        private LevelCatalog GetCatalog(GameMode mode) => mode.UsesCurvedPath() ? _curved : _classic;
    }
}
