using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigestAck;
using Gossip.Core.Abstractions.Messages.RumorDigestAck2;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Implementations.Messages.Handlers.Collectors;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

internal sealed class RumorDigestAckMessageHandler : BaseMessageHandler<RumorDigestAckMessage>
{
    private readonly IInfoCollector _infoCollector;

    public RumorDigestAckMessageHandler(
        IMessageSender messageSender,
        IPeerManager peerManager,
        ILogger logger,
        IInfoCollector infoCollector) : base(MessageType.RumorDigestAck, messageSender, peerManager, logger)
    {
        _infoCollector = infoCollector;
    }

    protected override Task HandleInternal(RumorDigestAckMessage message, CancellationToken cancellationToken)
    {
        HandleFullPeerInfos(message);

        return HandleDigestPeerInfos(message, cancellationToken);
    }

    private void HandleFullPeerInfos(RumorDigestAckMessage rumorDigestAckMessage)
    {
        foreach (FullPeerInfo fullPeerInfo in rumorDigestAckMessage.FullPeerInfos)
        {
            if (PeerManager.TryGet(fullPeerInfo.Address, out Peer? existPeer) && existPeer is RemotePeer existRemotePeer)
            {
                existRemotePeer.Apply(fullPeerInfo.Generation, fullPeerInfo.Rumors);
            }
            else
            {
                PeerManager.Add(RemotePeer.New(fullPeerInfo.Address, fullPeerInfo.Generation, fullPeerInfo.Rumors));
            }
        }
    }

    private Task HandleDigestPeerInfos(RumorDigestAckMessage rumorDigestAckMessage, CancellationToken cancellationToken)
    {
        if (rumorDigestAckMessage.DigestPeerInfos.Count == 0)
        {
            return Task.CompletedTask;
        }

        (IReadOnlyCollection<DigestPeerInfo> _, IReadOnlyCollection<FullPeerInfo> fullPeerInfos) = _infoCollector.Collect(rumorDigestAckMessage.DigestPeerInfos, fillDigestPeerInfos: false);

        return SendMessage(
            rumorDigestAckMessage.Sender,
            new RumorDigestAck2Message(PeerManager.LocalPeer.Address, fullPeerInfos),
            cancellationToken);
    }
}