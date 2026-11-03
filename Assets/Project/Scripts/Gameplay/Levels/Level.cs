using System.Collections.Generic;
using SelfBulletting.Gameplay.Obstacles;
using UnityEngine;

namespace SelfBulletting.Gameplay.Levels
{
    /// <summary>
    /// Layout of the level in the Game scene: start (bottom-left), finish (top-right) and the obstacles between them.
    /// Obstacles are placed by the editor generator under <see cref="ObstaclesRoot"/>.
    /// </summary>
    public sealed class Level : MonoBehaviour
    {
        [SerializeField] private Transform _start;
        [SerializeField] private Transform _finish;
        [SerializeField] private Transform _obstaclesRoot;

        private readonly Dictionary<Collider, Obstacle> _obstacles = new Dictionary<Collider, Obstacle>();

        /// <summary>Ground point under the ball at start.</summary>
        public Vector3 StartPoint => _start.position;

        /// <summary>Ground point in front of the door.</summary>
        public Vector3 FinishPoint => _finish.position;

        public Transform ObstaclesRoot => _obstaclesRoot;

        public void Initialize()
        {
            _obstacles.Clear();
            foreach (Obstacle obstacle in _obstaclesRoot.GetComponentsInChildren<Obstacle>())
            {
                _obstacles[obstacle.Collider] = obstacle;
            }
        }

        public bool TryGetObstacle(Collider collider, out Obstacle obstacle) => _obstacles.TryGetValue(collider, out obstacle);
    }
}
