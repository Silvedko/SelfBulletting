using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullScreenToGround : IScreenToGround
    {
        public float ScreenHeight => 0f;

        public bool TryGetGroundPoint(Vector2 screenPoint, out Vector3 groundPoint)
        {
            groundPoint = default;
            return false;
        }
    }
}
