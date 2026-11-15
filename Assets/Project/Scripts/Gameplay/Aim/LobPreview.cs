using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Aim
{
    /// <summary>
    /// The aim preview of the aim modes for one frame: the charge on the ball, the shot in front of it in the aim
    /// direction, the dotted arc, the damage ring at the landing point and the obstacles the ring covers.
    /// </summary>
    public sealed class LobPreview
    {
        private readonly ShotEconomy _economy;
        private readonly LobTargeting _targeting;
        private readonly ChargePreview _charge;
        private readonly TargetHighlight _highlight;
        private readonly GameViews _views;

        public LobPreview(ShotEconomy economy, LobTargeting targeting, ChargePreview charge, TargetHighlight highlight,
            GameViews views)
        {
            _economy = economy;
            _targeting = targeting;
            _charge = charge;
            _highlight = highlight;
            _views = views;
        }

        /// <param name="direction">Flat unit aim direction.</param>
        /// <param name="rangeShare">0 — the shortest range, 1 — the longest.</param>
        public FiredShot Show(ShotRequest request, Vector3 direction, float rangeShare)
        {
            float ballRadiusAfterShot = _charge.Show(request);
            float shotRadius = _economy.GetShotWorldRadius(request.CostPercent);
            LobTarget target = _targeting.Find(direction, rangeShare, ballRadiusAfterShot, shotRadius);

            _views.Shot.ShowCharging(target.LaunchCenter, shotRadius);
            _views.LobAim.ShowTrajectory(target.Arc);
            _views.Aim.SetDamageZoneVisible(true);
            _views.Aim.SetDamageZone(target.ImpactPoint, request.DamageWorldRadius);
            _highlight.Show(target.ImpactPoint, request.DamageWorldRadius);
            return new FiredShot(request, target);
        }

        /// <summary>The finger is down but there is no aim yet (the slingshot dead zone): only the charge shows.</summary>
        public void ShowWithoutTarget(ShotRequest request)
        {
            _charge.Show(request);
            _views.Shot.Hide();
            HideTarget();
        }

        /// <summary>Ends the preview. The shot itself stays where it is, so a fired shot can fly from there.</summary>
        public void Hide()
        {
            _charge.Reset();
            HideTarget();
        }

        private void HideTarget()
        {
            _views.LobAim.HideTrajectory();
            _views.Aim.SetDamageZoneVisible(false);
            _highlight.Clear();
        }
    }
}
