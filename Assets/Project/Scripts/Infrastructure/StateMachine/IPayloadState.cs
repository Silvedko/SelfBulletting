using System.Threading;
using Cysharp.Threading.Tasks;

namespace SelfBulletting.Infrastructure.StateMachine
{
    public interface IPayloadState<in TPayload> : IExitableState
    {
        UniTask EnterAsync(TPayload payload, CancellationToken cancellationToken);
    }
}
