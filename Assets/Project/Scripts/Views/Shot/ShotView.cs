using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Shot;
using SelfBulletting.Views.Water;
using UnityEngine;

namespace SelfBulletting.Views.Shot
{
    /// <summary>
    /// The shot: a droplet of the player's water. It pops out in front of the player ball and follows the charge
    /// every frame: as big as what the ball gives away. In Classic it then rolls straight along the path to the point
    /// where it touches the obstacle; in the aim modes it is lobbed along the arc over the obstacles. Either way it
    /// splashes where it stops. Gameplay computes every position; this view only shows and animates them.
    /// </summary>
    public sealed class ShotView : MonoBehaviour, IShotView
    {
        private const float DiameterPerRadius = 2f;

        [Tooltip("Sphere of 1 m diameter with the water drop material, child of this object.")]
        [SerializeField] private Transform _body;

        [SerializeField, Min(0.01f)] private float _separationSeconds = 0.15f;

        [Tooltip("Splash where the shot lands. Optional.")]
        [SerializeField] private WaterSplashEffect _splash;

        private Tween _separation;
        private float _lobShare;
        private Tween _flight;
        private float _radius;
        private float _grown;

        public void ShowCharging(Vector3 launchCenter, float shotRadius)
        {
            _radius = shotRadius;
            if (!_body.gameObject.activeSelf)
            {
                Separate();
            }

            _body.position = launchCenter;
            ApplyScale();
        }

        public async UniTask FlyAsync(Vector3 contactCenter, float speed, CancellationToken cancellationToken)
        {
            if (_separation.IsActive())
            {
                await _separation.ToUniTask(cancellationToken: cancellationToken);
            }

            float seconds = Vector3.Distance(_body.position, contactCenter) / speed;
            _flight = _body.DOMove(contactCenter, seconds).SetEase(Ease.Linear).SetLink(gameObject);
            await _flight.ToUniTask(cancellationToken: cancellationToken);

            if (_splash != null)
            {
                _splash.PlayImpact(contactCenter + Vector3.down * _radius, _radius);
            }
        }

        public async UniTask LobAsync(ArcPath arc, float speed, CancellationToken cancellationToken)
        {
            if (_separation.IsActive())
            {
                await _separation.ToUniTask(cancellationToken: cancellationToken);
            }

            float seconds = Mathf.Max(arc.HorizontalLength / speed, Mathf.Epsilon);
            _lobShare = 0f;
            _flight = DOTween.To(() => _lobShare, share =>
                {
                    _lobShare = share;
                    _body.position = arc.Evaluate(share);
                }, 1f, seconds)
                .SetEase(Ease.Linear)
                .SetLink(gameObject);
            await _flight.ToUniTask(cancellationToken: cancellationToken);

            if (_splash != null)
            {
                _splash.PlayImpact(arc.End + Vector3.down * _radius, _radius);
            }
        }

        public void Hide()
        {
            _separation?.Kill();
            _flight?.Kill();
            _body.gameObject.SetActive(false);
        }

        private void Awake()
        {
            _body.gameObject.SetActive(false);
        }

        // Pops the shot in from nothing; the size keeps following the charge while it grows.
        private void Separate()
        {
            _flight?.Kill();
            _separation?.Kill();
            _grown = 0f;
            _body.gameObject.SetActive(true);

            _separation = DOTween.To(() => _grown, value =>
                {
                    _grown = value;
                    ApplyScale();
                }, 1f, _separationSeconds)
                .SetEase(Ease.OutBack)
                .SetLink(gameObject);
        }

        private void ApplyScale()
        {
            _body.localScale = Vector3.one * (_radius * DiameterPerRadius * _grown);
        }
    }
}
