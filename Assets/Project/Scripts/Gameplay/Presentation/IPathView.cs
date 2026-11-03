using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// The pink strip on the ground from the ball to the finish. Its width is the ball diameter.
    /// </summary>
    public interface IPathView
    {
        void SetPath(Vector3 fromGroundPoint, Vector3 toGroundPoint);

        /// <summary>Ball radius (current or preview); the strip is twice as wide.</summary>
        void SetWidth(float ballWorldRadius);

        /// <summary>0 — calm, 1 — the preview is at the critical size. The strip pulses with the ball.</summary>
        void SetDanger(float level);
    }
}
