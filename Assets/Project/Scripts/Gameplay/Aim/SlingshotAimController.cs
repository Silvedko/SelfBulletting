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
    /// Slingshot mode. The finger goes down anywhere and starts the ping-pong charge; pulling it back aims the shot the
    /// opposite way, and the pull length sets how far it lands. Releasing fires. Releasing inside the dead zone cancels:
    /// nothing is spent and the next press starts over. The last shot is aimed the same way, without the scale.
    /// </summary>
    public sealed class SlingshotAimController : IShotAiming
    {
        private readonly IInputService _input;
        private readonly IScreenToGround _screen;
        private readonly SlingshotPull _pull;
        private readonly ShotEconomy _economy;
        private readonly AimScale _scale;
        private readonly BallModel _ball;
        private readonly LobPreview _preview;
        private readonly GameViews _views;
        private readonly IAudioService _audio;

        public SlingshotAimController(IInputService input, IScreenToGround screen, SlingshotPull pull, ShotEconomy economy,
            AimScale scale, BallModel ball, LobPreview preview, GameViews views, IAudioService audio)
        {
            _input = input;
            _screen = screen;
            _pull = pull;
            _economy = economy;
            _scale = scale;
            _ball = ball;
            _preview = preview;
            _views = views;
            _audio = audio;
        }

        public async UniTask<FiredShot> TakeShotAsync(ShotMode mode, RaycastHit blocker, CancellationToken cancellationToken)
        {
            bool isInstant = mode == ShotMode.Instant;
            if (isInstant)
            {
                _audio.Play(SoundId.LastShot);
            }

            try
            {
                while (true)
                {
                    // The hint hides while the finger aims, so it does not cover the field, and comes back after a cancel.
                    _views.Aim.SetLastShotHint(isInstant, isAimed: true);
                    await UniTask.WaitUntil(() => _input.WasPressedThisFrame, cancellationToken: cancellationToken);
                    _views.Aim.SetLastShotHint(false, isAimed: true);

                    FiredShot? shot = await HoldAsync(isInstant, cancellationToken);
                    if (shot.HasValue)
                    {
                        return shot.Value;
                    }
                }
            }
            finally
            {
                _views.Aim.SetLastShotHint(false, isAimed: true);
            }
        }

        /// <returns>The shot, or nothing when the finger was released inside the dead zone.</returns>
        private async UniTask<FiredShot?> HoldAsync(bool isInstant, CancellationToken cancellationToken)
        {
            Vector2 pressPoint = _input.Position;
            float limit = _economy.GetScaleLimit(_ball.RadiusPercent);
            float elapsedSeconds = 0f;
            Vector3 direction = Vector3.forward;
            FiredShot? shot = null;

            if (!isInstant)
            {
                _views.Aim.ShowScale();
                _audio.StartLoop(SoundId.AimCharge);
            }

            try
            {
                while (_input.IsPressed)
                {
                    ShotRequest request = CreateRequest(isInstant, elapsedSeconds, limit);
                    if (_pull.TryAim(_screen, pressPoint, _input.Position, ref direction, out float rangeShare))
                    {
                        shot = _preview.Show(request, direction, rangeShare);
                    }
                    else
                    {
                        shot = null;
                        _preview.ShowWithoutTarget(request);
                    }

                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    elapsedSeconds += Time.deltaTime;
                }
            }
            finally
            {
                _audio.StopLoop(SoundId.AimCharge);
                _preview.Hide();
                _views.Aim.Hide();
            }

            if (!shot.HasValue)
            {
                _views.Shot.Hide();
            }

            return shot;
        }

        private ShotRequest CreateRequest(bool isInstant, float elapsedSeconds, float limit)
        {
            if (isInstant)
            {
                return _economy.CreateInstantShot(_ball.RadiusPercent);
            }

            ShotRequest request = _economy.CreateAimedShot(_ball.RadiusPercent, _scale.Evaluate(elapsedSeconds, limit));
            _views.Aim.SetScale(request.Scale, limit);
            return request;
        }
    }
}
