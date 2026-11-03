using System.Threading;
using Cysharp.Threading.Tasks;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Look of one obstacle. Lives on the obstacle prefab next to <see cref="Obstacles.Obstacle"/>.
    /// </summary>
    public interface IObstacleView
    {
        /// <summary>The obstacle caught the infection (in the reference it turns the player's orange).</summary>
        void ShowInfected();

        /// <summary>The explosion. The obstacle is disabled when it completes.</summary>
        UniTask PlayExplosionAsync(CancellationToken cancellationToken);
    }
}
