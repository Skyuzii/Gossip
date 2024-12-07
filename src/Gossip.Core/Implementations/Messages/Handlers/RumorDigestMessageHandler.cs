using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigest;
using Gossip.Core.Abstractions.Messages.RumorDigestAck;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Implementations.Messages.Handlers.Collectors;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

internal sealed class RumorDigestMessageHandler : BaseMessageHandler<RumorDigestMessage>
{
    private readonly IInfoCollector _infoCollector;

    public RumorDigestMessageHandler(
        IMessageSender messageSender,
        IPeerManager peerManager,
        ILogger logger,
        IInfoCollector infoCollector) : base(MessageType.RumorDigest, messageSender, peerManager, logger)
    {
        _infoCollector = infoCollector;
    }

    protected override Task HandleInternal(RumorDigestMessage message, CancellationToken cancellationToken)
    {
        (IReadOnlyCollection<DigestPeerInfo> digestPeerInfos, IReadOnlyCollection<FullPeerInfo> fullPeerInfos) = _infoCollector.Collect(message.DigestPeerInfos);

        return SendMessage(
            message.Sender,
            new RumorDigestAckMessage(PeerManager.LocalPeer.Address, digestPeerInfos, fullPeerInfos),
            cancellationToken);
    }
}