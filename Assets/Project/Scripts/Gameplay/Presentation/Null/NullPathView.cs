using System.Collections.Generic;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullPathView : IPathView
    {
        public void SetPath(IReadOnlyList<Vector3> groundPoints)
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
