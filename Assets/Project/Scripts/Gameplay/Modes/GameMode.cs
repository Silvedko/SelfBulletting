namespace SelfBulletting.Gameplay.Modes
{
    /// <summary>
    /// How the player clears the path. Every mode keeps its own config, its own level and its own total score.
    /// Values are stored as numbers (menu buttons, mode configs), so new modes are appended.
    /// </summary>
    public enum GameMode
    {
        /// <summary>Straight path; the shot always rolls along it, the hold sets the charge.</summary>
        Classic,

        /// <summary>Curved path; drag back to aim, the drag length sets the range, the shot flies in an arc.</summary>
        Slingshot,

        /// <summary>Curved path; the arrow swings by itself, the first tap freezes it, the second one fires.</summary>
        Arrow,

        /// <summary>Curved path; drag back like the slingshot and the ball pours itself out as a stream of drops.</summary>
        Hose
    }

    public static class GameModeExtensions
    {
        /// <summary>Every mode but Classic plays the curved levels.</summary>
        public static bool UsesCurvedPath(this GameMode mode) => mode != GameMode.Classic;
    }
}
