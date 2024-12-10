using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigest;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

internal sealed class StartRumorDigestMessageHandler : BaseMessageHandler<StartRumorDigestMessage>
{
    private readonly IReceiversPicker _receiversPicker;

    public StartRumorDigestMessageHandler(
        IMessageSender messageSender,
        IPeerManager peerManager,
        ILogger logger,
        IReceiversPicker receiversPicker) : base(MessageType.StartRumorDigest, messageSender, peerManager, logger)
    {
        _receiversPicker = receiversPicker;
    }

    protected override async Task HandleInternal(StartRumorDigestMessage message, CancellationToken cancellationToken)
    {
        RumorDigestMessage rumorDigestMessage = GenerateRumorDigestMessage(message.Sender);

        IReadOnlyCollection<PeerAddress> receivers = _receiversPicker.Pick();

        foreach (PeerAddress receiver in receivers)
        {
            await SendMessage(receiver, rumorDigestMessage, cancellationToken);
        }
    }

    private RumorDigestMessage GenerateRumorDigestMessage(PeerAddress sender)
    {
        LocalPeer localPeer = PeerManager.LocalPeer;
        localPeer.Apply(Rumor.NextHeartBeat());

        var peerInfos = new List<DigestPeerInfo>(capacity: PeerManager.ActiveRemotePeersCount + 1)
        {
            new DigestPeerInfo(localPeer.Address, localPeer.Generation, localPeer.GetMaxVersion())
        };

        peerInfos.AddRange(PeerManager.ActiveRemotePeers
            .Select(peer => new DigestPeerInfo(peer.Address, peer.Generation, peer.GetMaxVersion()))
            .ToArray());

        return new RumorDigestMessage(sender, peerInfos);
    }
}