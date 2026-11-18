using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Shot;
using SelfBulletting.Infrastructure.Services.Audio;
using SelfBulletting.Infrastructure.Services.Input;
using UnityEngine;

namespace SelfBulletting.Gameplay.Aim
{
    /// <summary>
    /// Arrow mode, two taps. The arrow next to the ball swings by itself; the first tap fixes the direction. Then the
    /// landing point runs along the arrow back and forth; the second tap fires there. There is no power scale: every
    /// shot has the same cost and the same damage radius, only the direction and the range are chosen. The last shot
    /// takes everything left with the minimum radius.
    /// </summary>
    public sealed class ArrowAimController : IShotAiming
    {
        private readonly IInputService _input;
        private readonly ArrowSwing _swing;
        private readonly float _shotCostPercent;
        private readonly float _damageRadiusPercent;
        private readonly ShotEconomy _economy;
        private readonly BallModel _ball;
        private readonly LobPreview _preview;
        private readonly Vector3 _ballGroundPoint;
        private readonly GameViews _views;
        private readonly IAudioService _audio;

        /// <param name="shotCostPercent">What every shot costs, % of R0.</param>
        /// <param name="damageRadiusPercent">Damage radius of every shot, % of R0.</param>
        public ArrowAimController(IInputService input, ArrowSwing swing, float shotCostPercent, float damageRadiusPercent,
            ShotEconomy economy, BallModel ball, LobPreview preview, Vector3 ballGroundPoint, GameViews views,
            IAudioService audio)
        {
            _input = input;
            _swing = swing;
            _shotCostPercent = shotCostPercent;
            _damageRadiusPercent = damageRadiusPercent;
            _economy = economy;
            _ball = ball;
            _preview = preview;
            _ballGroundPoint = ballGroundPoint;
            _views = views;
            _audio = audio;
        }

        public async UniTask<FiredShot> TakeShotAsync(ShotMode mode, RaycastHit blocker, CancellationToken cancellationToken)
        {
            bool isInstant = mode == ShotMode.Instant;
            _views.Aim.SetLastShotHint(isInstant, isAimed: true);
            if (isInstant)
            {
                _audio.Play(SoundId.LastShot);
            }

            try
            {
                Vector3 direction = await SwingUntilTapAsync(cancellationToken);

                // The hint hides while the range is chosen, so it does not cover the field.
                _views.Aim.SetLastShotHint(false, isAimed: true);
                return await ChooseRangeAsync(direction, isInstant, cancellationToken);
            }
            finally
            {
                _views.LobAim.HideArrow();
                _views.Aim.SetLastShotHint(false, isAimed: true);
            }
        }

        private async UniTask<Vector3> SwingUntilTapAsync(CancellationToken cancellationToken)
        {
            float elapsedSeconds = 0f;
            while (true)
            {
                Vector3 direction = _swing.GetDirection(elapsedSeconds);
                _views.LobAim.ShowArrow(_ballGroundPoint, _ball.WorldRadius, direction);
                if (_input.WasPressedThisFrame)
                {
                    return direction;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                elapsedSeconds += Time.deltaTime;
            }
        }

        // The tap that fixed the direction is never read as the second one: the check runs from the next frame on.
        private async UniTask<FiredShot> ChooseRangeAsync(Vector3 direction, bool isInstant, CancellationToken cancellationToken)
        {
            // Cost and radius stay the same while the landing point runs, so the shot is made once.
            ShotRequest request = isInstant
                ? _economy.CreateInstantShot(_ball.RadiusPercent)
                : _economy.CreateFixedShot(_ball.RadiusPercent, _shotCostPercent, _damageRadiusPercent);
            float elapsedSeconds = 0f;
            FiredShot shot = ShowPreview(request, direction, elapsedSeconds);
            _audio.StartLoop(SoundId.AimCharge);

            try
            {
                while (true)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    elapsedSeconds += Time.deltaTime;
                    if (_input.WasPressedThisFrame)
                    {
                        return shot;
                    }

                    shot = ShowPreview(request, direction, elapsedSeconds);
                }
            }
            finally
            {
                _audio.StopLoop(SoundId.AimCharge);
                _preview.Hide();
                _views.Aim.Hide();
            }
        }

        private FiredShot ShowPreview(ShotRequest request, Vector3 direction, float elapsedSeconds)
        {
            _views.LobAim.ShowArrow(_ballGroundPoint, _ball.WorldRadius, direction);
            return _preview.Show(request, direction, _swing.GetRangeShare(elapsedSeconds));
        }
    }
}
