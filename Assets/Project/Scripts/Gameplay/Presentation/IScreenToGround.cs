using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Maps a screen point to the ground under it through the game camera, so the slingshot pull can be turned into a
    /// direction on the ground.
    /// </summary>
    public interface IScreenToGround
    {
        /// <summary>Screen height, pixels: slingshot pulls are measured as a share of it.</summary>
        float ScreenHeight { get; }

        /// <returns>False when the ray from the camera does not reach the ground.</returns>
        bool TryGetGroundPoint(Vector2 screenPoint, out Vector3 groundPoint);
    }
}
