using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Feedback;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Shot;
using SelfBulletting.Infrastructure.Services.Audio;
using SelfBulletting.Infrastructure.Services.Input;
using UnityEngine;

namespace SelfBulletting.Gameplay.Aim
{
    /// <summary>
    /// Turns the player's tap into a shot. While the finger is down the ping-pong scale sets the charge: the ball shrinks,
    /// the shot grows by the same amount, and every frame the touch point and the damage ring are found again for the new
    /// shot size.
    /// </summary>
    public sealed class AimController
    {
        private readonly IInputService _input;
        private readonly ShotEconomy _economy;
        private readonly AimScale _scale;
        private readonly ShotTargeting _targeting;
        private readonly BallModel _ball;
        private readonly DangerFeedback _danger;
        private readonly GameViews _views;
        private readonly IAudioService _audio;

        public AimController(IInputService input, ShotEconomy economy, AimScale scale, ShotTargeting targeting,
            BallModel ball, DangerFeedback danger, GameViews views, IAudioService audio)
        {
            _input = input;
            _economy = economy;
            _scale = scale;
            _targeting = targeting;
            _ball = ball;
            _danger = danger;
            _views = views;
            _audio = audio;
        }

        /// <param name="blocker">The first obstacle in the ball's corridor: the shot rolls towards it.</param>
        public async UniTask<FiredShot> TakeShotAsync(ShotMode mode, RaycastHit blocker, CancellationToken cancellationToken)
        {
            bool isInstant = mode == ShotMode.Instant;
            _views.Aim.SetLastShotHint(isInstant);
            if (isInstant)
            {
                _audio.Play(SoundId.LastShot);
            }

            await UniTask.WaitUntil(() => _input.WasPressedThisFrame, cancellationToken: cancellationToken);
            _views.Aim.SetLastShotHint(false);

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

            _views.Aim.Show();
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
                _danger.Reset();
                _views.Ball.SetDanger(0f);
                _views.Path.SetDanger(0f);
                _views.Aim.Hide();
            }

            return shot;
        }

        private FiredShot ShowPreview(float scale, float limit, RaycastHit blocker)
        {
            ShotRequest request = _economy.CreateAimedShot(_ball.RadiusPercent, scale);
            float previewPercent = _ball.RadiusPercent - request.CostPercent;
            float previewWorldRadius = _economy.ToWorldRadius(previewPercent);
            float danger = _danger.Tick(previewPercent, Time.deltaTime);

            _views.Ball.SetRadius(previewWorldRadius);
            _views.Ball.SetDanger(danger);
            _views.Path.SetWidth(previewWorldRadius);
            _views.Path.SetDanger(danger);

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
