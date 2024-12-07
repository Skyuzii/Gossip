using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Messages.RumorDigest;
using Gossip.Core.Abstractions.Messages.RumorDigestAck;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;

using Microsoft.Extensions.Logging;

namespace Gossip.Core.Implementations.Messages.Handlers;

public sealed class RumorDigestMessageHandler : BaseMessageHandler<RumorDigestMessage>
{
    public RumorDigestMessageHandler(
        IMessageSender messageSender,
        IPeerManager peerManager,
        ILogger logger) : base(MessageType.RumorDigest, messageSender, peerManager, logger)
    {
    }

    protected override Task HandleInternal(RumorDigestMessage message, CancellationToken cancellationToken)
    {
        var digestPeerInfos = new List<DigestPeerInfo>();
        var fullPeerInfos = new List<FullPeerInfo>();

        foreach (DigestPeerInfo digestPeerInfo in message.DigestPeerInfos)
        {
            if (!PeerManager.TryGet(digestPeerInfo.Address, out Peer existPeer) || digestPeerInfo.Generation > existPeer.Generation)
            {
                digestPeerInfos.Add(digestPeerInfo with { Generation = PeerGeneration.Empty(), MaxVersion = RumorVersion.Empty() });

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
                        existPeer.Generation,
                        existPeer.Rumors.Values.Where(x => x.Version > digestPeerInfo.MaxVersion).ToArray()));
            }
        }

        return SendMessage(
            message.Sender,
            new RumorDigestAckMessage(PeerManager.LocalPeer.Address, digestPeerInfos, fullPeerInfos),
            cancellationToken);
    }
}