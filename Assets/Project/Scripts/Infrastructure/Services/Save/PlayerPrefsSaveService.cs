using UnityEngine;

namespace SelfBulletting.Infrastructure.Services.Save
{
    public sealed class PlayerPrefsSaveService : ISaveService
    {
        private const string TotalScoreKey = "total_score";

        public int TotalScore => PlayerPrefs.GetInt(TotalScoreKey, 0);

        public void AddToTotalScore(int points)
        {
            PlayerPrefs.SetInt(TotalScoreKey, TotalScore + points);
            PlayerPrefs.Save();
        }
    }
}
