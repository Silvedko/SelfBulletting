using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Aim preview: the scale in the HUD and the damage ring on the ground around the impact point.
    /// </summary>
    public interface IAimView
    {
        void Show(Vector3 impactPoint);

        /// <param name="scale">Current value, 0..1.</param>
        /// <param name="limit">Upper bound the ball can afford, 0..1. The part above it is shown dimmed.</param>
        /// <param name="damageWorldRadius">Radius of the ring on the ground, meters.</param>
        void SetScale(float scale, float limit, float damageWorldRadius);

        /// <summary>"Last shot" hint while the next tap fires the last shot without the scale.</summary>
        void SetLastShotHint(bool isVisible);

        void Hide();
    }
}
