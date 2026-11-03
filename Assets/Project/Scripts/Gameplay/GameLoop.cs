using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Aim;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Finish;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Obstacles;
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
    /// </summary>
    public sealed class GameLoop
    {
        private static readonly SoundId[] s_comboSounds = { SoundId.Combo, SoundId.ComboBig, SoundId.ComboHuge };

        private readonly Level _level;
        private readonly BallModel _ball;
        private readonly ShotEconomy _economy;
        private readonly RoundRules _rules;
        private readonly PathScanner _pathScanner;
        private readonly AimController _aim;
        private readonly ObstacleCascade _cascade;
        private readonly ScoreCounter _score;
        private readonly WinSequence _winSequence;
        private readonly GameViews _views;
        private readonly IAudioService _audio;
        private readonly Action<Obstacle> _onObstacleDestroyed;

        public GameLoop(Level level, BallModel ball, ShotEconomy economy, RoundRules rules, PathScanner pathScanner,
            AimController aim, ObstacleCascade cascade, ScoreCounter score, WinSequence winSequence, GameViews views,
            IAudioService audio)
        {
            _level = level;
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
                Vector3 ballCenter = GetBallCenter();
                bool isBlocked = _pathScanner.TryFindFirstObstacle(ballCenter, GetFinishCenter(), _ball.WorldRadius, out RaycastHit firstObstacle);

                switch (_rules.Evaluate(!isBlocked, _ball.RadiusPercent))
                {
                    case RoundOutcome.Win:
                        return await FinishAsync(cancellationToken);

                    case RoundOutcome.Lose:
                        _audio.Play(SoundId.Lose);
                        return GameResult.Lose(_score.Summary);

                    default:
                        await PlayShotAsync(ballCenter, firstObstacle, cancellationToken);
                        break;
                }
            }
        }

        private async UniTask PlayShotAsync(Vector3 ballCenter, RaycastHit target, CancellationToken cancellationToken)
        {
            ShotMode mode = _economy.GetShotMode(_ball.RadiusPercent);
            ShotRequest shot = await _aim.TakeShotAsync(mode, ballCenter, target.point, cancellationToken);

            _ball.Spend(shot.CostPercent);
            _audio.Play(SoundId.ShotFire);
            await _views.Shot.FlyAsync(target.point, _economy.ShotSpeed, cancellationToken);
            _views.Shot.Hide();

            _level.TryGetObstacle(target.collider, out Obstacle directHit);
            int destroyed = await _cascade.RunAsync(target.point, shot.DamageWorldRadius, directHit, _onObstacleDestroyed, cancellationToken);
            ShowComboIfAny(target.point, _score.AddShot(destroyed));
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
            await _winSequence.PlayAsync(_level.StartPoint, _level.FinishPoint, cancellationToken);
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
            s_comboSounds[Mathf.Clamp(comboLevel - 1, 0, s_comboSounds.Length - 1)];

        // The sphere is swept at the height of the ball centre, so it touches the ground like the ball does.
        private Vector3 GetBallCenter() => _level.StartPoint + Vector3.up * _ball.WorldRadius;

        private Vector3 GetFinishCenter() => _level.FinishPoint + Vector3.up * _ball.WorldRadius;
    }
}
