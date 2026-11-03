using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation.Null
{
    public sealed class NullScoreView : IScoreView
    {
        public void ShowPoints(Vector3 worldPosition, int points)
        {
        }

        public void ShowCombo(Vector3 worldPosition, float multiplier, int bonus)
        {
        }
    }
}
