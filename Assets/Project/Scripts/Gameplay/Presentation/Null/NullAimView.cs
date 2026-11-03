using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullAimView : IAimView
    {
        public void Show()
        {
        }

        public void SetScale(float scale, float limit)
        {
        }

        public void SetDamageZone(Vector3 impactPoint, float damageWorldRadius)
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
