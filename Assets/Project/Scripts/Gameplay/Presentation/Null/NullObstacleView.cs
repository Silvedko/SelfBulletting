using System.Threading;
using Cysharp.Threading.Tasks;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullObstacleView : IObstacleView
    {
        public void ShowInfected()
        {
        }

        public void SetHighlighted(bool isHighlighted)
        {
        }

        public UniTask PlayExplosionAsync(CancellationToken cancellationToken) => UniTask.CompletedTask;
    }
}
