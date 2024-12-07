using System.Collections.Concurrent;

using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Implementations.Managers;

internal sealed class PeerManager : IPeerManager
{
    private readonly PeerManagerOptions _options;
    private readonly ConcurrentDictionary<PeerAddress, Peer> _activeRemotePeers;

    public PeerManager(
        Peer localPeer,
        PeerManagerOptions options,
        IReadOnlyCollection<Peer> activeRemoteStartingPeers)
    {
        _options = options;
        LocalPeer = localPeer;
        _activeRemotePeers = new ConcurrentDictionary<PeerAddress, Peer>(activeRemoteStartingPeers.ToDictionary(x => x.Address, y => y));
    }

    public Peer LocalPeer { get; }

    public int ActiveRemotePeersCount => _activeRemotePeers.Count;

    public IEnumerable<Peer> ActiveRemotePeers => _activeRemotePeers.Values;

    public bool TryGet(PeerAddress address, out Peer peer)
    {
        peer = default;

        if (LocalPeer.Address == address)
        {
            peer = LocalPeer;

            return true;
        }

        if (_activeRemotePeers.TryGetValue(address, out Peer? existRemotePeer))
        {
            peer = existRemotePeer;

            return true;
        }

        return false;
    }

    public void Add(Peer peer)
    {
        DeleteStaleActivePeerIfNeed();

        _activeRemotePeers.TryAdd(peer.Address, peer);
    }

    public void Unreachable(PeerAddress address)
    {
        _activeRemotePeers.TryRemove(address, out _);
    }

    private void DeleteStaleActivePeerIfNeed()
    {
        if (_activeRemotePeers.Count < _options.ActiveRemotePeersCapacity)
        {
            return;
        }

        Peer? remotePeer = _activeRemotePeers.Values.MinBy(x => x.RumorsUpdatedAt);

        if (remotePeer is not null)
        {
            _activeRemotePeers.Remove(remotePeer.Address, out _);
        }
    }
}