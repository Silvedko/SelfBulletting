using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullLobAimView : ILobAimView
    {
        public void ShowArrow(Vector3 ballGroundPoint, float ballRadius, Vector3 direction)
        {
        }

        public void HideArrow()
        {
        }

        public void ShowTrajectory(ArcPath arc)
        {
        }

        public void HideTrajectory()
        {
        }
    }
}
