namespace SelfBulletting.Infrastructure.Services.Save
{
    public interface ISaveService
    {
        int TotalScore { get; }

        void AddToTotalScore(int points);
    }
}
