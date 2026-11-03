using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.UI
{
    /// <summary>
    /// Full-screen splash in the Boot scene. Fades in over every scene switch, shows load progress and fades out.
    /// </summary>
    public sealed class LoadingCurtain : MonoBehaviour, IProgress<float>
    {
        private const float Opaque = 1f;
        private const float Transparent = 0f;

        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private RectTransform _progressFill;

        private LoadingSettings _settings;
        private Tween _fade;

        public void Construct(LoadingSettings settings)
        {
            _settings = settings;
        }

        /// <summary>Covers the screen at once: at app start there is nothing to fade from.</summary>
        public void Show()
        {
            _fade?.Kill();
            Report(0f);
            _canvasGroup.alpha = Opaque;
            _canvasGroup.blocksRaycasts = true;
            gameObject.SetActive(true);
        }

        public async UniTask ShowAsync(CancellationToken cancellationToken)
        {
            Report(0f);
            _canvasGroup.blocksRaycasts = true;
            gameObject.SetActive(true);
            await FadeAsync(Opaque, _settings.FadeInSeconds, cancellationToken);
        }

        public async UniTask HideAsync(CancellationToken cancellationToken)
        {
            await FadeAsync(Transparent, _settings.FadeOutSeconds, cancellationToken);
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        public void Report(float value)
        {
            Vector2 anchorMax = _progressFill.anchorMax;
            anchorMax.x = Mathf.Clamp01(value);
            _progressFill.anchorMax = anchorMax;
        }

        private UniTask FadeAsync(float alpha, float durationSeconds, CancellationToken cancellationToken)
        {
            _fade?.Kill();
            if (Mathf.Approximately(_canvasGroup.alpha, alpha))
            {
                return UniTask.CompletedTask;
            }

            _fade = _canvasGroup.DOFade(alpha, durationSeconds)
                .SetUpdate(true)
                .SetLink(gameObject);
            return _fade.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
        }
    }
}
