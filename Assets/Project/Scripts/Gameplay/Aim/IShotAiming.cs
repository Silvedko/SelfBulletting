using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Shot;
using UnityEngine;

namespace SelfBulletting.Gameplay.Aim
{
    /// <summary>
    /// Turns the player's input into one released shot. Every game mode has its own way to aim.
    /// </summary>
    public interface IShotAiming
    {
        /// <param name="mode">Aimed with the scale, or the last shot without it.</param>
        /// <param name="blocker">The first obstacle in the ball's corridor. Classic rolls the shot towards it.</param>
        UniTask<FiredShot> TakeShotAsync(ShotMode mode, RaycastHit blocker, CancellationToken cancellationToken);
    }
}
