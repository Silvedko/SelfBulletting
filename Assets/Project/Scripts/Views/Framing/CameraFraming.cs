using UnityEngine;

namespace SelfBulletting.Views.Framing
{
    /// <summary>
    /// Keeps the level in frame on any portrait screen (9:16 to 9:21) without moving the fixed camera:
    /// picks the smallest vertical field of view that shows every framing point with its margin.
    /// Fits again when a level is framed and whenever the screen aspect changes.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFraming : MonoBehaviour
    {
        private const float AnglePerHalfAngle = 2f;
        private const float MinDepth = 0.01f;

        [Tooltip("Scene points that must stay on screen, such as the finish pin. The ball start comes from the level.")]
        [SerializeField] private Transform[] _points;

        [Tooltip("Free space around every point, meters.")]
        [SerializeField, Min(0f)] private float _margin = 1.5f;

        private Camera _camera;
        private float _fittedAspect;
        private Vector3? _levelStart;

        /// <summary>Called once the level is loaded and the finish building is in place.</summary>
        public void FrameLevel(Vector3 startGroundPoint)
        {
            _levelStart = startGroundPoint;
            Fit();
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            Fit();
        }

        private void LateUpdate()
        {
            if (!Mathf.Approximately(_camera.aspect, _fittedAspect))
            {
                Fit();
            }
        }

        private void Fit()
        {
            float halfTangent = 0f;
            foreach (Transform point in _points)
            {
                if (point != null)
                {
                    halfTangent = Mathf.Max(halfTangent, GetRequiredHalfTangent(point.position));
                }
            }

            if (_levelStart.HasValue)
            {
                halfTangent = Mathf.Max(halfTangent, GetRequiredHalfTangent(_levelStart.Value));
            }

            if (halfTangent > 0f)
            {
                _camera.fieldOfView = Mathf.Atan(halfTangent) * Mathf.Rad2Deg * AnglePerHalfAngle;
            }

            _fittedAspect = _camera.aspect;
        }

        // Tangent of half the vertical field of view that keeps the point and its margin inside the frustum.
        private float GetRequiredHalfTangent(Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            float depth = Mathf.Max(local.z, MinDepth);
            float vertical = (Mathf.Abs(local.y) + _margin) / depth;
            float horizontal = (Mathf.Abs(local.x) + _margin) / (depth * _camera.aspect);
            return Mathf.Max(vertical, horizontal);
        }
    }
}
