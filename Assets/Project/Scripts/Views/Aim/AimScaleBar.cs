using DG.Tweening;
using UnityEngine;

namespace SelfBulletting.Views.Aim
{
    /// <summary>
    /// The aim scale in the HUD: the fill shows the current value, the part above the affordable limit is dimmed.
    /// Fill, dimmed part and marker are stretched by anchors, so the bar keeps its look at any width.
    /// </summary>
    public sealed class AimScaleBar : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _fill;
        [SerializeField] private RectTransform _lockedPart;
        [SerializeField] private RectTransform _marker;
        [SerializeField, Min(0f)] private float _fadeSeconds = 0.12f;

        private Tween _fade;

        public void Show()
        {
            FadeTo(1f);
        }

        public void Hide()
        {
            FadeTo(0f);
        }

        /// <param name="value">Current value, 0..1.</param>
        /// <param name="limit">Affordable upper bound, 0..1.</param>
        public void SetValue(float value, float limit)
        {
            _fill.anchorMax = new Vector2(value, _fill.anchorMax.y);
            _lockedPart.anchorMin = new Vector2(limit, _lockedPart.anchorMin.y);
            _marker.anchorMin = new Vector2(value, _marker.anchorMin.y);
            _marker.anchorMax = new Vector2(value, _marker.anchorMax.y);
        }

        private void Awake()
        {
            _group.alpha = 0f;
        }

        private void FadeTo(float alpha)
        {
            _fade?.Kill();
            _fade = _group.DOFade(alpha, _fadeSeconds).SetLink(gameObject);
        }
    }
}
