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
    /// Classic mode: turns the player's tap into a shot that rolls along the path. While the finger is down the
    /// ping-pong scale sets the charge: the ball shrinks, the shot grows by the same amount, and every frame the touch
    /// point and the damage ring are found again for the new shot size.
    /// </summary>
    public sealed class ClassicAimController : IShotAiming
    {
        private readonly IInputService _input;
        private readonly ShotEconomy _economy;
        private readonly AimScale _scale;
        private readonly ShotTargeting _targeting;
        private readonly BallModel _ball;
        private readonly ChargePreview _charge;
        private readonly GameViews _views;
        private readonly IAudioService _audio;

        public ClassicAimController(IInputService input, ShotEconomy economy, AimScale scale, ShotTargeting targeting,
            BallModel ball, ChargePreview charge, GameViews views, IAudioService audio)
        {
            _input = input;
            _economy = economy;
            _scale = scale;
            _targeting = targeting;
            _ball = ball;
            _charge = charge;
            _views = views;
            _audio = audio;
        }

        public async UniTask<FiredShot> TakeShotAsync(ShotMode mode, RaycastHit blocker, CancellationToken cancellationToken)
        {
            bool isInstant = mode == ShotMode.Instant;
            _views.Aim.SetLastShotHint(isInstant, isAimed: false);
            if (isInstant)
            {
                _audio.Play(SoundId.LastShot);
            }

            await UniTask.WaitUntil(() => _input.WasPressedThisFrame, cancellationToken: cancellationToken);
            _views.Aim.SetLastShotHint(false, isAimed: false);

            if (isInstant)
            {
                return Charge(_economy.CreateInstantShot(_ball.RadiusPercent), blocker);
            }

            return await HoldAsync(blocker, cancellationToken);
        }

        private async UniTask<FiredShot> HoldAsync(RaycastHit blocker, CancellationToken cancellationToken)
        {
            float limit = _economy.GetScaleLimit(_ball.RadiusPercent);
            float elapsedSeconds = 0f;

            _views.Aim.ShowScale();
            _views.Aim.SetDamageZoneVisible(true);
            _audio.StartLoop(SoundId.AimCharge);
            FiredShot shot = ShowPreview(0f, limit, blocker);

            try
            {
                while (_input.IsPressed)
                {
                    shot = ShowPreview(_scale.Evaluate(elapsedSeconds, limit), limit, blocker);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    elapsedSeconds += Time.deltaTime;
                }
            }
            finally
            {
                _audio.StopLoop(SoundId.AimCharge);
                _charge.Reset();
                _views.Aim.Hide();
            }

            return shot;
        }

        private FiredShot ShowPreview(float scale, float limit, RaycastHit blocker)
        {
            ShotRequest request = _economy.CreateAimedShot(_ball.RadiusPercent, scale);
            _charge.Show(request);

            FiredShot shot = Charge(request, blocker);
            _views.Aim.SetScale(request.Scale, limit);
            _views.Aim.SetDamageZone(shot.Target.ImpactPoint, request.DamageWorldRadius);
            return shot;
        }

        private FiredShot Charge(ShotRequest request, RaycastHit blocker)
        {
            float ballRadiusAfterShot = _economy.ToWorldRadius(_ball.RadiusPercent - request.CostPercent);
            float shotRadius = _economy.GetShotWorldRadius(request.CostPercent);
            ShotTarget target = _targeting.Find(ballRadiusAfterShot, shotRadius, blocker);

            _views.Shot.ShowCharging(target.LaunchCenter, shotRadius);
            return new FiredShot(request, target);
        }
    }
}
