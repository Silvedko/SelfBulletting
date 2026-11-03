using System.Threading;
using Cysharp.Threading.Tasks;

namespace SelfBulletting.Infrastructure.StateMachine
{
    public interface IExitableState
    {
        UniTask ExitAsync(CancellationToken cancellationToken);
    }
}
