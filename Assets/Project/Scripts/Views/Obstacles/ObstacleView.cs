using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Views.Common;
using UnityEngine;

namespace SelfBulletting.Views.Obstacles
{
    /// <summary>
    /// Look of one obstacle: it turns the player's orange with a twitch when infected, then pops and shrinks away
    /// with a burst of chunks. The colour goes through a MaterialPropertyBlock, so every obstacle shares one material.
    /// </summary>
    public sealed class ObstacleView : MonoBehaviour, IObstacleView
    {
        [SerializeField] private Renderer _renderer;

        [Tooltip("The visible mesh; it is scaled, the collider is not.")]
        [SerializeField] private Transform _model;

        [SerializeField] private Color _infectedColor = new Color(0.96f, 0.6f, 0.25f);
        [SerializeField, Min(0f)] private float _infectSeconds = 0.1f;

        [Tooltip("Twitch on infection, share of the model size.")]
        [SerializeField, Range(0f, 1f)] private float _infectPunch = 0.15f;

        [SerializeField, Min(1f)] private float _popScale = 1.3f;
        [SerializeField, Min(0f)] private float _popSeconds = 0.08f;
        [SerializeField, Min(0f)] private float _shrinkSeconds = 0.15f;

        private ExplosionEffect _explosion;
        private MaterialPropertyBlock _properties;
        private DOGetter<Color> _getColor;
        private DOSetter<Color> _setColor;
        private TweenCallback _burst;
        private Color _color;
        private Vector3 _modelScale;
        private Tween _infection;

        public void Construct(ExplosionEffect explosion)
        {
            _explosion = explosion;
        }

        public void ShowInfected()
        {
            _infection = DOTween.Sequence()
                .Join(DOTween.To(_getColor, _setColor, _infectedColor, _infectSeconds))
                .Join(_model.DOPunchScale(_modelScale * _infectPunch, _infectSeconds))
                .SetLink(gameObject);
        }

        public UniTask PlayExplosionAsync(CancellationToken cancellationToken)
        {
            _infection?.Kill(complete: true);

            return DOTween.Sequence()
                .Append(_model.DOScale(_modelScale * _popScale, _popSeconds).SetEase(Ease.OutQuad))
                .AppendCallback(_burst)
                .Append(_model.DOScale(Vector3.zero, _shrinkSeconds).SetEase(Ease.InBack))
                .SetLink(gameObject)
                .ToUniTask(cancellationToken: cancellationToken);
        }

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            _color = _renderer.sharedMaterial.GetColor(ShaderIds.BaseColor);
            _modelScale = _model.localScale;
            _getColor = () => _color;
            _setColor = SetColor;
            _burst = Burst;
        }

        private void SetColor(Color color)
        {
            _color = color;
            _properties.SetColor(ShaderIds.BaseColor, color);
            _renderer.SetPropertyBlock(_properties);
        }

        private void Burst()
        {
            if (_explosion != null)
            {
                _explosion.Play(_model.position, _infectedColor);
            }
        }
    }
}
