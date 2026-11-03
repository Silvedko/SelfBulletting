using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Infrastructure.Services.Audio;
using UnityEngine;

namespace SelfBulletting.Gameplay.Finish
{
    /// <summary>
    /// The ball hops along the centre of the cleared path; the door opens once the ball is close enough.
    /// </summary>
    public sealed class WinSequence
    {
        private readonly FinishSettings _settings;
        private readonly GameViews _views;
        private readonly IAudioService _audio;

        public WinSequence(FinishSettings settings, GameViews views, IAudioService audio)
        {
            _settings = settings;
            _views = views;
            _audio = audio;
        }

        public async UniTask PlayAsync(Vector3 fromGroundPoint, Vector3 toGroundPoint, CancellationToken cancellationToken)
        {
            IReadOnlyList<Vector3> hops = HopPath.Build(fromGroundPoint, toGroundPoint, _settings.HopLength);
            Vector3 current = fromGroundPoint;
            bool isDoorOpen = false;

            foreach (Vector3 hop in hops)
            {
                isDoorOpen = OpenDoorWhenClose(current, toGroundPoint, isDoorOpen);
                _audio.Play(SoundId.Hop);
                await _views.Ball.HopAsync(hop, _settings.HopHeight, _settings.HopDurationSeconds, cancellationToken);
                current = hop;
            }

            OpenDoorWhenClose(current, toGroundPoint, isDoorOpen);
        }

        private bool OpenDoorWhenClose(Vector3 ballPoint, Vector3 doorPoint, bool isDoorOpen)
        {
            if (isDoorOpen || Vector3.Distance(ballPoint, doorPoint) > _settings.DoorOpenDistance)
            {
                return isDoorOpen;
            }

            _views.Door.Open();
            _audio.Play(SoundId.DoorOpen);
            return true;
        }
    }
}
