using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Paths;
using UnityEngine;

namespace SelfBulletting.Gameplay.Scanning
{
    /// <summary>
    /// Sweeps a sphere of the ball size along the path to the finish, segment by segment, at the height of the ball centre.
    /// The first obstacle along the path is what blocks the ball; in Classic the next shot also flies into it.
    /// The same sweep moves the Hose drops along their arcs.
    /// </summary>
    public sealed class PathScanner : IObstacleSweep
    {
        private readonly RaycastHit[] _hits;
        private readonly int _obstacleMask;

        public PathScanner(PhysicsSettings settings)
        {
            _hits = new RaycastHit[settings.QueryBufferSize];
            _obstacleMask = settings.ObstacleMask;
        }

        public bool IsClear(Vector3 from, Vector3 to, float radius) => !TryFindFirstObstacle(from, to, radius, out _);

        public bool IsClear(GroundPath path, float radius) => !TryFindFirstObstacle(path, radius, out _);

        /// <summary>The first obstacle along the path: segments are swept in order, the nearest hit of the first blocked one wins.</summary>
        /// <param name="radius">Ball radius; the sphere is lifted by it, so it touches the ground like the ball does.</param>
        public bool TryFindFirstObstacle(GroundPath path, float radius, out RaycastHit firstHit)
        {
            Vector3 lift = Vector3.up * radius;
            for (int i = 1; i < path.Points.Count; i++)
            {
                if (TryFindFirstObstacle(path.Points[i - 1] + lift, path.Points[i] + lift, radius, out firstHit))
                {
                    return true;
                }
            }

            firstHit = default;
            return false;
        }

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

        public bool TrySweep(Vector3 from, Vector3 to, float radius, out Vector3 contactCenter, out Collider obstacle)
        {
            if (!TryFindFirstObstacle(from, to, radius, out RaycastHit hit))
            {
                contactCenter = to;
                obstacle = null;
                return false;
            }

            contactCenter = from + (to - from).normalized * hit.distance;
            obstacle = hit.collider;
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
