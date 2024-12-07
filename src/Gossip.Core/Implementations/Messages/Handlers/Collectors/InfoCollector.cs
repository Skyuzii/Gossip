using Gossip.Core.Abstractions.Messages.Common;
using Gossip.Core.Abstractions.Peers;
using Gossip.Core.Abstractions.Peers.Rumors;

namespace Gossip.Core.Implementations.Messages.Handlers.Collectors;

internal sealed class InfoCollector : IInfoCollector
{
    private readonly IPeerManager _peerManager;

    public InfoCollector(IPeerManager peerManager)
    {
        _peerManager = peerManager;
    }

    public (IReadOnlyCollection<DigestPeerInfo>, IReadOnlyCollection<FullPeerInfo>) Collect(IReadOnlyCollection<DigestPeerInfo> digestPeerInfos, bool fillDigestPeerInfos = true)
    {
        var newDigestPeerInfos = new List<DigestPeerInfo>();
        var fullPeerInfos = new List<FullPeerInfo>();

        foreach (DigestPeerInfo digestPeerInfo in digestPeerInfos)
        {
            if (!_peerManager.TryGet(digestPeerInfo.Address, out Peer existPeer) || digestPeerInfo.Generation > existPeer.Generation)
            {
                if (fillDigestPeerInfos)
                {
                    newDigestPeerInfos.Add(digestPeerInfo with { Generation = PeerGeneration.Empty(), MaxVersion = RumorVersion.Empty() });
                }

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

            if (fillDigestPeerInfos && digestPeerInfo.MaxVersion > existPeerMaxVersion)
            {
                newDigestPeerInfos.Add(digestPeerInfo with { MaxVersion = existPeerMaxVersion });

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

        return (newDigestPeerInfos, fullPeerInfos);
    }
}