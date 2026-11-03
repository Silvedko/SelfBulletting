namespace SelfBulletting.Gameplay.Shot
{
    public enum ShotMode
    {
        /// <summary>Nothing is left above the critical size.</summary>
        None,

        /// <summary>Hold to charge on the ping-pong scale (full or cut to what the ball can afford).</summary>
        Aimed,

        /// <summary>The last shot without the scale: a tap fires everything above the critical size.</summary>
        Instant
    }
}
