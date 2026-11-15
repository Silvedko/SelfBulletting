using System.Collections.Generic;
using SelfBulletting.Gameplay.Obstacles;
using SelfBulletting.Gameplay.Paths;
using UnityEngine;

namespace SelfBulletting.Gameplay.Levels
{
    /// <summary>
    /// Layout of the level in the Game scene: start (bottom-left), finish (top-right), the obstacles between them and,
    /// for the curved levels of the aim modes, the waypoints of the path. Obstacles and waypoints are placed by the
    /// editor generator under <see cref="ObstaclesRoot"/> and the path root.
    /// </summary>
    public sealed class Level : MonoBehaviour
    {
        /// <summary>Distance between the points of a curved path, meters.</summary>
        public const float CurveSpacing = 0.5f;

        [SerializeField] private Transform _start;
        [SerializeField] private Transform _finish;
        [SerializeField] private Transform _obstaclesRoot;

        [Tooltip("Optional. Its children, in order, are the waypoints of the curved path between the start and the finish.")]
        [SerializeField] private Transform _pathRoot;

        [Tooltip("The playing field on the ground: x/width is the world X, y/height is the world Z. Lobbed shots land inside it.")]
        [SerializeField] private Rect _field = new Rect(0f, 0f, 12f, 24f);

        private readonly Dictionary<Collider, Obstacle> _obstacles = new Dictionary<Collider, Obstacle>();

        /// <summary>Ground point under the ball at start.</summary>
        public Vector3 StartPoint => _start.position;

        /// <summary>Ground point in front of the door.</summary>
        public Vector3 FinishPoint => _finish.position;

        public Transform ObstaclesRoot => _obstaclesRoot;
        public Transform PathRoot => _pathRoot;
        public Rect Field => _field;

        public void Initialize()
        {
            _obstacles.Clear();
            foreach (Obstacle obstacle in _obstaclesRoot.GetComponentsInChildren<Obstacle>())
            {
                _obstacles[obstacle.Collider] = obstacle;
            }
        }

        public bool TryGetObstacle(Collider collider, out Obstacle obstacle)
        {
            if (collider == null)
            {
                obstacle = null;
                return false;
            }

            return _obstacles.TryGetValue(collider, out obstacle);
        }

        /// <summary>The smooth path through the waypoints; straight when the level has none.</summary>
        public GroundPath CreateCurvedPath() => GroundPath.Smooth(GetControlPoints(), CurveSpacing);

        public GroundPath CreateStraightPath() => GroundPath.Straight(StartPoint, FinishPoint);

        /// <summary>Start, every waypoint in order, finish.</summary>
        public List<Vector3> GetControlPoints()
        {
            var points = new List<Vector3> { StartPoint };
            if (_pathRoot != null)
            {
                foreach (Transform waypoint in _pathRoot)
                {
                    points.Add(waypoint.position);
                }
            }

            points.Add(FinishPoint);
            return points;
        }

        private void OnDrawGizmos()
        {
            if (_start == null || _finish == null)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            Vector3 fieldCenter = new Vector3(_field.center.x, 0f, _field.center.y);
            Gizmos.DrawWireCube(fieldCenter, new Vector3(_field.width, 0f, _field.height));

            if (_pathRoot == null || _pathRoot.childCount == 0)
            {
                return;
            }

            Gizmos.color = Color.cyan;
            IReadOnlyList<Vector3> points = CreateCurvedPath().Points;
            for (int i = 1; i < points.Count; i++)
            {
                Gizmos.DrawLine(points[i - 1], points[i]);
            }

            foreach (Transform waypoint in _pathRoot)
            {
                Gizmos.DrawWireSphere(waypoint.position, 0.3f);
            }
        }
    }
}
