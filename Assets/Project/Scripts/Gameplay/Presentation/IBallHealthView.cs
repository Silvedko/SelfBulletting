namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// HUD indicator of what is left of the player ball. Updated only when a shot is fully resolved
    /// (after the explosions), never while the player is aiming.
    /// </summary>
    public interface IBallHealthView
    {
        /// <param name="radiusPercent">Ball size, % of R0.</param>
        /// <param name="health">What is left to shoot: 1 at full size, 0 at the critical size.</param>
        void Show(float radiusPercent, float health);
    }
}
