using System;
using SelfBulletting.Gameplay.Modes;
using TMPro;
using UnityEngine;

namespace SelfBulletting.UI.Menu
{
    /// <summary>
    /// A button for every game mode; each shows the level and the total score of its mode.
    /// The game version (Player Settings → Version) is shown at the bottom.
    /// </summary>
    public sealed class MenuScreen : MonoBehaviour
    {
        [SerializeField] private ModeButton[] _modeButtons;
        [SerializeField] private TMP_Text _versionText;

        public event Action<GameMode> PlayRequested;

        public void Show(GameMode mode, int totalScore, int levelNumber)
        {
            foreach (ModeButton button in _modeButtons)
            {
                if (button.Mode == mode)
                {
                    button.Show(levelNumber, totalScore);
                    button.SetInteractable(true);
                }
            }
        }

        private void Awake()
        {
            if (_versionText != null)
            {
                _versionText.text = $"v{Application.version}";
            }

            foreach (ModeButton button in _modeButtons)
            {
                button.Clicked += OnModeClicked;
            }
        }

        private void OnDestroy()
        {
            foreach (ModeButton button in _modeButtons)
            {
                button.Clicked -= OnModeClicked;
            }
        }

        private void OnModeClicked(GameMode mode)
        {
            foreach (ModeButton button in _modeButtons)
            {
                button.SetInteractable(false);
            }

            PlayRequested?.Invoke(mode);
        }
    }
}
