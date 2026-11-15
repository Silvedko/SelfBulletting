using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Aim preview of the aim modes on top of <see cref="IAimView"/>: the dotted arc of the lobbed shot and the arrow
    /// of the Arrow mode.
    /// </summary>
    public interface ILobAimView
    {
        /// <summary>The arrow lies on the ground and starts at the edge of the ball.</summary>
        /// <param name="direction">Flat unit direction.</param>
        void ShowArrow(Vector3 ballGroundPoint, float ballRadius, Vector3 direction);

        void HideArrow();

        /// <summary>Called every frame while aiming: the arc follows the aim and the shot size.</summary>
        void ShowTrajectory(ArcPath arc);

        void HideTrajectory();
    }
}
