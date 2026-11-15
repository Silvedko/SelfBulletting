using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Views.Aim
{
    /// <summary>
    /// Aim preview of the aim modes: a dotted arc of the lobbed shot whose dots flow towards the landing point, and the
    /// arrow of the Arrow mode lying on the ground next to the ball.
    /// </summary>
    public sealed class LobAimView : MonoBehaviour, ILobAimView
    {
        [Tooltip("One dot of the arc: a small sphere, child of this object. The other dots are copies of it.")]
        [SerializeField] private Transform _dotTemplate;

        [SerializeField, Min(2)] private int _dotCount = 18;

        [Tooltip("Dot diameter at the launch and at the landing end of the arc, meters.")]
        [SerializeField, Min(0f)] private float _dotSizeAtLaunch = 0.2f;
        [SerializeField, Min(0f)] private float _dotSizeAtLanding = 0.12f;

        [Tooltip("How many dot spacings the dots move towards the landing point per second.")]
        [SerializeField, Min(0f)] private float _flowSpeed = 1.5f;

        [Tooltip("The arrow: a line renderer whose width curve draws the shaft and the head.")]
        [SerializeField] private LineRenderer _arrow;

        [SerializeField, Min(0.1f)] private float _arrowLength = 3f;

        [Tooltip("Gap between the ball and the arrow tail, meters.")]
        [SerializeField, Min(0f)] private float _arrowGap = 0.2f;

        [Tooltip("Lift above the ground (and the path strip) against z-fighting, meters.")]
        [SerializeField, Min(0f)] private float _groundOffset = 0.04f;

        private Transform[] _dots;
        private bool _isTrajectoryShown;

        public void ShowArrow(Vector3 ballGroundPoint, float ballRadius, Vector3 direction)
        {
            Vector3 tail = ballGroundPoint + direction * (ballRadius + _arrowGap) + Vector3.up * _groundOffset;
            _arrow.SetPosition(0, tail);
            _arrow.SetPosition(1, tail + direction * _arrowLength);
            _arrow.enabled = true;
        }

        public void HideArrow()
        {
            _arrow.enabled = false;
        }

        public void ShowTrajectory(ArcPath arc)
        {
            float flow = Mathf.Repeat(Time.time * _flowSpeed, 1f);
            for (int i = 0; i < _dots.Length; i++)
            {
                float share = (i + flow) / _dots.Length;
                Transform dot = _dots[i];
                dot.position = arc.Evaluate(share);
                dot.localScale = Vector3.one * Mathf.Lerp(_dotSizeAtLaunch, _dotSizeAtLanding, share);
                if (!_isTrajectoryShown)
                {
                    dot.gameObject.SetActive(true);
                }
            }

            _isTrajectoryShown = true;
        }

        public void HideTrajectory()
        {
            if (!_isTrajectoryShown)
            {
                return;
            }

            foreach (Transform dot in _dots)
            {
                dot.gameObject.SetActive(false);
            }

            _isTrajectoryShown = false;
        }

        private void Awake()
        {
            _dots = new Transform[_dotCount];
            _dots[0] = _dotTemplate;
            for (int i = 1; i < _dots.Length; i++)
            {
                _dots[i] = Instantiate(_dotTemplate, _dotTemplate.parent);
                _dots[i].name = $"{_dotTemplate.name} {i}";
            }

            foreach (Transform dot in _dots)
            {
                dot.gameObject.SetActive(false);
            }

            _arrow.useWorldSpace = true;
            _arrow.positionCount = 2;
            _arrow.enabled = false;
        }
    }
}
