using SelfBulletting.Infrastructure.Services.Audio;
using UnityEngine;

namespace SelfBulletting.Gameplay.Score
{
    /// <summary>The bigger the combo, the brighter the sound; tiers above the last sound reuse the brightest one.</summary>
    public static class ComboSounds
    {
        private static readonly SoundId[] _sounds = { SoundId.Combo, SoundId.ComboBig, SoundId.ComboHuge };

        /// <param name="comboLevel">1 — the first tier, and so on.</param>
        public static SoundId ForLevel(int comboLevel) => _sounds[Mathf.Clamp(comboLevel - 1, 0, _sounds.Length - 1)];
    }
}
