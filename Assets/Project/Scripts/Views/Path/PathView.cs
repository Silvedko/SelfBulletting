using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Views.Common;
using UnityEngine;

namespace SelfBulletting.Views.Path
{
    /// <summary>
    /// The pink strip on the ground from the ball to the finish. Its width eases to the ball diameter;
    /// the warning pulse widens and tints it in sync with the ball.
    /// </summary>
    public sealed class PathView : MonoBehaviour, IPathView
    {
        private const float WidthPerRadius = 2f;
        private const float QuadToGroundAngle = 90f;

        [Tooltip("Quad primitive, child of this object: local X is the width, local Y the length.")]
        [SerializeField] private Transform _strip;
        [SerializeField] private Renderer _renderer;

        [Tooltip("Lift above the ground against z-fighting, meters.")]
        [SerializeField, Min(0f)] private float _groundOffset = 0.01f;

        [Tooltip("How fast the shown width catches up with the ball. Higher is snappier.")]
        [SerializeField, Min(1f)] private float _resizeSharpness = 20f;

        [SerializeField] private DangerPulseSettings _dangerPulse = new DangerPulseSettings();

        private SmoothedValue _radius;
        private DangerPulse _pulse;
        private MaterialPropertyBlock _properties;
        private Color _baseColor;
        private float _length;
        private bool _hasWidth;
        private float _shownPhase = -1f;

        /// <summary>Flat unit direction from the ball to the exit. Shots fly along it.</summary>
        public Vector3 Direction { get; private set; } = Vector3.forward;

        public void SetPath(Vector3 fromGroundPoint, Vector3 toGroundPoint)
        {
            Vector3 direction = toGroundPoint - fromGroundPoint;
            _length = direction.magnitude;
            Direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

            Vector3 middle = Vector3.Lerp(fromGroundPoint, toGroundPoint, 0.5f) + Vector3.up * _groundOffset;
            Quaternion flatAlongPath = Quaternion.LookRotation(direction) * Quaternion.Euler(QuadToGroundAngle, 0f, 0f);
            _strip.SetPositionAndRotation(middle, flatAlongPath);
            ApplyWidth();
        }

        public void SetWidth(float ballWorldRadius)
        {
            if (_hasWidth)
            {
                _radius.SetTarget(ballWorldRadius);
                return;
            }

            _hasWidth = true;
            _radius.Snap(ballWorldRadius);
            ApplyWidth();
        }

        public void SetDanger(float level)
        {
            _pulse.SetLevel(level);
        }

        private void Awake()
        {
            _radius = new SmoothedValue(_resizeSharpness);
            _pulse = new DangerPulse(_dangerPulse, gameObject);
            _properties = new MaterialPropertyBlock();
            _baseColor = _renderer.sharedMaterial.GetColor(ShaderIds.BaseColor);
        }

        private void LateUpdate()
        {
            _radius.Tick(Time.deltaTime);
            ApplyWidth();
            ApplyColor();
        }

        private void ApplyWidth()
        {
            float width = _radius.Current * WidthPerRadius * _pulse.SizeFactor;
            _strip.localScale = new Vector3(width, _length, 1f);
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
