using System;
using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Gameplay.Levels
{
    /// <summary>
    /// Every level of the game in the order it is played. New levels are added by SelfBulletting → Level → Create New Level.
    /// </summary>
    [CreateAssetMenu(fileName = nameof(LevelCatalog), menuName = "SelfBulletting/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [Tooltip("Level prefabs in play order. After the last one the game starts over from the first.")]
        [SerializeField] private List<Level> _levels = new List<Level>();

        public int Count => _levels.Count;

        public Level Get(int index)
        {
            if (index < 0 || index >= _levels.Count || _levels[index] == null)
            {
                throw new InvalidOperationException($"{name} has no level at index {index} (it holds {_levels.Count}).");
            }

            return _levels[index];
        }
    }
}
