using System.Collections.Concurrent;

using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Implementations.Managers;

internal sealed class PeerManager : IPeerManager
{
    private readonly PeerManagerOptions _options;
    private readonly ConcurrentDictionary<PeerAddress, RemotePeer> _activeRemotePeers;

    public PeerManager(
        LocalPeer localPeer,
        PeerManagerOptions options,
        IReadOnlyCollection<RemotePeer> activeRemoteStartingPeers)
    {
        _options = options;
        LocalPeer = localPeer;
        _activeRemotePeers = new ConcurrentDictionary<PeerAddress, RemotePeer>(activeRemoteStartingPeers.ToDictionary(x => x.Address, y => y));
    }

    public LocalPeer LocalPeer { get; }

    public int ActiveRemotePeersCount => _activeRemotePeers.Count;

    public IEnumerable<RemotePeer> ActiveRemotePeers => _activeRemotePeers.Values;

    public bool TryGet(PeerAddress address, out Peer peer)
    {
        peer = default;

        if (LocalPeer.Address == address)
        {
            peer = LocalPeer;

            return true;
        }

        if (_activeRemotePeers.TryGetValue(address, out RemotePeer? existRemotePeer))
        {
            peer = existRemotePeer;

            return true;
        }

        return false;
    }

    public void Add(RemotePeer peer)
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