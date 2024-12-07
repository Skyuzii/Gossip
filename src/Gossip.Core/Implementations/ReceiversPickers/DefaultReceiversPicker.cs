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
        var receivers = new List<PeerAddress>(capacity: 2);

        RemotePeer[] activeRemotePeers = _peerManager.ActiveRemotePeers.ToArray();

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

        return receivers;
    }
}