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
    /// landing point runs along the arrow back and forth; the second tap fires there. The size follows the range:
    /// the biggest affordable shot at the shortest range, the minimum shot at the longest, so a far target is cheap but
    /// weak. The last shot keeps its size (everything left), only the range is chosen.
    /// </summary>
    public sealed class ArrowAimController : IShotAiming
    {
        private readonly IInputService _input;
        private readonly ArrowSwing _swing;
        private readonly ShotEconomy _economy;
        private readonly BallModel _ball;
        private readonly LobPreview _preview;
        private readonly Vector3 _ballGroundPoint;
        private readonly GameViews _views;
        private readonly IAudioService _audio;

        public ArrowAimController(IInputService input, ArrowSwing swing, ShotEconomy economy, BallModel ball,
            LobPreview preview, Vector3 ballGroundPoint, GameViews views, IAudioService audio)
        {
            _input = input;
            _swing = swing;
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
            float limit = _economy.GetScaleLimit(_ball.RadiusPercent);
            float elapsedSeconds = 0f;
            FiredShot shot = ShowPreview(direction, isInstant, elapsedSeconds, limit);

            if (!isInstant)
            {
                _views.Aim.ShowScale();
                _audio.StartLoop(SoundId.AimCharge);
            }

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

                    shot = ShowPreview(direction, isInstant, elapsedSeconds, limit);
                }
            }
            finally
            {
                _audio.StopLoop(SoundId.AimCharge);
                _preview.Hide();
                _views.Aim.Hide();
            }
        }

        private FiredShot ShowPreview(Vector3 direction, bool isInstant, float elapsedSeconds, float limit)
        {
            _views.LobAim.ShowArrow(_ballGroundPoint, _ball.WorldRadius, direction);
            float rangeShare = _swing.GetRangeShare(elapsedSeconds);
            if (isInstant)
            {
                return _preview.Show(_economy.CreateInstantShot(_ball.RadiusPercent), direction, rangeShare);
            }

            // Near ranges ask for more than a small ball can afford; the economy cuts them to the biggest affordable shot.
            ShotRequest request = _economy.CreateAimedShot(_ball.RadiusPercent, ArrowSwing.GetSizeScale(rangeShare));
            _views.Aim.SetScale(request.Scale, limit);
            return _preview.Show(request, direction, rangeShare);
        }
    }
}
