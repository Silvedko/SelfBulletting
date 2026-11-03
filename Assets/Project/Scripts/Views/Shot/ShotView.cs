using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Views.Ball;
using SelfBulletting.Views.Path;
using UnityEngine;

namespace SelfBulletting.Views.Shot
{
    /// <summary>
    /// The shot ball. It pops out of the player ball towards the exit, then rolls straight along the path
    /// until it is level with the impact point. The impact point may lie off the path centre (the path is as wide
    /// as the ball), so the shot never turns towards it.
    /// </summary>
    public sealed class ShotView : MonoBehaviour, IShotView
    {
        private const float DiameterPerRadius = 2f;

        [Tooltip("Sphere of 1 m diameter, child of this object.")]
        [SerializeField] private Transform _body;

        [Tooltip("The last shot is fired by a tap without the scale, so nobody calls ShowCharging: it separates from this ball.")]
        [SerializeField] private BallView _ball;

        [Tooltip("Shots fly straight along this path, towards the exit.")]
        [SerializeField] private PathView _path;

        [Tooltip("Shot radius until ShowCharging reports the real one, meters.")]
        [SerializeField, Min(0.01f)] private float _radius = 0.45f;

        [Tooltip("Gap between the player ball and the separated shot, meters.")]
        [SerializeField, Min(0f)] private float _gap = 0.1f;

        [SerializeField, Min(0.01f)] private float _separationSeconds = 0.15f;

        private Tween _separation;
        private Tween _flight;
        private float _groundHeight;

        public void ShowCharging(Vector3 ballCenter, float ballRadius, Vector3 target, float shotRadius)
        {
            _radius = shotRadius;
            Separate(ballCenter, ballRadius);
        }

        public async UniTask FlyAsync(Vector3 target, float speed, CancellationToken cancellationToken)
        {
            if (!_body.gameObject.activeSelf)
            {
                Separate(_ball.Center, _ball.Radius);
            }

            if (_separation.IsActive())
            {
                await _separation.ToUniTask(cancellationToken: cancellationToken);
            }

            Vector3 end = ToRollingHeight(GetLevelPointOnPath(target));
            float seconds = Vector3.Distance(_body.position, end) / speed;
            _flight = _body.DOMove(end, seconds).SetEase(Ease.Linear).SetLink(gameObject);
            await _flight.ToUniTask(cancellationToken: cancellationToken);
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

        private void Separate(Vector3 ballCenter, float ballRadius)
        {
            _groundHeight = ballCenter.y - ballRadius;
            Vector3 restPoint = ToRollingHeight(ballCenter + _path.Direction * (ballRadius + _radius + _gap));

            _flight?.Kill();
            _separation?.Kill();
            _body.position = ToRollingHeight(ballCenter);
            _body.localScale = Vector3.zero;
            _body.gameObject.SetActive(true);

            _separation = DOTween.Sequence()
                .Join(_body.DOMove(restPoint, _separationSeconds).SetEase(Ease.OutBack))
                .Join(_body.DOScale(_radius * DiameterPerRadius, _separationSeconds).SetEase(Ease.OutBack))
                .SetLink(gameObject);
        }

        // The point ahead on the path centre line that is level with the target; never behind the shot.
        private Vector3 GetLevelPointOnPath(Vector3 target)
        {
            float distance = Mathf.Max(Vector3.Dot(target - _body.position, _path.Direction), 0f);
            return _body.position + _path.Direction * distance;
        }

        private Vector3 ToRollingHeight(Vector3 point) => new Vector3(point.x, _groundHeight + _radius, point.z);
    }
}
