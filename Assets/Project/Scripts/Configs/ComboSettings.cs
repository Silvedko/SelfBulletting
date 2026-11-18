using System;
using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Configs
{
    /// <summary>
    /// Combo tiers of the modes that shoot: the more obstacles one shot puts out, the bigger its multiplier.
    /// A mode without tiers has no combo at all.
    /// </summary>
    [Serializable]
    public sealed class ComboSettings
    {
        [Tooltip("The highest tier the shot reaches is applied.")]
        [SerializeField] private List<ComboTier> _tiers = new List<ComboTier>
        {
            new ComboTier(3, 1.5f),
            new ComboTier(6, 2f),
            new ComboTier(10, 3f)
        };

        public ComboSettings()
        {
        }

        public ComboSettings(IEnumerable<ComboTier> tiers)
        {
            _tiers = new List<ComboTier>(tiers);
            Validate();
        }

        /// <summary>No tiers: points are never multiplied.</summary>
        public static ComboSettings None { get; } = new ComboSettings(Array.Empty<ComboTier>());

        public IReadOnlyList<ComboTier> Tiers => _tiers;
        public bool HasTiers => _tiers.Count > 0;

        public void Validate()
        {
            _tiers.Sort((left, right) => left.MinObstacles.CompareTo(right.MinObstacles));
        }
    }
}
