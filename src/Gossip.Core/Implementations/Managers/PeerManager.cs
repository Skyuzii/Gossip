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

    public event Action<RemotePeer>? DiscoveredNewPeer;

    public event Action<RemotePeer>? DeletedStalePeer;

    public event Action<PeerAddress>? NewUnreachablePeer;

    public event Action<PeerAddress>? DeletedUnreachablePeer;

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

        _unreachableRemotePeers.Remove(peer.Address);

        if (_activeRemotePeers.TryAdd(peer.Address, peer))
        {
            DiscoveredNewPeer?.Invoke(peer);
        }
    }

    public void Unreachable(PeerAddress address)
    {
        DeleteStaleUnreachablePeerIfNeed();

        _activeRemotePeers.TryRemove(address, out _);

        if (_unreachableRemotePeers.Add(address))
        {
            NewUnreachablePeer?.Invoke(address);
        }
    }

    private void DeleteStaleActivePeerIfNeed()
    {
        if (_activeRemotePeers.Count < _options.ActiveRemotePeersCapacity)
        {
            return;
        }

        RemotePeer? remotePeer = _activeRemotePeers.Values.Where(x => !x.IsStarting).MinBy(x => x.RumorsUpdatedAt);

        if (remotePeer is not null && _activeRemotePeers.TryRemove(remotePeer.Address, out _))
        {
            DeletedStalePeer?.Invoke(remotePeer);
        }
    }

    private void DeleteStaleUnreachablePeerIfNeed()
    {
        if (_unreachableRemotePeers.Count < _options.UnreachableRemotePeersCapacity)
        {
            return;
        }

        PeerAddress address = _unreachableRemotePeers.LastOrDefault();

        if (!address.IsEmpty && _unreachableRemotePeers.Remove(address))
        {
            DeletedUnreachablePeer?.Invoke(address);
        }
    }
}