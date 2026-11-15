using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Aim;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Finish;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Obstacles;
using SelfBulletting.Gameplay.Paths;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Rules;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Score;
using SelfBulletting.Gameplay.Shot;
using SelfBulletting.Infrastructure.Services.Audio;
using UnityEngine;

namespace SelfBulletting.Gameplay
{
    /// <summary>
    /// One round as a linear async loop: check the path → aim → shoot → infection wave → check again.
    /// The path is straight in Classic and curved in the aim modes; the aiming decides how the shot flies.
    /// </summary>
    public sealed class GameLoop
    {
        private static readonly SoundId[] _comboSounds = { SoundId.Combo, SoundId.ComboBig, SoundId.ComboHuge };

        private readonly Level _level;
        private readonly GroundPath _path;
        private readonly BallModel _ball;
        private readonly ShotEconomy _economy;
        private readonly RoundRules _rules;
        private readonly PathScanner _pathScanner;
        private readonly IShotAiming _aim;
        private readonly ObstacleCascade _cascade;
        private readonly ScoreCounter _score;
        private readonly WinSequence _winSequence;
        private readonly GameViews _views;
        private readonly IAudioService _audio;
        private readonly Action<Obstacle> _onObstacleDestroyed;

        public GameLoop(Level level, GroundPath path, BallModel ball, ShotEconomy economy, RoundRules rules,
            PathScanner pathScanner, IShotAiming aim, ObstacleCascade cascade, ScoreCounter score, WinSequence winSequence, GameViews views,
            IAudioService audio)
        {
            _level = level;
            _path = path;
            _ball = ball;
            _economy = economy;
            _rules = rules;
            _pathScanner = pathScanner;
            _aim = aim;
            _cascade = cascade;
            _score = score;
            _winSequence = winSequence;
            _views = views;
            _audio = audio;
            _onObstacleDestroyed = ShowObstaclePoints;
        }

        public async UniTask<GameResult> RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                bool isBlocked = _pathScanner.TryFindFirstObstacle(_path, _ball.WorldRadius, out RaycastHit firstObstacle);

                switch (_rules.Evaluate(!isBlocked, _ball.RadiusPercent))
                {
                    case RoundOutcome.Win:
                        return await FinishAsync(cancellationToken);

                    case RoundOutcome.Lose:
                        _audio.Play(SoundId.Lose);
                        return GameResult.Lose(_score.Summary);

                    default:
                        await PlayShotAsync(firstObstacle, cancellationToken);
                        break;
                }
            }
        }

        private async UniTask PlayShotAsync(RaycastHit blocker, CancellationToken cancellationToken)
        {
            ShotMode mode = _economy.GetShotMode(_ball.RadiusPercent);
            FiredShot shot = await _aim.TakeShotAsync(mode, blocker, cancellationToken);

            _ball.Spend(shot.Request.CostPercent);
            _audio.Play(SoundId.ShotFire);
            if (shot.IsLob)
            {
                await _views.Shot.LobAsync(shot.Lob.Arc, _economy.ShotSpeed, cancellationToken);
            }
            else
            {
                await _views.Shot.FlyAsync(shot.Target.ContactCenter, _economy.ShotSpeed, cancellationToken);
            }

            _views.Shot.Hide();

            _level.TryGetObstacle(shot.DirectHit, out Obstacle directHit);
            int destroyed = await _cascade.RunAsync(shot.ImpactPoint, shot.Request.DamageWorldRadius, directHit,
                _onObstacleDestroyed, cancellationToken);
            ShowComboIfAny(shot.ImpactPoint, _score.AddShot(destroyed));
            ShowBallHealth();
        }

        // The HUD changes only once the shot is fully resolved, so it never pulses while the player aims.
        private void ShowBallHealth()
        {
            _views.Health.Show(_ball.RadiusPercent, _economy.GetHealth(_ball.RadiusPercent));
        }

        private async UniTask<GameResult> FinishAsync(CancellationToken cancellationToken)
        {
            _audio.Play(SoundId.Win);
            await _winSequence.PlayAsync(_path, cancellationToken);
            _score.AwardHealthBonus(_economy.GetRemainder(_ball.RadiusPercent));
            return GameResult.Finish(_score.Summary);
        }

        private void ShowObstaclePoints(Obstacle obstacle)
        {
            _views.Score.ShowPoints(obstacle.transform.position, _score.PointsPerObstacle);
        }

        private void ShowComboIfAny(Vector3 impactPoint, ShotScore shotScore)
        {
            if (!shotScore.IsCombo)
            {
                return;
            }

            _views.Score.ShowCombo(impactPoint, shotScore.Multiplier, shotScore.ComboBonus);
            _audio.Play(GetComboSound(shotScore.ComboLevel));
        }

        // The bigger the combo, the brighter the sound; tiers above the last sound reuse the brightest one.
        private static SoundId GetComboSound(int comboLevel) =>
            _comboSounds[Mathf.Clamp(comboLevel - 1, 0, _comboSounds.Length - 1)];
    }
}
