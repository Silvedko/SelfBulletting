using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Views.Common;
using SelfBulletting.Views.Water;
using UnityEngine;

namespace SelfBulletting.Views.Obstacles
{
    /// <summary>
    /// Look of one obstacle: a burning flame. When the water reaches it, a puddle spreads under it, the flame shudders
    /// and the water climbs it from the base, turning it to steam; then it hisses out in a puff of steam and sinks
    /// into the ground. The douse level goes through a MaterialPropertyBlock, so every flame shares one material.
    /// </summary>
    public sealed class ObstacleView : MonoBehaviour, IObstacleView
    {
        [SerializeField] private Renderer _renderer;

        [Tooltip("The visible flame standing on the ground; it is scaled, the collider is not.")]
        [SerializeField] private Transform _model;

        [Tooltip("How far the water climbs the flame while it is infected: 0 — none, 1 — all steam.")]
        [SerializeField, Range(0f, 1f)] private float _infectedDouse = 0.55f;
        [SerializeField, Min(0f)] private float _infectSeconds = 0.2f;

        [Tooltip("Shudder on infection, share of the model size.")]
        [SerializeField, Range(0f, 1f)] private float _infectPunch = 0.15f;

        [SerializeField, Min(0f)] private float _douseSeconds = 0.08f;

        [Tooltip("Width of the flame, share of its size, when it has sunk into the ground.")]
        [SerializeField, Min(0f)] private float _sunkWidth = 1.3f;
        [SerializeField, Min(0f)] private float _sinkSeconds = 0.15f;

        [Tooltip("Where the steam comes out, share of the flame height above its base.")]
        [SerializeField, Range(0f, 1f)] private float _steamHeight = 0.35f;

        private SteamPuffEffect _steamPuff;
        private WaterSplashEffect _waterSplash;
        private MaterialPropertyBlock _properties;
        private DOGetter<float> _getDouse;
        private DOSetter<float> _setDouse;
        private TweenCallback _puff;
        private float _douse;
        private Vector3 _modelScale;
        private Vector3 _sunkScale;
        private Tween _infection;

        public void Construct(SteamPuffEffect steamPuff, WaterSplashEffect waterSplash)
        {
            _steamPuff = steamPuff;
            _waterSplash = waterSplash;
        }

        public void ShowInfected()
        {
            if (_waterSplash != null)
            {
                _waterSplash.PlaySpread(transform.position);
            }

            _infection = DOTween.Sequence()
                .Join(DOTween.To(_getDouse, _setDouse, _infectedDouse, _infectSeconds).SetEase(Ease.OutQuad))
                .Join(_model.DOPunchScale(_modelScale * _infectPunch, _infectSeconds))
                .SetLink(gameObject);
        }

        public UniTask PlayExplosionAsync(CancellationToken cancellationToken)
        {
            _infection?.Kill(complete: true);

            return DOTween.Sequence()
                .Append(DOTween.To(_getDouse, _setDouse, 1f, _douseSeconds).SetEase(Ease.OutQuad))
                .AppendCallback(_puff)
                .Append(_model.DOScale(_sunkScale, _sinkSeconds).SetEase(Ease.InQuad))
                .SetLink(gameObject)
                .ToUniTask(cancellationToken: cancellationToken);
        }

        private void Awake()
        {
            _properties = new MaterialPropertyBlock();
            _modelScale = _model.localScale;
            _sunkScale = new Vector3(_modelScale.x * _sunkWidth, 0f, _modelScale.z);
            _getDouse = () => _douse;
            _setDouse = SetDouse;
            _puff = Puff;
        }

        private void SetDouse(float douse)
        {
            _douse = douse;
            _properties.SetFloat(ShaderIds.Douse, douse);
            _renderer.SetPropertyBlock(_properties);
        }

        private void Puff()
        {
            if (_steamPuff != null)
            {
                _steamPuff.Play(_model.position + Vector3.up * (_model.lossyScale.y * _steamHeight));
            }
        }
    }
}
