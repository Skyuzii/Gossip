using Gossip.Core.Abstractions.Messages.Common;

namespace Gossip.Core.Abstractions;

public interface IGossiper : IDisposable
{
    Task Start(CancellationToken cancellationToken);

    Task Stop(CancellationToken cancellationToken);

    ValueTask Enqueue(Message message, CancellationToken cancellationToken);
}