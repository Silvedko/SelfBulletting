using System.Collections.Generic;
using SelfBulletting.Gameplay.Paths;
using UnityEngine;

namespace SelfBulletting.Gameplay.Finish
{
    /// <summary>
    /// Splits the path to the finish into equal hops along it, none longer than the hop length,
    /// so on a curved path the ball follows the curve.
    /// </summary>
    public static class HopPath
    {
        public static IReadOnlyList<Vector3> Build(GroundPath path, float hopLength)
        {
            var points = new List<Vector3>();
            if (path.Length <= Mathf.Epsilon)
            {
                return points;
            }

            int hopCount = Mathf.CeilToInt(path.Length / hopLength);
            for (int hop = 1; hop <= hopCount; hop++)
            {
                points.Add(path.GetPoint(path.Length * hop / hopCount));
            }

            return points;
        }
    }
}
