using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using TMPro;
using UnityEngine;

namespace SelfBulletting.Views.Score
{
    /// <summary>
    /// One pooled world-space label: pops in, floats up, fades out and hands itself back to the pool.
    /// </summary>
    public sealed class FloatingText : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField, Min(0f)] private float _popSeconds = 0.15f;

        [Tooltip("Share of the duration the text stays opaque before it fades.")]
        [SerializeField, Range(0f, 1f)] private float _opaqueShare = 0.5f;

        private Action<FloatingText> _onFinished;
        private TweenCallback _finish;
        private DOGetter<float> _getAlpha;
        private DOSetter<float> _setAlpha;
        private Sequence _sequence;

        public void Initialize(Action<FloatingText> onFinished)
        {
            _onFinished = onFinished;
        }

        /// <param name="cancellationToken">Cancelling it kills the animation (the label is not handed back to the pool).</param>
        public void Play(Vector3 position, Quaternion rotation, string text, FloatingTextStyle style,
            CancellationToken cancellationToken)
        {
            _sequence?.Kill();
            transform.SetPositionAndRotation(position, rotation);
            transform.localScale = Vector3.zero;

            _text.text = text;
            _text.fontSize = style.FontSize;
            _text.color = style.Color;

            float fadeDelay = style.DurationSeconds * _opaqueShare;
            _sequence = DOTween.Sequence()
                .Join(transform.DOScale(1f, _popSeconds).SetEase(Ease.OutBack))
                .Join(transform.DOMoveY(position.y + style.Rise, style.DurationSeconds).SetEase(Ease.OutCubic))
                .Insert(fadeDelay, DOTween.To(_getAlpha, _setAlpha, 0f, style.DurationSeconds - fadeDelay))
                .SetLink(gameObject)
                .OnComplete(_finish);

            _sequence.ToUniTask(TweenCancelBehaviour.Kill, cancellationToken).Forget();
        }

        private void Awake()
        {
            _finish = Finish;
            _getAlpha = () => _text.alpha;
            _setAlpha = alpha => _text.alpha = alpha;
        }

        private void Finish()
        {
            _onFinished?.Invoke(this);
        }
    }
}
