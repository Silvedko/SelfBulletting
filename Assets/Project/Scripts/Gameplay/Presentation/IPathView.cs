using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// The strip on the ground from the ball to the finish. It follows the path points and is as wide as the ball diameter.
    /// </summary>
    public interface IPathView
    {
        /// <param name="groundPoints">The path as a polyline, from the ball to the finish: two points for a straight path.</param>
        void SetPath(IReadOnlyList<Vector3> groundPoints);

        /// <summary>Ball radius (current or preview); the strip is twice as wide.</summary>
        void SetWidth(float ballWorldRadius);

        /// <summary>0 — calm, 1 — the preview is at the critical size. The strip pulses with the ball.</summary>
        void SetDanger(float level);
    }
}
