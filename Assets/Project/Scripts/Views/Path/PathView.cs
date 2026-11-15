using System.Collections.Generic;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Views.Common;
using UnityEngine;

namespace SelfBulletting.Views.Path
{
    /// <summary>
    /// The water-blue strip on the ground from the ball to the finish: a ribbon mesh along the path points, straight
    /// or curved. Its width eases to the ball diameter; the warning pulse widens and tints it in sync with the ball.
    /// </summary>
    public sealed class PathView : MonoBehaviour, IPathView
    {
        private const float WidthPerRadius = 2f;
        private const float HalfWidthPerWidth = 0.5f;
        private const float MaxMiterFactor = 2f;
        private const float WidthTolerance = 0.0005f;

        [Tooltip("Mesh filter of the strip; its mesh is replaced by the ribbon.")]
        [SerializeField] private MeshFilter _meshFilter;
        [SerializeField] private Renderer _renderer;

        [Tooltip("Lift above the ground against z-fighting, meters.")]
        [SerializeField, Min(0f)] private float _groundOffset = 0.01f;

        [Tooltip("How fast the shown width catches up with the ball. Higher is snappier.")]
        [SerializeField, Min(1f)] private float _resizeSharpness = 20f;

        [SerializeField] private DangerPulseSettings _dangerPulse = new DangerPulseSettings();

        private readonly List<Vector3> _centers = new List<Vector3>();
        private readonly List<Vector3> _sides = new List<Vector3>();

        private SmoothedValue _radius;
        private DangerPulse _pulse;
        private MaterialPropertyBlock _properties;
        private Color _baseColor;
        private Mesh _mesh;
        private Vector3[] _vertices = new Vector3[0];
        private bool _hasWidth;
        private float _shownWidth = -1f;
        private float _shownPhase = -1f;

        public void SetPath(IReadOnlyList<Vector3> groundPoints)
        {
            _centers.Clear();
            _sides.Clear();
            for (int i = 0; i < groundPoints.Count; i++)
            {
                _centers.Add(groundPoints[i] + Vector3.up * _groundOffset);
                _sides.Add(GetSide(groundPoints, i));
            }

            BuildMesh();
            _shownWidth = -1f;
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
            _mesh = new Mesh { name = "PathRibbon" };
            _mesh.MarkDynamic();
            _meshFilter.sharedMesh = _mesh;
        }

        private void OnDestroy()
        {
            Destroy(_mesh);
        }

        private void LateUpdate()
        {
            _radius.Tick(Time.deltaTime);
            ApplyWidth();
            ApplyColor();
        }

        // Two vertices per path point, across the path; UV v runs along the path in meters.
        private void BuildMesh()
        {
            int count = _centers.Count;
            _vertices = new Vector3[count * 2];
            var uvs = new Vector2[count * 2];
            var triangles = new int[Mathf.Max(0, count - 1) * 6];

            float along = 0f;
            for (int i = 0; i < count; i++)
            {
                along += i > 0 ? Vector3.Distance(_centers[i - 1], _centers[i]) : 0f;
                uvs[i * 2] = new Vector2(0f, along);
                uvs[i * 2 + 1] = new Vector2(1f, along);
            }

            for (int i = 0; i < count - 1; i++)
            {
                int vertex = i * 2;
                int triangle = i * 6;
                triangles[triangle] = vertex;
                triangles[triangle + 1] = vertex + 2;
                triangles[triangle + 2] = vertex + 1;
                triangles[triangle + 3] = vertex + 1;
                triangles[triangle + 4] = vertex + 2;
                triangles[triangle + 5] = vertex + 3;
            }

            _mesh.Clear();
            _mesh.vertices = _vertices;
            _mesh.uv = uvs;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
        }

        private void ApplyWidth()
        {
            float width = _radius.Current * WidthPerRadius * _pulse.SizeFactor;
            if (_centers.Count < 2 || Mathf.Abs(width - _shownWidth) < WidthTolerance)
            {
                return;
            }

            _shownWidth = width;
            Transform strip = _meshFilter.transform;
            float halfWidth = width * HalfWidthPerWidth;
            for (int i = 0; i < _centers.Count; i++)
            {
                _vertices[i * 2] = strip.InverseTransformPoint(_centers[i] - _sides[i] * halfWidth);
                _vertices[i * 2 + 1] = strip.InverseTransformPoint(_centers[i] + _sides[i] * halfWidth);
            }

            _mesh.vertices = _vertices;
            _mesh.RecalculateBounds();
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

        // Points to the right of the path. At a bend it is the mitre of both segments, so the strip keeps its width.
        private static Vector3 GetSide(IReadOnlyList<Vector3> points, int index)
        {
            Vector3 incoming = index > 0 ? FlatDirection(points[index - 1], points[index]) : Vector3.zero;
            Vector3 outgoing = index < points.Count - 1 ? FlatDirection(points[index], points[index + 1]) : Vector3.zero;
            Vector3 tangent = (incoming + outgoing).normalized;
            if (tangent == Vector3.zero)
            {
                tangent = outgoing != Vector3.zero ? outgoing : incoming;
            }

            Vector3 side = Vector3.Cross(Vector3.up, tangent);
            Vector3 segmentSide = Vector3.Cross(Vector3.up, outgoing != Vector3.zero ? outgoing : incoming);
            float cosine = Vector3.Dot(side, segmentSide);
            return side * Mathf.Min(1f / Mathf.Max(cosine, Mathf.Epsilon), MaxMiterFactor);
        }

        private static Vector3 FlatDirection(Vector3 from, Vector3 to)
        {
            Vector3 direction = to - from;
            direction.y = 0f;
            return direction.normalized;
        }
    }
}
