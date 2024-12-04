using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigest;
using Gossip.Core.Abstractions.Messages.RumorDigestAck;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

public sealed class RumorDigestMessageHandler : IMessageHandler
{
    private readonly IPeerManager _peerManager;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<RumorDigestMessageHandler> _logger;

    public RumorDigestMessageHandler(
        IPeerManager peerManager,
        IMessageSender messageSender,
        ILogger<RumorDigestMessageHandler> logger)
    {
        _peerManager = peerManager;
        _messageSender = messageSender;
        _logger = logger;
    }

    public MessageType Type => MessageType.RumorDigest;

    public Task Handle(Message message, CancellationToken cancellationToken)
    {
        if (message is not RumorDigestMessage rumorDigestMessage)
        {
            throw new ArgumentOutOfRangeException();
        }

        _logger.LogInformation("Start handling the message {@RumorDigestMessage}", rumorDigestMessage);

        var digestPeerInfos = new List<DigestPeerInfo>();
        var fullPeerInfos = new List<FullPeerInfo>();

        foreach (DigestPeerInfo digestPeerInfo in rumorDigestMessage.DigestPeerInfos)
        {
            if (!_peerManager.TryGet(digestPeerInfo.Address, out Peer existPeer))
            {
                digestPeerInfos.Add(digestPeerInfo with { MaxVersion = RumorVersion.Empty });

                continue;
            }

            RumorVersion existPeerMaxVersion = existPeer.GetMaxVersion();

            if (digestPeerInfo.MaxVersion > existPeerMaxVersion)
            {
                digestPeerInfos.Add(digestPeerInfo with { MaxVersion = existPeerMaxVersion });

                continue;
            }

            if (digestPeerInfo.MaxVersion < existPeerMaxVersion)
            {
                fullPeerInfos.Add(
                    new FullPeerInfo(
                        existPeer.Address,
                        existPeer.Rumors.Values.Where(x => x.Version > digestPeerInfo.MaxVersion).ToArray()));
            }
        }

        var rumorDigestAckMessage = new RumorDigestAckMessage(_peerManager.LocalPeer.Address, digestPeerInfos, fullPeerInfos);

        _logger.LogInformation("Send to {Receiver} the message {@RumorDigestAckMessage}", message.Sender, rumorDigestAckMessage);

        return _messageSender.Send(message.Sender, rumorDigestAckMessage, cancellationToken);
    }
}