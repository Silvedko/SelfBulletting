using System;
using System.Collections.Generic;
using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Scanning
{
    /// <summary>
    /// Finds obstacle colliders inside the damage radius, nearest first, so the infection spreads as a wave.
    /// </summary>
    public sealed class ObstacleScanner
    {
        private readonly Collider[] _colliders;
        private readonly int _obstacleMask;
        private readonly Comparison<Collider> _byDistanceToCenter;

        private Vector3 _center;

        public ObstacleScanner(PhysicsSettings settings)
        {
            _colliders = new Collider[settings.QueryBufferSize];
            _obstacleMask = settings.ObstacleMask;
            _byDistanceToCenter = CompareByDistanceToCenter;
        }

        public int FindInRadius(Vector3 center, float radius, List<Collider> results)
        {
            results.Clear();

            int count = Physics.OverlapSphereNonAlloc(center, radius, _colliders, _obstacleMask, QueryTriggerInteraction.Ignore);
            if (count >= _colliders.Length)
            {
                Debug.LogWarning($"[{nameof(ObstacleScanner)}] {count} colliders filled the buffer. Increase the query buffer size.");
            }

            for (int i = 0; i < count; i++)
            {
                results.Add(_colliders[i]);
            }

            _center = center;
            results.Sort(_byDistanceToCenter);
            return count;
        }

        private int CompareByDistanceToCenter(Collider left, Collider right)
        {
            float leftDistance = (left.transform.position - _center).sqrMagnitude;
            float rightDistance = (right.transform.position - _center).sqrMagnitude;
            return leftDistance.CompareTo(rightDistance);
        }
    }
}
