using SelfBulletting.Gameplay.Modes;

namespace SelfBulletting.Infrastructure.Services.Save
{
    /// <summary>
    /// Player progress. Every game mode has its own total score and its own current level.
    /// </summary>
    public interface ISaveService
    {
        int GetTotalScore(GameMode mode);

        /// <summary>Index of the level the player plays next in this mode.</summary>
        int GetLevelIndex(GameMode mode);

        void AddToTotalScore(GameMode mode, int points);

        void SetLevelIndex(GameMode mode, int index);
    }
}
