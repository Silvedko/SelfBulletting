using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// Where a drop leaving along an arc will hit: the arc is swept piece by piece, the first fire touched wins,
    /// otherwise the drop lands at the end. The aim preview uses it to show what the stream is about to put out.
    /// </summary>
    public static class ArcProbe
    {
        /// <param name="dropRadius">Drop radius, meters.</param>
        /// <param name="touchRadius">The sphere the drop feels the fires with, meters.</param>
        /// <param name="segments">How many straight pieces the arc is checked in.</param>
        public static DropImpact Find(ArcPath arc, float dropRadius, float touchRadius, int segments, IObstacleSweep sweep)
        {
            int pieces = Mathf.Max(1, segments);
            Vector3 from = arc.Start;
            for (int i = 1; i <= pieces; i++)
            {
                Vector3 to = arc.Evaluate((float)i / pieces);
                if (sweep.TrySweep(from, to, touchRadius, out Vector3 contact, out Collider obstacle))
                {
                    return DropImpact.OnObstacle(contact, dropRadius, obstacle);
                }

                from = to;
            }

            return DropImpact.OnGround(arc.End, dropRadius);
        }
    }
}
