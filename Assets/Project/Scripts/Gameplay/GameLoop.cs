using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Ball;
using SelfBulletting.Gameplay.Finish;
using SelfBulletting.Gameplay.Paths;
using SelfBulletting.Gameplay.Rules;
using SelfBulletting.Gameplay.Scanning;
using SelfBulletting.Gameplay.Score;
using SelfBulletting.Gameplay.Strategies;
using SelfBulletting.Infrastructure.Services.Audio;
using UnityEngine;

namespace SelfBulletting.Gameplay
{
    /// <summary>
    /// One round as a linear async loop: check the path → a turn of the mode → check again, until the ball wins or has
    /// nothing left to spend. The mode decides what a turn is (<see cref="IShotStrategy"/>): one released shot, or the
    /// Hose pouring until the way is open or the water runs out.
    /// </summary>
    public sealed class GameLoop
    {
        private readonly GroundPath _path;
        private readonly BallModel _ball;
        private readonly RoundRules _rules;
        private readonly PathCheck _pathCheck;
        private readonly IShotStrategy _shots;
        private readonly ScoreCounter _score;
        private readonly WinSequence _winSequence;
        private readonly IAudioService _audio;

        public GameLoop(GroundPath path, BallModel ball, RoundRules rules, PathCheck pathCheck, IShotStrategy shots,
            ScoreCounter score, WinSequence winSequence, IAudioService audio)
        {
            _path = path;
            _ball = ball;
            _rules = rules;
            _pathCheck = pathCheck;
            _shots = shots;
            _score = score;
            _winSequence = winSequence;
            _audio = audio;
        }

        public async UniTask<GameResult> RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                bool isBlocked = _pathCheck.TryFindBlocker(out RaycastHit blocker);

                switch (_rules.Evaluate(!isBlocked, _ball.RadiusPercent))
                {
                    case RoundOutcome.Win:
                        return await FinishAsync(cancellationToken);

                    case RoundOutcome.Lose:
                        _audio.Play(SoundId.Lose);
                        return GameResult.Lose(_score.Summary);

                    default:
                        await _shots.PlayAsync(blocker, cancellationToken);
                        break;
                }
            }
        }

        private async UniTask<GameResult> FinishAsync(CancellationToken cancellationToken)
        {
            _audio.Play(SoundId.Win);
            await _winSequence.PlayAsync(_path, cancellationToken);
            _score.AwardHealthBonus(_ball.Remainder);
            return GameResult.Finish(_score.Summary);
        }
    }
}
