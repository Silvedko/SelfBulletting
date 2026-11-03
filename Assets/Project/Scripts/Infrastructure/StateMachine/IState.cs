using System.Threading;
using Cysharp.Threading.Tasks;

namespace SelfBulletting.Infrastructure.StateMachine
{
    public interface IState : IExitableState
    {
        UniTask EnterAsync(CancellationToken cancellationToken);
    }
}
