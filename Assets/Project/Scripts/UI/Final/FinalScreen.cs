using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SelfBulletting.UI.Final
{
    /// <summary>
    /// Overlay shown on top of the frozen Game scene when a round ends.
    /// </summary>
    public sealed class FinalScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private string _winTitle = "Victory!";
        [SerializeField] private string _loseTitle = "Defeat";
        [SerializeField] private string _scoreFormat = "Score: {0}";

        public event Action RetryRequested;
        public event Action MenuRequested;

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public UniTask ShowAsync(GameResult result, CancellationToken cancellationToken)
        {
            _titleText.text = result.IsWin ? _winTitle : _loseTitle;
            _scoreText.text = string.Format(_scoreFormat, result.Score.Total);
            SetButtonsInteractable(true);
            gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        private void Awake()
        {
            _retryButton.onClick.AddListener(OnRetryClicked);
            _menuButton.onClick.AddListener(OnMenuClicked);
        }

        private void OnDestroy()
        {
            _retryButton.onClick.RemoveListener(OnRetryClicked);
            _menuButton.onClick.RemoveListener(OnMenuClicked);
        }

        private void OnRetryClicked()
        {
            SetButtonsInteractable(false);
            RetryRequested?.Invoke();
        }

        private void OnMenuClicked()
        {
            SetButtonsInteractable(false);
            MenuRequested?.Invoke();
        }

        private void SetButtonsInteractable(bool isInteractable)
        {
            _retryButton.interactable = isInteractable;
            _menuButton.interactable = isInteractable;
        }
    }
}
