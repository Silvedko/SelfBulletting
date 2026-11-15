using SelfBulletting.Gameplay.Modes;
using UnityEngine;

namespace SelfBulletting.Infrastructure.Services.Save
{
    /// <summary>
    /// Classic keeps the keys it had before the modes appeared, so existing progress survives; other modes add a suffix.
    /// </summary>
    public sealed class PlayerPrefsSaveService : ISaveService
    {
        private const string TotalScoreKey = "total_score";
        private const string LevelIndexKey = "level_index";

        public int GetTotalScore(GameMode mode) => PlayerPrefs.GetInt(GetKey(TotalScoreKey, mode), 0);

        public int GetLevelIndex(GameMode mode) => PlayerPrefs.GetInt(GetKey(LevelIndexKey, mode), 0);

        public void AddToTotalScore(GameMode mode, int points)
        {
            PlayerPrefs.SetInt(GetKey(TotalScoreKey, mode), GetTotalScore(mode) + points);
            PlayerPrefs.Save();
        }

        public void SetLevelIndex(GameMode mode, int index)
        {
            PlayerPrefs.SetInt(GetKey(LevelIndexKey, mode), index);
            PlayerPrefs.Save();
        }

        private static string GetKey(string key, GameMode mode) =>
            mode == GameMode.Classic ? key : $"{key}_{mode.ToString().ToLowerInvariant()}";
    }
}
