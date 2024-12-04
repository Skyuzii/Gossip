namespace Gossip.Core.Abstractions.Peers;

public interface IPeerManager
{
    // todo: add events on disconnect and connect peers
    Peer LocalPeer { get; }

    int ActiveRemotePeersCount { get; }

    IEnumerable<Peer> ActiveRemotePeers { get; }

    /// <summary>
    /// Return peer
    /// Can return local peer
    /// </summary>
    /// <param name="address"></param>
    /// <param name="peer"></param>
    /// <returns></returns>
    bool TryGet(PeerAddress address, out Peer peer);

    void Add(Peer peer);
}