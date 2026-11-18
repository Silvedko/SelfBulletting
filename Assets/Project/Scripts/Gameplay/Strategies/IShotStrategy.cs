using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SelfBulletting.Gameplay.Strategies
{
    /// <summary>
    /// One turn of the round: how the ball spends itself on the obstacles. The round checks the path before every turn
    /// and decides the win or the loss; the strategy only clears the way.
    /// </summary>
    public interface IShotStrategy
    {
        /// <param name="blocker">The first obstacle on the path. Classic rolls its shot towards it.</param>
        UniTask PlayAsync(RaycastHit blocker, CancellationToken cancellationToken);
    }
}
