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
    /// Turns the player's tap into a shot: holds the ping-pong scale while the finger is down and keeps every preview in sync.
    /// </summary>
    public sealed class AimController
    {
        private readonly IInputService _input;
        private readonly ShotEconomy _economy;
        private readonly AimScale _scale;
        private readonly BallModel _ball;
        private readonly DangerFeedback _danger;
        private readonly GameViews _views;
        private readonly IAudioService _audio;

        public AimController(IInputService input, ShotEconomy economy, AimScale scale, BallModel ball,
            DangerFeedback danger, GameViews views, IAudioService audio)
        {
            _input = input;
            _economy = economy;
            _scale = scale;
            _ball = ball;
            _danger = danger;
            _views = views;
            _audio = audio;
        }

        public async UniTask<ShotRequest> TakeShotAsync(ShotMode mode, Vector3 ballCenter, Vector3 impactPoint,
            CancellationToken cancellationToken)
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
                return _economy.CreateInstantShot(_ball.RadiusPercent);
            }

            float scale = await HoldAsync(ballCenter, impactPoint, cancellationToken);
            return _economy.CreateAimedShot(_ball.RadiusPercent, scale);
        }

        private async UniTask<float> HoldAsync(Vector3 ballCenter, Vector3 impactPoint, CancellationToken cancellationToken)
        {
            float limit = _economy.GetScaleLimit(_ball.RadiusPercent);
            float elapsedSeconds = 0f;
            float scale = 0f;

            _views.Aim.Show(impactPoint);
            _views.Shot.ShowCharging(ballCenter, _ball.WorldRadius, impactPoint, _economy.ShotWorldRadius);
            _audio.StartLoop(SoundId.AimCharge);

            try
            {
                while (_input.IsPressed)
                {
                    scale = _scale.Evaluate(elapsedSeconds, limit);
                    ShowPreview(scale, limit);
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

            return scale;
        }

        private void ShowPreview(float scale, float limit)
        {
            float previewPercent = _economy.GetPreviewRadiusPercent(_ball.RadiusPercent, scale);
            float previewWorldRadius = _economy.ToWorldRadius(previewPercent);
            float danger = _danger.Tick(previewPercent, Time.deltaTime);

            _views.Ball.SetRadius(previewWorldRadius);
            _views.Ball.SetDanger(danger);
            _views.Path.SetWidth(previewWorldRadius);
            _views.Path.SetDanger(danger);
            _views.Aim.SetScale(scale, limit, _economy.GetDamageWorldRadius(scale));
        }
    }
}
