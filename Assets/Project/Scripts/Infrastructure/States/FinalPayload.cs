using SelfBulletting.Gameplay;

namespace SelfBulletting.Infrastructure.States
{
    public readonly struct FinalPayload
    {
        public FinalPayload(GameResult result, GameEntryPoint game)
        {
            Result = result;
            Game = game;
        }

        public GameResult Result { get; }
        public GameEntryPoint Game { get; }
    }
}
