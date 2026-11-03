using System;
using UnityEngine;

namespace SelfBulletting.UI.Menu
{
    /// <summary>
    /// Composition root of the Menu scene.
    /// </summary>
    public sealed class MenuEntryPoint : MonoBehaviour
    {
        [SerializeField] private MenuScreen _screen;

        public event Action PlayRequested
        {
            add => _screen.PlayRequested += value;
            remove => _screen.PlayRequested -= value;
        }

        public void Construct(int totalScore, int levelNumber)
        {
            _screen.Show(totalScore, levelNumber);
        }
    }
}
