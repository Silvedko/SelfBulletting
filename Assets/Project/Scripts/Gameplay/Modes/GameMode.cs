namespace SelfBulletting.Gameplay.Modes
{
    /// <summary>
    /// How the player aims. Every mode keeps its own level and its own total score.
    /// </summary>
    public enum GameMode
    {
        /// <summary>Straight path; the shot always rolls along it, the hold sets the charge.</summary>
        Classic,

        /// <summary>Curved path; drag back to aim, the drag length sets the range, the shot flies in an arc.</summary>
        Slingshot,

        /// <summary>Curved path; the arrow swings by itself, the press freezes it, the charge sets the range.</summary>
        Arrow
    }

    public static class GameModeExtensions
    {
        /// <summary>Aim modes play the curved levels and lob the shot over the obstacles.</summary>
        public static bool IsAimed(this GameMode mode) => mode != GameMode.Classic;
    }
}
