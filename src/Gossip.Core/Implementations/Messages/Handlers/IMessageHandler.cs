using Gossip.Core.Abstractions.Messages.Common;

namespace Gossip.Core.Implementations.Messages.Handlers;

internal interface IMessageHandler
{
    MessageType Type { get; }

    Task Handle(Message message, CancellationToken cancellationToken);
}