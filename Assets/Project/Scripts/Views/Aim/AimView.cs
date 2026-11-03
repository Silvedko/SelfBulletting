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

        public void Show(Vector3 impactPoint)
        {
            _scaleBar.Show();
            _damageZone.Show(impactPoint);
        }

        public void SetScale(float scale, float limit, float damageWorldRadius)
        {
            _scaleBar.SetValue(scale, limit);
            _damageZone.SetRadius(damageWorldRadius);
        }

        public void SetLastShotHint(bool isVisible)
        {
            _lastShotHint.SetVisible(isVisible);
        }

        public void Hide()
        {
            _scaleBar.Hide();
            _damageZone.Hide();
        }
    }
}
