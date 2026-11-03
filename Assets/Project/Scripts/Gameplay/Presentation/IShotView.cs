using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Look of the shot ball. It is as big as what the player ball gives away, so it grows and shrinks with the charge.
    /// </summary>
    public interface IShotView
    {
        /// <summary>
        /// The shot sits in front of the player ball. Called every frame while the player aims
        /// (and once for the last shot without the scale); the first call after <see cref="Hide"/> pops it out.
        /// </summary>
        void ShowCharging(Vector3 launchCenter, float shotRadius);

        /// <summary>Rolls the shot straight along the path to where it touches the obstacle.</summary>
        UniTask FlyAsync(Vector3 contactCenter, float speed, CancellationToken cancellationToken);

        void Hide();
    }
}
