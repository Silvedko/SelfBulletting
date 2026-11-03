using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Levels;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Infrastructure.Services.Audio;
using UnityEngine;

namespace SelfBulletting.Gameplay.Obstacles
{
    /// <summary>
    /// Infects every obstacle inside the damage radius and blows them up as a wave from the impact point.
    /// The obstacle the shot hit always explodes.
    /// </summary>
    public sealed class ObstacleCascade
    {
        private readonly ObstacleScanner _scanner;
        private readonly Level _level;
        private readonly InfectionSettings _settings;
        private readonly IAudioService _audio;

        private readonly List<Collider> _colliders = new List<Collider>();
        private readonly List<Obstacle> _targets = new List<Obstacle>();
        private readonly List<UniTask> _explosions = new List<UniTask>();

        public ObstacleCascade(ObstacleScanner scanner, Level level, InfectionSettings settings, IAudioService audio)
        {
            _scanner = scanner;
            _level = level;
            _settings = settings;
            _audio = audio;
        }

        /// <returns>The number of destroyed obstacles.</returns>
        public async UniTask<int> RunAsync(Vector3 impactPoint, float damageRadius, Obstacle directHit,
            Action<Obstacle> onDestroyed, CancellationToken cancellationToken)
        {
            CollectTargets(impactPoint, damageRadius, directHit);

            _explosions.Clear();
            foreach (Obstacle target in _targets)
            {
                float delaySeconds = Vector3.Distance(impactPoint, target.transform.position) / _settings.WaveSpeed;
                _explosions.Add(DestroyAsync(target, delaySeconds, onDestroyed, cancellationToken));
            }

            await UniTask.WhenAll(_explosions);
            return _targets.Count;
        }

        private void CollectTargets(Vector3 impactPoint, float damageRadius, Obstacle directHit)
        {
            _targets.Clear();
            _scanner.FindInRadius(impactPoint, damageRadius, _colliders);

            foreach (Collider collider in _colliders)
            {
                if (_level.TryGetObstacle(collider, out Obstacle obstacle) && !obstacle.IsDestroyed)
                {
                    _targets.Add(obstacle);
                }
            }

            if (directHit != null && !directHit.IsDestroyed && !_targets.Contains(directHit))
            {
                _targets.Insert(0, directHit);
            }
        }

        private async UniTask DestroyAsync(Obstacle obstacle, float delaySeconds, Action<Obstacle> onDestroyed,
            CancellationToken cancellationToken)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken: cancellationToken);
            obstacle.Infect();
            _audio.Play(SoundId.Infect);

            await UniTask.Delay(TimeSpan.FromSeconds(_settings.InfectDurationSeconds), cancellationToken: cancellationToken);
            _audio.Play(SoundId.Explode);
            onDestroyed?.Invoke(obstacle);
            await obstacle.ExplodeAsync(cancellationToken);
        }
    }
}
