using UnityEngine;

namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Floating score texts over the level: "+10" over each exploded obstacle and the combo label at the impact point.
    /// </summary>
    public interface IScoreView
    {
        void ShowPoints(Vector3 worldPosition, int points);

        void ShowCombo(Vector3 worldPosition, float multiplier, int bonus);
    }
}
