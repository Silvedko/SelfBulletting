using System.Collections.Generic;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Obstacles;
using SelfBulletting.Gameplay.Scanning;
using UnityEngine;

namespace SelfBulletting.Gameplay.Aim
{
    /// <summary>
    /// Highlights the obstacles inside the damage ring of the shot being aimed. Only the obstacles that enter or leave
    /// the ring are told, so a steady aim costs one overlap query per frame and nothing else.
    /// </summary>
    public sealed class TargetHighlight
    {
        private readonly ObstacleScanner _scanner;
        private readonly Level _level;
        private readonly List<Collider> _colliders = new List<Collider>();
        private readonly HashSet<Obstacle> _shown = new HashSet<Obstacle>();
        private readonly HashSet<Obstacle> _next = new HashSet<Obstacle>();
        private readonly List<Obstacle> _left = new List<Obstacle>();

        public TargetHighlight(ObstacleScanner scanner, Level level)
        {
            _scanner = scanner;
            _level = level;
        }

        public void Show(Vector3 impactPoint, float damageRadius)
        {
            _next.Clear();
            _scanner.FindInRadius(impactPoint, damageRadius, _colliders);
            foreach (Collider collider in _colliders)
            {
                if (_level.TryGetObstacle(collider, out Obstacle obstacle) && !obstacle.IsDestroyed)
                {
                    _next.Add(obstacle);
                }
            }

            _left.Clear();
            foreach (Obstacle obstacle in _shown)
            {
                if (!_next.Contains(obstacle))
                {
                    _left.Add(obstacle);
                }
            }

            foreach (Obstacle obstacle in _left)
            {
                obstacle.SetHighlighted(false);
                _shown.Remove(obstacle);
            }

            foreach (Obstacle obstacle in _next)
            {
                if (_shown.Add(obstacle))
                {
                    obstacle.SetHighlighted(true);
                }
            }
        }

        public void Clear()
        {
            foreach (Obstacle obstacle in _shown)
            {
                obstacle.SetHighlighted(false);
            }

            _shown.Clear();
        }
    }
}
