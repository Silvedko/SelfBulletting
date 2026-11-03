namespace SelfBulletting.Infrastructure.Services.Save
{
    public interface ISaveService
    {
        int TotalScore { get; }

        /// <summary>Index of the level the player plays next.</summary>
        int LevelIndex { get; }

        void AddToTotalScore(int points);

        void SetLevelIndex(int index);
    }
}
