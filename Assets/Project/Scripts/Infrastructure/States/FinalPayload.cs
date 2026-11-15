using SelfBulletting.Gameplay;
using SelfBulletting.Gameplay.Modes;

namespace SelfBulletting.Infrastructure.States
{
    public readonly struct FinalPayload
    {
        public FinalPayload(GameResult result, GameEntryPoint game, GameMode mode)
        {
            Result = result;
            Game = game;
            Mode = mode;
        }

        public GameResult Result { get; }
        public GameEntryPoint Game { get; }

        /// <summary>The mode the round was played in: the score and the progress go there, and Next or Retry stays in it.</summary>
        public GameMode Mode { get; }
    }
}
