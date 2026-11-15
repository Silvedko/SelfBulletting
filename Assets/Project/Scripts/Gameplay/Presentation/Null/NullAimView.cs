using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullAimView : IAimView
    {
        public void ShowScale()
        {
        }

        public void SetScale(float scale, float limit)
        {
        }

        public void SetDamageZoneVisible(bool isVisible)
        {
        }

        public void SetDamageZone(Vector3 impactPoint, float damageWorldRadius)
        {
        }

        public void SetLastShotHint(bool isVisible, bool isAimed)
        {
        }

        public void Hide()
        {
        }
    }
}
