using System.Collections.Concurrent;

using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Implementations.Managers;

internal sealed class PeerManager : IPeerManager
{
    private readonly PeerManagerOptions _options;
    private readonly ConcurrentDictionary<PeerAddress, RemotePeer> _activeRemotePeers;
    private readonly HashSet<PeerAddress> _unreachableRemotePeers;

    public PeerManager(
        LocalPeer localPeer,
        PeerManagerOptions options,
        IReadOnlyCollection<RemotePeer> activeRemoteStartingPeers)
    {
        _options = options;
        LocalPeer = localPeer;
        _unreachableRemotePeers = new HashSet<PeerAddress>();
        _activeRemotePeers = new ConcurrentDictionary<PeerAddress, RemotePeer>(activeRemoteStartingPeers.ToDictionary(x => x.Address, y => y));
    }

    public LocalPeer LocalPeer { get; }

    public int ActiveRemotePeersCount => _activeRemotePeers.Count;

    public IEnumerable<RemotePeer> ActiveRemotePeers => _activeRemotePeers.Values;

    public IReadOnlyCollection<PeerAddress> UnreachableRemotePeers => _unreachableRemotePeers;

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
        _unreachableRemotePeers.Remove(peer.Address);
    }

    public void Unreachable(PeerAddress address)
    {
        DeleteStaleUnreachablePeerIfNeed();

        _activeRemotePeers.TryRemove(address, out _);
        _unreachableRemotePeers.Add(address);
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

    private void DeleteStaleUnreachablePeerIfNeed()
    {
        if (_unreachableRemotePeers.Count < _options.UnreachableRemotePeersCapacity)
        {
            return;
        }

        PeerAddress? address = _unreachableRemotePeers.LastOrDefault();

        if (address is not null)
        {
            _unreachableRemotePeers.Remove(address.Value);
        }
    }
}