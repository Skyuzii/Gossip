namespace Gossip.Core.Abstractions.Peers;

public interface IPeerManager
{
    LocalPeer LocalPeer { get; }

    int ActiveRemotePeersCount { get; }

    IEnumerable<RemotePeer> ActiveRemotePeers { get; }

    IReadOnlyCollection<PeerAddress> UnreachableRemotePeers { get; }

    event Action<RemotePeer> DiscoveredNewPeer;

    event Action<RemotePeer> DeletedStalePeer;

    event Action<PeerAddress> NewUnreachablePeer;

    event Action<PeerAddress> DeletedUnreachablePeer;

    /// <summary>
    /// Return any peer
    /// </summary>
    /// <param name="address"></param>
    /// <param name="peer"></param>
    /// <returns></returns>
    bool TryGet(PeerAddress address, out Peer peer);

    void Add(RemotePeer peer);

    void Unreachable(PeerAddress address);
}