using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigest;
using Gossip.Core.Abstractions.Peers;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

internal sealed class StartRumorDigestMessageHandler : IMessageHandler
{
    private readonly IPeerManager _peerManager;
    private readonly IReceiversPicker _receiversPicker;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<StartRumorDigestMessageHandler> _logger;

    public StartRumorDigestMessageHandler(
        IPeerManager peerManager,
        IReceiversPicker receiversPicker,
        IMessageSender messageSender,
        ILogger<StartRumorDigestMessageHandler> logger)
    {
        _peerManager = peerManager;
        _receiversPicker = receiversPicker;
        _messageSender = messageSender;
        _logger = logger;
    }

    public MessageType Type => MessageType.StartRumorDigest;

    public async Task Handle(Message message, CancellationToken cancellationToken)
    {
        if (message is not StartRumorDigestMessage)
        {
            throw new ArgumentOutOfRangeException();
        }

        _logger.LogInformation("Start handling the message {@Message}", message);

        RumorDigestMessage rumorDigestMessage = GenerateRumorDigestMessage(message.Sender);

        IReadOnlyCollection<PeerAddress> receivers = _receiversPicker.Pick();

        foreach (PeerAddress receiver in receivers)
        {
            _logger.LogInformation("Send to {Receiver} the message {@RumorDigestMessage}", receiver, rumorDigestMessage);

            await _messageSender.Send(receiver, rumorDigestMessage, cancellationToken);
        }
    }

    private RumorDigestMessage GenerateRumorDigestMessage(PeerAddress sender)
    {
        var peerInfos = new List<DigestPeerInfo>(capacity: _peerManager.ActiveRemotePeersCount + 1)
        {
            new DigestPeerInfo(_peerManager.LocalPeer.Address, _peerManager.LocalPeer.GetMaxVersion())
        };

        peerInfos.AddRange(_peerManager.ActiveRemotePeers
            .Select(peer => new DigestPeerInfo(peer.Address, peer.GetMaxVersion()))
            .ToArray());

        return new RumorDigestMessage(sender, peerInfos);
    }
}