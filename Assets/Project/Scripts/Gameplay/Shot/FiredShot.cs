namespace SelfBulletting.Gameplay.Shot
{
    public readonly struct FiredShot
    {
        public FiredShot(ShotRequest request, ShotTarget target)
        {
            Request = request;
            Target = target;
        }

        public ShotRequest Request { get; }
        public ShotTarget Target { get; }
    }
}
