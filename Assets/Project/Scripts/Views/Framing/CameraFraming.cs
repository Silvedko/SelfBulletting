using UnityEngine;

namespace SelfBulletting.Views.Framing
{
    /// <summary>
    /// Keeps the level in frame on any portrait screen (9:16 to 9:21) without moving the fixed camera:
    /// picks the smallest vertical field of view that shows every framing point with its margin.
    /// Fits again whenever the screen aspect changes.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class CameraFraming : MonoBehaviour
    {
        private const float AnglePerHalfAngle = 2f;
        private const float MinDepth = 0.01f;

        [Tooltip("Points that must stay on screen: the ball start, the finish building, its pin.")]
        [SerializeField] private Transform[] _points;

        [Tooltip("Free space around every point, meters.")]
        [SerializeField, Min(0f)] private float _margin = 1.5f;

        private Camera _camera;
        private float _fittedAspect;

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
                halfTangent = Mathf.Max(halfTangent, GetRequiredHalfTangent(point.position));
            }

            _camera.fieldOfView = Mathf.Atan(halfTangent) * Mathf.Rad2Deg * AnglePerHalfAngle;
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
