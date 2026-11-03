using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullAimView : IAimView
    {
        public void Show(Vector3 impactPoint)
        {
        }

        public void SetScale(float scale, float limit, float damageWorldRadius)
        {
        }

        public void SetLastShotHint(bool isVisible)
        {
        }

        public void Hide()
        {
        }
    }
}
