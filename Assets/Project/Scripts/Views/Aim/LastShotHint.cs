using DG.Tweening;
using UnityEngine;

namespace SelfBulletting.Views.Aim
{
    /// <summary>
    /// "Last shot" label in the HUD: fades in and pulses while the next tap fires the last shot without the scale.
    /// </summary>
    public sealed class LastShotHint : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _label;
        [SerializeField, Min(0f)] private float _fadeSeconds = 0.2f;
        [SerializeField, Min(1f)] private float _pulseScale = 1.12f;
        [SerializeField, Min(0.05f)] private float _pulseHalfSeconds = 0.4f;

        private Tween _fade;
        private Tween _pulse;
        private TweenCallback _stopPulse;

        public void SetVisible(bool isVisible)
        {
            _fade?.Kill();
            _fade = _group.DOFade(isVisible ? 1f : 0f, _fadeSeconds).SetLink(gameObject);

            if (isVisible)
            {
                _pulse.Play();
                return;
            }

            _fade.OnComplete(_stopPulse);
        }

        private void Awake()
        {
            _group.alpha = 0f;
            _stopPulse = StopPulse;
            _pulse = _label.DOScale(_pulseScale, _pulseHalfSeconds)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetAutoKill(false)
                .SetLink(gameObject)
                .Pause();
        }

        private void StopPulse()
        {
            _pulse.Rewind();
        }
    }
}
