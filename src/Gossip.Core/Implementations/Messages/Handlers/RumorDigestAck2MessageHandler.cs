using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigestAck2;
using Gossip.Core.Abstractions.Peers;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

internal sealed class RumorDigestAck2MessageHandler : BaseMessageHandler<RumorDigestAck2Message>
{
    public RumorDigestAck2MessageHandler(
        IMessageSender messageSender,
        IPeerManager peerManager,
        ILogger logger) : base(MessageType.RumorDigestAck2, messageSender, peerManager, logger)
    {
    }

    protected override Task HandleInternal(RumorDigestAck2Message message, CancellationToken cancellationToken)
    {
        foreach (FullPeerInfo? fullPeerInfo in message.FullPeerInfos)
        {
            if (PeerManager.TryGet(fullPeerInfo.Address, out Peer existPeer))
            {
                existPeer.Apply(fullPeerInfo.Rumors);
            }
            else
            {
                PeerManager.Add(Peer.CreateRemote(fullPeerInfo.Address, fullPeerInfo.Rumors));
            }
        }

        return Task.CompletedTask;
    }
}