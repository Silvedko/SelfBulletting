using System;
using SelfBulletting.Gameplay.Hose;
using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullHoseView : IHoseView
    {
        public void Prepare(int maxDrops)
        {
        }

        public void ShowDrops(ReadOnlySpan<WaterDrop> drops)
        {
        }

        public void PlaySplash(Vector3 center, Vector3 groundPoint, float dropRadius)
        {
        }

        public void Hide()
        {
        }
    }
}
