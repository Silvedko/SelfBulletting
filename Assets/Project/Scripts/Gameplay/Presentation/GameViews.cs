namespace SelfBulletting.Gameplay.Presentation
{
    /// <summary>
    /// Every view the round talks to. Missing views are replaced with null objects, so the round runs without visuals.
    /// </summary>
    public sealed class GameViews
    {
        public GameViews(IBallView ball, IShotView shot, IPathView path, IAimView aim, IScoreView score, IDoorView door,
            IBallHealthView health, ILevelView level)
        {
            Ball = ball;
            Shot = shot;
            Path = path;
            Aim = aim;
            Score = score;
            Door = door;
            Health = health;
            Level = level;
        }

        public IBallView Ball { get; }
        public IShotView Shot { get; }
        public IPathView Path { get; }
        public IAimView Aim { get; }
        public IScoreView Score { get; }
        public IDoorView Door { get; }
        public IBallHealthView Health { get; }
        public ILevelView Level { get; }
    }
}
