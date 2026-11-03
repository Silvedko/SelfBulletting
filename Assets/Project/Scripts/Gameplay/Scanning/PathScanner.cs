using SelfBulletting.Configs;
using UnityEngine;

namespace SelfBulletting.Gameplay.Scanning
{
    /// <summary>
    /// Sweeps a sphere of the ball size along the straight path to the finish.
    /// The nearest obstacle is both what blocks the ball and what the next shot flies into.
    /// </summary>
    public sealed class PathScanner
    {
        private readonly RaycastHit[] _hits;
        private readonly int _obstacleMask;

        public PathScanner(PhysicsSettings settings)
        {
            _hits = new RaycastHit[settings.QueryBufferSize];
            _obstacleMask = settings.ObstacleMask;
        }

        public bool IsClear(Vector3 from, Vector3 to, float radius) => !TryFindFirstObstacle(from, to, radius, out _);

        public bool TryFindFirstObstacle(Vector3 from, Vector3 to, float radius, out RaycastHit firstHit)
        {
            firstHit = default;
            Vector3 offset = to - from;
            float distance = offset.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return false;
            }

            int count = Physics.SphereCastNonAlloc(from, radius, offset / distance, _hits, distance, _obstacleMask, QueryTriggerInteraction.Ignore);
            WarnIfBufferIsFull(count);

            // SphereCastNonAlloc does not sort its results.
            int nearest = -1;
            for (int i = 0; i < count; i++)
            {
                if (nearest < 0 || _hits[i].distance < _hits[nearest].distance)
                {
                    nearest = i;
                }
            }

            if (nearest < 0)
            {
                return false;
            }

            firstHit = _hits[nearest];

            // A collider that already overlaps the sphere at the start is reported with distance 0 and point zero.
            if (firstHit.distance <= 0f)
            {
                firstHit.point = firstHit.collider.ClosestPoint(from);
            }

            return true;
        }

        private void WarnIfBufferIsFull(int count)
        {
            if (count >= _hits.Length)
            {
                Debug.LogWarning($"[{nameof(PathScanner)}] {count} hits filled the buffer; the nearest one may be missed. Increase the query buffer size.");
            }
        }
    }
}
