using System;
using SelfBulletting.Gameplay.Modes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SelfBulletting.UI.Menu
{
    /// <summary>
    /// One game mode in the menu: the button starts it, the labels show its level and total score.
    /// </summary>
    public sealed class ModeButton : MonoBehaviour
    {
        [SerializeField] private GameMode _mode;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private string _levelFormat = "Level {0}";
        [SerializeField] private string _scoreFormat = "Score: {0}";

        public event Action<GameMode> Clicked;

        public GameMode Mode => _mode;

        public void Show(int levelNumber, int totalScore)
        {
            _levelText.text = string.Format(_levelFormat, levelNumber);
            _scoreText.text = string.Format(_scoreFormat, totalScore);
        }

        public void SetInteractable(bool isInteractable)
        {
            _button.interactable = isInteractable;
        }

        private void Awake()
        {
            _button.onClick.AddListener(OnClicked);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnClicked);
        }

        private void OnClicked()
        {
            Clicked?.Invoke(_mode);
        }
    }
}
