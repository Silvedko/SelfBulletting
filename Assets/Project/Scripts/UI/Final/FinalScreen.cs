using System;
using System.Globalization;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SelfBulletting.Gameplay;
using SelfBulletting.Gameplay.Score;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SelfBulletting.UI.Final
{
    /// <summary>
    /// Overlay shown on top of the frozen Game scene when a round ends. The title pops in, then the score
    /// counts up line by line: destroyed obstacles → combo bonus → ball bonus → total. The buttons unlock when the count ends.
    /// </summary>
    public sealed class FinalScreen : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _menuButton;
        [SerializeField] private string _winTitle = "Victory!";
        [SerializeField] private string _loseTitle = "Defeat";

        [Tooltip("{0} — destroyed obstacles, {1} — combo bonus, {2} — bonus for the ball left, {3} — total score.")]
        [SerializeField] private string _summaryFormat = "Obstacles: {0}\nCombo bonus: +{1}\nBall bonus: +{2}\nScore: {3}";

        [Tooltip("How long one line of the score counts up, seconds.")]
        [SerializeField, Min(0f)] private float _countSecondsPerLine = 0.6f;

        [SerializeField, Min(0f)] private float _titlePopSeconds = 0.3f;

        public event Action RetryRequested;
        public event Action MenuRequested;

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public async UniTask ShowAsync(GameResult result, CancellationToken cancellationToken)
        {
            _titleText.text = result.IsWin ? _winTitle : _loseTitle;
            SetButtonsInteractable(false);
            ShowScore(0, 0, 0, 0);
            gameObject.SetActive(true);

            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, destroyCancellationToken))
            {
                await PlayCountUpAsync(result.Score, linked.Token);
            }

            SetButtonsInteractable(true);
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

        private async UniTask PlayCountUpAsync(ScoreSummary score, CancellationToken cancellationToken)
        {
            await PopTitleAsync(cancellationToken);
            await CountAsync(value => ShowScore(value, 0, 0, 0), score.DestroyedObstacles, cancellationToken);
            await CountAsync(value => ShowScore(score.DestroyedObstacles, value, 0, 0), score.ComboBonus, cancellationToken);
            await CountAsync(value => ShowScore(score.DestroyedObstacles, score.ComboBonus, value, 0), score.HealthBonus, cancellationToken);
            await CountAsync(value => ShowScore(score.DestroyedObstacles, score.ComboBonus, score.HealthBonus, value),
                score.Total, cancellationToken);
        }

        private UniTask PopTitleAsync(CancellationToken cancellationToken)
        {
            Transform title = _titleText.transform;
            title.localScale = Vector3.zero;
            return title.DOScale(1f, _titlePopSeconds)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject)
                .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
        }

        private UniTask CountAsync(Action<int> show, int target, CancellationToken cancellationToken)
        {
            if (target <= 0)
            {
                show(0);
                return UniTask.CompletedTask;
            }

            int current = 0;
            return DOTween.To(() => current, value =>
                {
                    current = value;
                    show(value);
                }, target, _countSecondsPerLine)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject)
                .ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
        }

        private void ShowScore(int obstacles, int comboBonus, int healthBonus, int total)
        {
            _scoreText.text = string.Format(CultureInfo.InvariantCulture, _summaryFormat, obstacles, comboBonus, healthBonus, total);
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
