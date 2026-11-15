using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullLevelView : ILevelView
    {
        public void Present(Vector3 startGroundPoint, Vector3 finishGroundPoint, Vector3 finishDirection, Transform obstaclesRoot)
        {
        }
    }
}
