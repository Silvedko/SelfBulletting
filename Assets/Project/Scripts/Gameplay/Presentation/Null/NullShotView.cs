using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullShotView : IShotView
    {
        public void ShowCharging(Vector3 launchCenter, float shotRadius)
        {
        }

        public UniTask FlyAsync(Vector3 contactCenter, float speed, CancellationToken cancellationToken) => UniTask.CompletedTask;

        public void Hide()
        {
        }
    }
}
