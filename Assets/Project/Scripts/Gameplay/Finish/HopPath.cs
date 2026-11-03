using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Gameplay.Finish
{
    /// <summary>
    /// Splits the straight path to the finish into equal hops no longer than the hop length.
    /// </summary>
    public static class HopPath
    {
        public static IReadOnlyList<Vector3> Build(Vector3 from, Vector3 to, float hopLength)
        {
            var points = new List<Vector3>();
            float distance = Vector3.Distance(from, to);
            if (distance <= Mathf.Epsilon)
            {
                return points;
            }

            int hopCount = Mathf.CeilToInt(distance / hopLength);
            for (int hop = 1; hop <= hopCount; hop++)
            {
                points.Add(Vector3.Lerp(from, to, (float)hop / hopCount));
            }

            return points;
        }
    }
}
