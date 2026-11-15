using System;
using SelfBulletting.Gameplay.Modes;
using UnityEngine;

namespace SelfBulletting.UI.Menu
{
    /// <summary>
    /// Composition root of the Menu scene.
    /// </summary>
    public sealed class MenuEntryPoint : MonoBehaviour
    {
        [SerializeField] private MenuScreen _screen;

        public event Action<GameMode> PlayRequested
        {
            add => _screen.PlayRequested += value;
            remove => _screen.PlayRequested -= value;
        }

        public void ShowMode(GameMode mode, int totalScore, int levelNumber)
        {
            _screen.Show(mode, totalScore, levelNumber);
        }
    }
}
