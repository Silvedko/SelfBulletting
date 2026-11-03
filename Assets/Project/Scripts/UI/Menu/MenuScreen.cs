using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SelfBulletting.UI.Menu
{
    public sealed class MenuScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _totalScoreText;
        [SerializeField] private Button _playButton;
        [SerializeField] private string _totalScoreFormat = "Total score: {0}";

        public event Action PlayRequested;

        public void Show(int totalScore)
        {
            _totalScoreText.text = string.Format(_totalScoreFormat, totalScore);
            _playButton.interactable = true;
        }

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
        }

        private void OnPlayClicked()
        {
            _playButton.interactable = false;
            PlayRequested?.Invoke();
        }
    }
}
