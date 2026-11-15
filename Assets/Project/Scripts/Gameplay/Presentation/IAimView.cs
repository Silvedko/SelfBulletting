using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Aim preview shared by every mode: the scale in the HUD, the damage ring on the ground around the impact point
    /// and the "Last shot" hint.
    /// </summary>
    public interface IAimView
    {
        void ShowScale();

        /// <param name="scale">Current value, 0..1.</param>
        /// <param name="limit">Upper bound the ball can afford, 0..1. The part above it is shown dimmed.</param>
        void SetScale(float scale, float limit);

        void SetDamageZoneVisible(bool isVisible);

        /// <summary>
        /// The ring on the ground. Both values change every frame while aiming: the radius follows the scale,
        /// and the impact point moves with the aim or because the shot grows and touches the obstacles at another spot.
        /// </summary>
        void SetDamageZone(Vector3 impactPoint, float damageWorldRadius);

        /// <summary>"Last shot" hint while the next shot takes everything above the critical size without the scale.</summary>
        /// <param name="isAimed">The last shot is still aimed (the aim modes), so the hint says "aim and fire", not "tap to fire".</param>
        void SetLastShotHint(bool isVisible, bool isAimed);

        /// <summary>Hides the scale and the ring.</summary>
        void Hide();
    }
}
