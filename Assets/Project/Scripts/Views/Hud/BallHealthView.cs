using System.Globalization;
using DG.Tweening;
using SelfBulletting.Gameplay.Presentation;
using TMPro;
using UnityEngine;

namespace SelfBulletting.Views.Hud
{
    /// <summary>
    /// HUD bar of what is left of the ball. Glides to each new value after a shot is resolved.
    /// </summary>
    public sealed class BallHealthView : MonoBehaviour, IBallHealthView
    {
        private const float FullSizePercent = 100f;

        [SerializeField] private RectTransform _fill;
        [SerializeField] private TMP_Text _valueText;

        [Tooltip("{0} — ball size, % of the start radius.")]
        [SerializeField] private string _valueFormat = "{0:0}%";

        [SerializeField, Min(0f)] private float _glideSeconds = 0.4f;

        private float _shownPercent = FullSizePercent;
        private float _shownHealth = 1f;
        private Tween _glide;

        public void Show(float radiusPercent, float health)
        {
            _glide?.Kill();

            float fromPercent = _shownPercent;
            float fromHealth = _shownHealth;
            float progress = 0f;

            _glide = DOTween.To(() => progress, value =>
                {
                    progress = value;
                    Apply(Mathf.Lerp(fromPercent, radiusPercent, value), Mathf.Lerp(fromHealth, health, value));
                }, 1f, _glideSeconds)
                .SetEase(Ease.OutCubic)
                .SetLink(gameObject);
        }

        private void Awake()
        {
            Apply(_shownPercent, _shownHealth);
        }

        private void Apply(float radiusPercent, float health)
        {
            _shownPercent = radiusPercent;
            _shownHealth = health;

            Vector2 anchorMax = _fill.anchorMax;
            anchorMax.x = Mathf.Clamp01(health);
            _fill.anchorMax = anchorMax;

            _valueText.text = string.Format(CultureInfo.InvariantCulture, _valueFormat, radiusPercent);
        }
    }
}
