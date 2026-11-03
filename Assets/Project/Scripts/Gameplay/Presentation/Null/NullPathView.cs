using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullPathView : IPathView
    {
        public void SetPath(Vector3 fromGroundPoint, Vector3 toGroundPoint)
        {
        }

        public void SetWidth(float ballWorldRadius)
        {
        }

        public void SetDanger(float level)
        {
        }
    }
}
