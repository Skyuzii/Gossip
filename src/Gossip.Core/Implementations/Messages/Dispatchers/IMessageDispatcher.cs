using Gossip.Core.Abstractions.Messages.Common;

namespace Gossip.Core.Implementations.Messages.Dispatchers;

internal interface IMessageDispatcher : IDisposable
{
    ValueTask Enqueue(Message message, CancellationToken cancellationToken);
}