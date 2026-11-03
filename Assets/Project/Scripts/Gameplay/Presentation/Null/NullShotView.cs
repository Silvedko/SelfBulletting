using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullShotView : IShotView
    {
        public void ShowCharging(Vector3 ballCenter, float ballRadius, Vector3 target, float shotRadius)
        {
        }

        public UniTask FlyAsync(Vector3 target, float speed, CancellationToken cancellationToken) => UniTask.CompletedTask;

        public void Hide()
        {
        }
    }
}
