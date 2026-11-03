using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullBallView : IBallView
    {
        public void Place(Vector3 groundPoint, float worldRadius)
        {
        }

        public void SetRadius(float worldRadius)
        {
        }

        public void SetDanger(float level)
        {
        }

        public UniTask HopAsync(Vector3 groundPoint, float height, float durationSeconds, CancellationToken cancellationToken) =>
            UniTask.CompletedTask;
    }
}
