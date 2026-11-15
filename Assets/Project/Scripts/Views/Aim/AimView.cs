using SelfBulletting.Gameplay.Presentation;
using UnityEngine;

namespace SelfBulletting.Views.Aim
{
    /// <summary>
    /// Aim preview: the scale in the HUD, the damage zone on the ground and the "Last shot" hint.
    /// </summary>
    public sealed class AimView : MonoBehaviour, IAimView
    {
        [SerializeField] private AimScaleBar _scaleBar;
        [SerializeField] private DamageZoneView _damageZone;
        [SerializeField] private LastShotHint _lastShotHint;

        public void ShowScale()
        {
            _scaleBar.Show();
        }

        public void SetScale(float scale, float limit)
        {
            _scaleBar.SetValue(scale, limit);
        }

        public void SetDamageZoneVisible(bool isVisible)
        {
            if (isVisible)
            {
                _damageZone.Show();
            }
            else
            {
                _damageZone.Hide();
            }
        }

        public void SetDamageZone(Vector3 impactPoint, float damageWorldRadius)
        {
            _damageZone.MoveTo(impactPoint);
            _damageZone.SetRadius(damageWorldRadius);
        }

        public void SetLastShotHint(bool isVisible, bool isAimed)
        {
            _lastShotHint.SetVisible(isVisible, isAimed);
        }

        public void Hide()
        {
            _scaleBar.Hide();
            _damageZone.Hide();
        }
    }
}
