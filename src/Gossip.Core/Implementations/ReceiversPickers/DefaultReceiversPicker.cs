using System.Collections.Concurrent;

using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Implementations.ReceiversPickers;

public sealed class DefaultReceiversPicker : IReceiversPicker
{
    private readonly IPeerManager _peerManager;
    private readonly ConcurrentDictionary<PeerAddress, (bool isStarting, int count)> _counter;

    public DefaultReceiversPicker(IPeerManager peerManager)
    {
        _peerManager = peerManager;
        _counter = new ConcurrentDictionary<PeerAddress, (bool isStarting, int count)>();
    }

    public IReadOnlyCollection<PeerAddress> Pick()
    {
        // todo: add optimization on finding starting peer
        var receivers = new List<PeerAddress>(capacity: 3);

        RemotePeer[] activeRemotePeers = _peerManager.ActiveRemotePeers.ToArray();

        if (TryGetRemoteStartingPeer(activeRemotePeers, out PeerAddress remoteStartingPeerAddress))
        {
            receivers.Add(remoteStartingPeerAddress);
        }

        if (TryGetRemotePeer(activeRemotePeers, out PeerAddress remotePeerAddress))
        {
            receivers.Add(remotePeerAddress);
        }

        if (TryGetRandomUnreachableRemotePeer(out PeerAddress unreachableRemotePeerAddress))
        {
            receivers.Add(unreachableRemotePeerAddress);
        }

        return receivers;
    }

    private bool TryGetRemoteStartingPeer(RemotePeer[] activeRemotePeers, out PeerAddress address)
    {
        address = default;

        RemotePeer[] remoteStartingPeers = activeRemotePeers.Where(remotePeer => remotePeer.IsStarting).ToArray();

        address = remoteStartingPeers.FirstOrDefault(remotePeer => !_counter.ContainsKey(remotePeer.Address))?.Address
            ?? _counter.OrderBy(x => x.Value.count).FirstOrDefault().Key;

        _counter.AddOrUpdate(address, _ => (isStarting: true, count: 1), (_, tuple) => (tuple.isStarting, tuple.count + 1));

        return !address.IsEmpty;
    }

    private bool TryGetRemotePeer(RemotePeer[] activeRemotePeers, out PeerAddress address)
    {
        address = default;

        RemotePeer[] remotePeers = activeRemotePeers.Where(remotePeer => !remotePeer.IsStarting).ToArray();

        address = remotePeers.FirstOrDefault(remotePeer => !_counter.ContainsKey(remotePeer.Address))?.Address
            ?? _counter.OrderBy(x => x.Value.count).FirstOrDefault().Key;

        _counter.AddOrUpdate(address, _ => (isStarting: false, count: 1), (_, tuple) => (tuple.isStarting, tuple.count + 1));

        return !address.IsEmpty;
    }

    private bool TryGetRandomUnreachableRemotePeer(out PeerAddress address)
    {
        PeerAddress[] unreachableRemotePeers = _peerManager.UnreachableRemotePeers.ToArray();

        if (unreachableRemotePeers.Length > 0)
        {
            address = unreachableRemotePeers[Random.Shared.Next(minValue: 0, unreachableRemotePeers.Length)];

            return true;
        }

        address = default;
        return false;
    }
}