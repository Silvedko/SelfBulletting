using SelfBulletting.Gameplay.Aim;
using SelfBulletting.Gameplay.Presentation;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Shot;

namespace SelfBulletting.Gameplay.Hose
{
    /// <summary>
    /// What the player sees while the water flows: the dotted arc the new drops take and the fires their splash will hit.
    /// No damage ring, no scale and no shot ball.
    /// </summary>
    public sealed class HosePreview
    {
        private readonly IObstacleSweep _sweep;
        private readonly DropSize _size;
        private readonly TargetHighlight _highlight;
        private readonly ILobAimView _lobAim;
        private readonly int _segments;

        /// <param name="segments">How many pieces of the arc are checked every frame.</param>
        public HosePreview(IObstacleSweep sweep, DropSize size, TargetHighlight highlight, ILobAimView lobAim, int segments)
        {
            _sweep = sweep;
            _size = size;
            _highlight = highlight;
            _lobAim = lobAim;
            _segments = segments;
        }

        /// <param name="dropRadius">Radius of the drops leaving along the arc, meters.</param>
        public void Show(ArcPath arc, float dropRadius)
        {
            DropImpact impact = ArcProbe.Find(arc, dropRadius, _size.GetTouchRadius(dropRadius), _segments, _sweep);
            _lobAim.ShowTrajectory(arc);
            _highlight.Show(impact.Center, _size.GetSplashRadius(dropRadius));
        }

        public void Hide()
        {
            _lobAim.HideTrajectory();
            _highlight.Clear();
        }
    }
}
