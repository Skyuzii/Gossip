using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Abstractions.Messages;

public interface IMessageSender
{
    Task<MessageSendResult> Send(PeerAddress receiver, Message message, CancellationToken cancellationToken);
}