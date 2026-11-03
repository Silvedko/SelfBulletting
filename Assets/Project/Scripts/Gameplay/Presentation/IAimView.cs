using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Aim preview: the scale in the HUD and the damage ring on the ground around the impact point.
    /// </summary>
    public interface IAimView
    {
        void Show();

        /// <param name="scale">Current value, 0..1.</param>
        /// <param name="limit">Upper bound the ball can afford, 0..1. The part above it is shown dimmed.</param>
        void SetScale(float scale, float limit);

        /// <summary>
        /// The ring on the ground. Both values change every frame while aiming: the radius follows the scale,
        /// and the impact point moves because the shot grows and touches the obstacles at another spot.
        /// </summary>
        void SetDamageZone(Vector3 impactPoint, float damageWorldRadius);

        /// <summary>"Last shot" hint while the next tap fires the last shot without the scale.</summary>
        void SetLastShotHint(bool isVisible);

        void Hide();
    }
}
