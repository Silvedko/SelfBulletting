using System.Threading;
using Cysharp.Threading.Tasks;
using SelfBulletting.Gameplay.Score;
using SelfBulletting.UI.Final;
using UnityEngine;

namespace SelfBulletting.Gameplay
{
    /// <summary>
    /// Composition root of the Game scene.
    /// </summary>
    public sealed class GameEntryPoint : MonoBehaviour
    {
        [SerializeField] private FinalScreen _finalScreen;

        private GameDependencies _dependencies;

        public FinalScreen FinalScreen => _finalScreen;

        public void Construct(GameDependencies dependencies)
        {
            _dependencies = dependencies;
            _finalScreen.Hide();
        }

        public async UniTask<GameResult> PlayRoundAsync(CancellationToken cancellationToken)
        {
            using (var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, destroyCancellationToken))
            {
                // Skeleton placeholder: a tap ends the round. The gameplay step replaces this with GameLoop.
                await UniTask.WaitUntil(() => _dependencies.Input.WasPressedThisFrame, cancellationToken: linked.Token);
                return GameResult.Finish(ScoreSummary.Empty);
            }
        }
    }
}
