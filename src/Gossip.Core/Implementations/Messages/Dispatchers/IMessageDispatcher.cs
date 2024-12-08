using Gossip.Core.Abstractions.Messages.Common;

namespace Gossip.Core.Implementations.Messages.Dispatchers;

internal interface IMessageDispatcher : IDisposable
{
    ValueTask Process(Message message, CancellationToken cancellationToken);

    ValueTask Enqueue(Message message, CancellationToken cancellationToken);
}