using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Look of the player ball. Gameplay passes ground points; the view lifts the ball by its radius.
    /// </summary>
    public interface IBallView
    {
        void Place(Vector3 groundPoint, float worldRadius);

        /// <summary>Current size, or the preview size while the player is aiming.</summary>
        void SetRadius(float worldRadius);

        /// <summary>0 — calm, 1 — the preview is at the critical size. Drives the pulsing.</summary>
        void SetDanger(float level);

        /// <summary>One hop towards the finish.</summary>
        UniTask HopAsync(Vector3 groundPoint, float height, float durationSeconds, CancellationToken cancellationToken);
    }
}
