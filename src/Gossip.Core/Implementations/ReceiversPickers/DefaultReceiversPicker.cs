using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Implementations.ReceiversPickers;

public sealed class DefaultReceiversPicker : IReceiversPicker
{
    private readonly IPeerManager _peerManager;

    public DefaultReceiversPicker(IPeerManager peerManager)
    {
        _peerManager = peerManager;
    }

    public IReadOnlyCollection<PeerAddress> Pick()
    {
        // todo: add optimization on finding starting peer
        var receivers = new List<PeerAddress>(capacity: 3);

        RemotePeer[] activeRemotePeers = _peerManager.ActiveRemotePeers.ToArray();
        IReadOnlyCollection<PeerAddress> unreachableRemotePeers = _peerManager.UnreachableRemotePeers;

        Peer? firstReceiver = activeRemotePeers.FirstOrDefault(x => x.IsStarting);
        Peer? secondReceiver = activeRemotePeers.FirstOrDefault(x => !x.IsStarting);

        if (firstReceiver is not null)
        {
            receivers.Add(firstReceiver.Address);
        }

        if (secondReceiver is not null)
        {
            receivers.Add(secondReceiver.Address);
        }

        if (unreachableRemotePeers.Count > 0)
        {
            receivers.Add(unreachableRemotePeers.Skip(Random.Shared.Next(minValue: 0, unreachableRemotePeers.Count - 1)).First());
        }

        return receivers;
    }
}