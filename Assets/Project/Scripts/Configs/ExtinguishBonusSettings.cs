using System;
using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// Bonus for fires put out together: every fire that goes out within <see cref="WindowMilliseconds"/> of the first
    /// one counts towards one burst, and the highest tier the burst reaches multiplies its points. A mode without tiers
    /// has no such bonus.
    /// </summary>
    [Serializable]
    public sealed class ExtinguishBonusSettings
    {
        private const int ShortestWindowMilliseconds = 1;
        private const float MillisecondsPerSecond = 1000f;

        [Tooltip("Fires that go out within this many milliseconds of the first one count as put out together.")]
        [SerializeField, Min(ShortestWindowMilliseconds)] private int _windowMilliseconds = 300;

        [Tooltip("The highest tier the burst reaches is applied: its points are multiplied, the base points of every " +
            "fire are already counted, so the bonus adds the rest.")]
        [SerializeField] private List<ComboTier> _tiers = new List<ComboTier>
        {
            new ComboTier(2, 1.5f),
            new ComboTier(3, 2f),
            new ComboTier(5, 3f)
        };

        public ExtinguishBonusSettings()
        {
        }

        public ExtinguishBonusSettings(int windowMilliseconds, IEnumerable<ComboTier> tiers)
        {
            _windowMilliseconds = windowMilliseconds;
            _tiers = new List<ComboTier>(tiers);
            Validate();
        }

        /// <summary>No tiers: fires put out together earn nothing extra.</summary>
        public static ExtinguishBonusSettings None { get; } =
            new ExtinguishBonusSettings(ShortestWindowMilliseconds, Array.Empty<ComboTier>());

        public int WindowMilliseconds => _windowMilliseconds;
        public float WindowSeconds => _windowMilliseconds / MillisecondsPerSecond;
        public IReadOnlyList<ComboTier> Tiers => _tiers;
        public bool HasTiers => _tiers.Count > 0;

        public void Validate()
        {
            _windowMilliseconds = Mathf.Max(ShortestWindowMilliseconds, _windowMilliseconds);
            _tiers.Sort((left, right) => left.MinObstacles.CompareTo(right.MinObstacles));
        }
    }
}
