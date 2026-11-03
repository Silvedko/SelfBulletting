using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Views.Common;
using UnityEngine;

namespace SelfBulletting.Views.Ball
{
    /// <summary>
    /// The player ball. The root stands on the ground point and hops; the body sphere is lifted by its radius.
    /// Size changes ease in, the warning pulse inflates the ball and tints it.
    /// </summary>
    public sealed class BallView : MonoBehaviour, IBallView
    {
        private const float DiameterPerRadius = 2f;

        [Tooltip("Sphere of 1 m diameter, child of this object.")]
        [SerializeField] private Transform _body;
        [SerializeField] private Renderer _renderer;

        [Tooltip("How fast the shown size catches up with the real one. Higher is snappier.")]
        [SerializeField, Min(1f)] private float _resizeSharpness = 20f;

        [SerializeField] private DangerPulseSettings _dangerPulse = new DangerPulseSettings();

        private SmoothedValue _radius;
        private DangerPulse _pulse;
        private MaterialPropertyBlock _properties;
        private Color _baseColor;
        private float _shownPhase = -1f;

        /// <summary>Real radius of the ball, without easing and pulsing.</summary>
        public float Radius => _radius.Target;

        /// <summary>Centre of the ball at its real size.</summary>
        public Vector3 Center => transform.position + Vector3.up * Radius;

        public void Place(Vector3 groundPoint, float worldRadius)
        {
            transform.position = groundPoint;
            _radius.Snap(worldRadius);
            ApplySize();
        }

        public void SetRadius(float worldRadius)
        {
            _radius.SetTarget(worldRadius);
        }

        public void SetDanger(float level)
        {
            _pulse.SetLevel(level);
        }

        public UniTask HopAsync(Vector3 groundPoint, float height, float durationSeconds, CancellationToken cancellationToken) =>
            transform.DOJump(groundPoint, height, 1, durationSeconds)
                .SetEase(Ease.Linear)
                .SetLink(gameObject)
                .ToUniTask(cancellationToken: cancellationToken);

        private void Awake()
        {
            _radius = new SmoothedValue(_resizeSharpness);
            _radius.Snap(_body.localScale.x / DiameterPerRadius);
            _pulse = new DangerPulse(_dangerPulse, gameObject);
            _properties = new MaterialPropertyBlock();
            _baseColor = _renderer.sharedMaterial.GetColor(ShaderIds.BaseColor);
        }

        private void LateUpdate()
        {
            _radius.Tick(Time.deltaTime);
            ApplySize();
            ApplyColor();
        }

        private void ApplySize()
        {
            float shownRadius = _radius.Current * _pulse.SizeFactor;
            _body.localPosition = Vector3.up * shownRadius;
            _body.localScale = Vector3.one * (shownRadius * DiameterPerRadius);
        }

        private void ApplyColor()
        {
            if (_pulse.Phase == _shownPhase)
            {
                return;
            }

            _shownPhase = _pulse.Phase;
            _properties.SetColor(ShaderIds.BaseColor, _pulse.Tint(_baseColor));
            _renderer.SetPropertyBlock(_properties);
        }
    }
}
