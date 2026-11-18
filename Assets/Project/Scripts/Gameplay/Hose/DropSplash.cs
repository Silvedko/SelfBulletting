using System;
using System.Collections.Generic;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Obstacles;
using SelfBulletting.Gameplay.Scanning;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// A drop splashes where it hits: the fire it touched and every fire within the splash radius take one hit each,
    /// all at once. No wave and no chain: a fire that goes out does not spread anything to its neighbours.
    /// </summary>
    public sealed class DropSplash
    {
        private readonly ObstacleScanner _scanner;
        private readonly Level _level;
        private readonly DropSize _size;
        private readonly HitCounter<Obstacle> _hits;
        private readonly List<Collider> _colliders = new List<Collider>();
        private readonly List<Obstacle> _targets = new List<Obstacle>();

        public DropSplash(ObstacleScanner scanner, Level level, DropSize size, HitCounter<Obstacle> hits)
        {
            _scanner = scanner;
            _level = level;
            _size = size;
            _hits = hits;
        }

        /// <param name="onExtinguished">
        /// Called at once for every fire the splash puts out. It has to start the fire going out right there, so the next
        /// drop of the same frame does not hit it again.
        /// </param>
        public void Apply(DropImpact impact, Action<Obstacle> onExtinguished)
        {
            CollectTargets(impact);
            foreach (Obstacle fire in _targets)
            {
                float doused = _hits.AddHit(fire);
                if (doused >= 1f)
                {
                    onExtinguished(fire);
                }
                else
                {
                    fire.Wet(doused);
                }
            }
        }

        private void CollectTargets(DropImpact impact)
        {
            _targets.Clear();
            AddTarget(impact.Touched);

            _scanner.FindInRadius(impact.Center, _size.GetSplashRadius(impact.DropRadius), _colliders);
            foreach (Collider collider in _colliders)
            {
                AddTarget(collider);
            }
        }

        private void AddTarget(Collider collider)
        {
            if (_level.TryGetObstacle(collider, out Obstacle obstacle) && !obstacle.IsDestroyed && !_targets.Contains(obstacle))
            {
                _targets.Add(obstacle);
            }
        }
    }
}
