using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigestAck2;
using Gossip.Core.Abstractions.Peers;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

internal sealed class RumorDigestAck2MessageHandler : IMessageHandler
{
    private readonly IPeerManager _peerManager;
    private readonly ILogger<RumorDigestAck2MessageHandler> _logger;

    public RumorDigestAck2MessageHandler(
        IPeerManager peerManager,
        ILogger<RumorDigestAck2MessageHandler> logger)
    {
        _peerManager = peerManager;
        _logger = logger;
    }

    public MessageType Type => MessageType.RumorDigestAck2;

    public Task Handle(Message message, CancellationToken cancellationToken)
    {
        if (message is not RumorDigestAck2Message rumorDigestAck2Message)
        {
            throw new ArgumentOutOfRangeException();
        }

        _logger.LogInformation("Start handling the message {@RumorDigestAck2Message}", rumorDigestAck2Message);

        foreach (FullPeerInfo? fullPeerInfo in rumorDigestAck2Message.FullPeerInfos)
        {
            if (_peerManager.TryGet(fullPeerInfo.Address, out Peer existPeer))
            {
                existPeer.Apply(fullPeerInfo.Rumors);
            }
            else
            {
                _peerManager.Add(Peer.CreateRemote(fullPeerInfo.Address, fullPeerInfo.Rumors));
            }
        }

        return Task.CompletedTask;
    }
}