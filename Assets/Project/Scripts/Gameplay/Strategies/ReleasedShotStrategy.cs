using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Aim;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Obstacles;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Score;
using SelfBulletting.Gameplay.Shot;
using SelfBulletting.Infrastructure.Services.Audio;
using UnityEngine;

namespace SelfBulletting.Gameplay.Strategies
{
    /// <summary>
    /// Classic, Slingshot and Arrow: one turn is one released shot. Aim → spend the mass → the shot rolls or flies →
    /// the infection wave → points and combo. Only the aiming differs between the modes.
    /// </summary>
    public sealed class ReleasedShotStrategy : IShotStrategy
    {
        private static readonly SoundId[] _comboSounds = { SoundId.Combo, SoundId.ComboBig, SoundId.ComboHuge };

        private readonly Level _level;
        private readonly BallModel _ball;
        private readonly ShotEconomy _economy;
        private readonly IShotAiming _aim;
        private readonly ObstacleCascade _cascade;
        private readonly ScoreCounter _score;
        private readonly GameViews _views;
        private readonly IAudioService _audio;
        private readonly Action<Obstacle> _onObstacleDestroyed;

        public ReleasedShotStrategy(Level level, BallModel ball, ShotEconomy economy, IShotAiming aim,
            ObstacleCascade cascade, ScoreCounter score, GameViews views, IAudioService audio)
        {
            _level = level;
            _ball = ball;
            _economy = economy;
            _aim = aim;
            _cascade = cascade;
            _score = score;
            _views = views;
            _audio = audio;
            _onObstacleDestroyed = ShowObstaclePoints;
        }

        public async UniTask PlayAsync(RaycastHit blocker, CancellationToken cancellationToken)
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

            // The HUD changes only once the shot is fully resolved, so it never pulses while the player aims.
            _views.Health.Show(_ball.RadiusPercent, _ball.Health);
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
