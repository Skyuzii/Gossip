using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigestAck;
using Gossip.Core.Abstractions.Messages.RumorDigestAck2;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

internal sealed class RumorDigestAckMessageHandler : BaseMessageHandler<RumorDigestAckMessage>
{
    public RumorDigestAckMessageHandler(
        IMessageSender messageSender,
        IPeerManager peerManager,
        ILogger logger) : base(MessageType.RumorDigestAck, messageSender, peerManager, logger)
    {
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

        var fullPeerInfos = new List<FullPeerInfo>();

        foreach (DigestPeerInfo digestPeerInfo in rumorDigestAckMessage.DigestPeerInfos)
        {
            if (!PeerManager.TryGet(digestPeerInfo.Address, out Peer? existPeer) || digestPeerInfo.Generation > existPeer.Generation)
            {
                continue;
            }

            if (digestPeerInfo.Generation < existPeer.Generation)
            {
                fullPeerInfos.Add(
                    new FullPeerInfo(
                        existPeer.Address,
                        existPeer.Generation,
                        existPeer.Rumors.Values.ToArray()));

                continue;
            }

            if (digestPeerInfo.MaxVersion < existPeer.GetMaxVersion())
            {
                fullPeerInfos.Add(
                    new FullPeerInfo(
                        existPeer.Address,
                        existPeer.Generation,
                        existPeer.Rumors.Values.Where(x => x.Version > digestPeerInfo.MaxVersion).ToArray()));
            }
        }

        return SendMessage(
            rumorDigestAckMessage.Sender,
            new RumorDigestAck2Message(PeerManager.LocalPeer.Address, fullPeerInfos),
            cancellationToken);
    }
}