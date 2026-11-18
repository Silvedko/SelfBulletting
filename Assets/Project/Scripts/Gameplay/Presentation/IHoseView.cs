using System;
using SelfBulletting.Gameplay.Hose;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Hose mode: the drops in the air and their splashes. Gameplay moves every drop; the view only puts a droplet there.
    /// </summary>
    public interface IHoseView
    {
        /// <summary>Called once before the round, so no droplet is created while the water flows.</summary>
        void Prepare(int maxDrops);

        /// <summary>Called every frame of the Hose turn with every drop in the air, in no particular order.</summary>
        void ShowDrops(ReadOnlySpan<WaterDrop> drops);

        /// <summary>A drop hit a fire or the ground.</summary>
        /// <param name="center">The drop centre when it hit.</param>
        /// <param name="groundPoint">The ground under it, where the water spreads.</param>
        void PlaySplash(Vector3 center, Vector3 groundPoint, float dropRadius);

        void Hide();
    }
}
