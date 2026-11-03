using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Look of the shot ball. It is always the same size (see <c>ShotSettings.ShotRadiusPercent</c>).
    /// </summary>
    public interface IShotView
    {
        /// <summary>The shot separates from the ball on the side facing the target while the player holds the tap.</summary>
        void ShowCharging(Vector3 ballCenter, float ballRadius, Vector3 target, float shotRadius);

        UniTask FlyAsync(Vector3 target, float speed, CancellationToken cancellationToken);

        void Hide();
    }
}
