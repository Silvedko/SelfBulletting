using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Configs;
using SelfBulletting.Gameplay.Paths;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Infrastructure.Services.Audio;
using UnityEngine;

namespace SelfBulletting.Gameplay.Finish
{
    /// <summary>
    /// The ball hops along the centre of the cleared path; the door opens once the ball is close enough along the path.
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

        public async UniTask PlayAsync(GroundPath path, CancellationToken cancellationToken)
        {
            IReadOnlyList<Vector3> hops = HopPath.Build(path, _settings.HopLength);
            float hopLength = hops.Count > 0 ? path.Length / hops.Count : 0f;
            bool isDoorOpen = false;

            for (int i = 0; i < hops.Count; i++)
            {
                isDoorOpen = OpenDoorWhenClose(path.Length - hopLength * i, isDoorOpen);
                _audio.Play(SoundId.Hop);
                await _views.Ball.HopAsync(hops[i], _settings.HopHeight, _settings.HopDurationSeconds, cancellationToken);
            }

            OpenDoorWhenClose(0f, isDoorOpen);
        }

        private bool OpenDoorWhenClose(float distanceToDoor, bool isDoorOpen)
        {
            if (isDoorOpen || distanceToDoor > _settings.DoorOpenDistance)
            {
                return isDoorOpen;
            }

            _views.Door.Open();
            _audio.Play(SoundId.DoorOpen);
            return true;
        }
    }
}
