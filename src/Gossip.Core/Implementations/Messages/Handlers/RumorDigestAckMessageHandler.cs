using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigestAck;
using Gossip.Core.Abstractions.Messages.RumorDigestAck2;
using Gossip.Core.Abstractions.Peers;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

internal sealed class RumorDigestAckMessageHandler : IMessageHandler
{
    private readonly IPeerManager _peerManager;
    private readonly IMessageSender _messageSender;
    private readonly ILogger<RumorDigestAckMessageHandler> _logger;

    public RumorDigestAckMessageHandler(
        IPeerManager peerManager,
        IMessageSender messageSender,
        ILogger<RumorDigestAckMessageHandler> logger)
    {
        _peerManager = peerManager;
        _messageSender = messageSender;
        _logger = logger;
    }

    public MessageType Type => MessageType.RumorDigestAck;

    public Task Handle(Message message, CancellationToken cancellationToken)
    {
        if (message is not RumorDigestAckMessage rumorDigestAckMessage)
        {
            throw new ArgumentOutOfRangeException();
        }

        _logger.LogInformation("Start handling the message {@RumorDigestAckMessage}", rumorDigestAckMessage);

        HandleFullPeerInfos(rumorDigestAckMessage);

        return HandleDigestPeerInfos(rumorDigestAckMessage, cancellationToken);
    }

    private void HandleFullPeerInfos(RumorDigestAckMessage rumorDigestAckMessage)
    {
        foreach (FullPeerInfo? actualPeerInfo in rumorDigestAckMessage.FullPeerInfos)
        {
            if (_peerManager.TryGet(actualPeerInfo.Address, out Peer? existPeer))
            {
                existPeer.Apply(actualPeerInfo.Rumors);
            }
            else
            {
                _peerManager.Add(Peer.CreateRemote(actualPeerInfo.Address, actualPeerInfo.Rumors));
            }
        }
    }

    private Task HandleDigestPeerInfos(RumorDigestAckMessage rumorDigestAckMessage, CancellationToken cancellationToken)
    {
        if (rumorDigestAckMessage.DigestPeerInfos.Count == 0)
        {
            return Task.CompletedTask;
        }

        var fullPeerInfos = new List<FullPeerInfo>();

        foreach (DigestPeerInfo digestPeerInfo in rumorDigestAckMessage.DigestPeerInfos)
        {
            if (_peerManager.TryGet(digestPeerInfo.Address, out Peer? existPeer) && digestPeerInfo.MaxVersion < existPeer.GetMaxVersion())
            {
                fullPeerInfos.Add(
                    new FullPeerInfo(
                        existPeer.Address,
                        existPeer.Rumors.Values.Where(x => x.Version > digestPeerInfo.MaxVersion).ToArray()));
            }
        }

        var rumorDigestAck2Message = new RumorDigestAck2Message(_peerManager.LocalPeer.Address, fullPeerInfos);

        _logger.LogInformation("Send to {Receiver} the message {@RumorDigestAck2Message}", rumorDigestAckMessage.Sender, rumorDigestAck2Message);

        return _messageSender.Send(rumorDigestAckMessage.Sender, rumorDigestAck2Message, cancellationToken);
    }
}