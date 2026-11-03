using UnityEngine;

namespace SelfBulletting.Views.Aim
{
    /// <summary>
    /// Damage zone on the ground around the impact point: a translucent disc with an outline.
    /// It breathes with the aim scale because its radius follows the scale every frame, and it moves with the impact point,
    /// which shifts as the growing shot touches the obstacles at another spot.
    /// </summary>
    public sealed class DamageZoneView : MonoBehaviour
    {
        private const float DiameterPerRadius = 2f;
        private const float FullCircle = Mathf.PI * 2f;

        [SerializeField] private LineRenderer _outline;

        [Tooltip("Cylinder primitive (1 m diameter), child of this object.")]
        [SerializeField] private Transform _disc;

        [Tooltip("The ground the zone lies on.")]
        [SerializeField] private Transform _ground;

        [SerializeField, Min(8)] private int _outlineSegments = 64;

        [Tooltip("Lift above the ground (and the path strip) against z-fighting, meters.")]
        [SerializeField, Min(0f)] private float _groundOffset = 0.03f;

        [Tooltip("Height scale of the disc cylinder.")]
        [SerializeField, Min(0.0001f)] private float _discThickness = 0.005f;

        private Vector3[] _outlinePoints;

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void MoveTo(Vector3 impactPoint)
        {
            transform.position = new Vector3(impactPoint.x, _ground.position.y + _groundOffset, impactPoint.z);
        }

        public void SetRadius(float radius)
        {
            for (int i = 0; i < _outlinePoints.Length; i++)
            {
                float angle = FullCircle * i / _outlinePoints.Length;
                _outlinePoints[i] = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
            }

            _outline.SetPositions(_outlinePoints);
            float diameter = radius * DiameterPerRadius;
            _disc.localScale = new Vector3(diameter, _discThickness, diameter);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // Runs on the first Show: the zone stays inactive in the scene until the player aims.
        private void Awake()
        {
            _outlinePoints = new Vector3[_outlineSegments];
            _outline.useWorldSpace = false;
            _outline.loop = true;
            _outline.positionCount = _outlineSegments;
        }
    }
}
