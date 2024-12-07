namespace Gossip.Core.Abstractions.Peers;

public interface IPeerManager
{
    Peer LocalPeer { get; }

    int ActiveRemotePeersCount { get; }

    IEnumerable<Peer> ActiveRemotePeers { get; }

    /// <summary>
    /// Return any peer
    /// </summary>
    /// <param name="address"></param>
    /// <param name="peer"></param>
    /// <returns></returns>
    bool TryGet(PeerAddress address, out Peer peer);

    void Add(Peer peer);

    void Unreachable(PeerAddress address);
}