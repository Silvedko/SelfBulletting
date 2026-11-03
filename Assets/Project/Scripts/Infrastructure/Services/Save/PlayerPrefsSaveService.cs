using UnityEngine;

namespace SelfBulletting.Infrastructure.Services.Save
{
    public sealed class PlayerPrefsSaveService : ISaveService
    {
        private const string TotalScoreKey = "total_score";
        private const string LevelIndexKey = "level_index";

        public int TotalScore => PlayerPrefs.GetInt(TotalScoreKey, 0);
        public int LevelIndex => PlayerPrefs.GetInt(LevelIndexKey, 0);

        public void AddToTotalScore(int points)
        {
            PlayerPrefs.SetInt(TotalScoreKey, TotalScore + points);
            PlayerPrefs.Save();
        }

        public void SetLevelIndex(int index)
        {
            PlayerPrefs.SetInt(LevelIndexKey, index);
            PlayerPrefs.Save();
        }
    }
}
