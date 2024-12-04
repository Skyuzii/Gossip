using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Registrars;

public sealed class GossiperConfig
{
    public PeerAddress LocalPeerAddress { get; }

    public int ActiveRemotePeersCapacity { get; } = 100;

    public int MessageDispatcherCapacity { get; } = 1000;

    public int SyncDigestInMs { get; } = 1000;

    public IReadOnlyCollection<PeerAddress> RemoteStartingPeerAddresses { get; }

    internal IReceiversPicker? ReceiversPicker { get; }

    internal Func<IServiceProvider, IMessageSender> MessageSender { get; }

    public GossiperConfig(
        PeerAddress localPeerAddress,
        int activeRemotePeersCapacity,
        int messageDispatcherCapacity,
        int syncDigestInMs,
        IReadOnlyCollection<PeerAddress> remoteStartingPeerAddresses,
        IReceiversPicker? receiversPicker,
        Func<IServiceProvider, IMessageSender> messageSender)
    {
        LocalPeerAddress = localPeerAddress;
        ActiveRemotePeersCapacity = activeRemotePeersCapacity;
        MessageDispatcherCapacity = messageDispatcherCapacity;
        SyncDigestInMs = syncDigestInMs;
        RemoteStartingPeerAddresses = remoteStartingPeerAddresses;
        ReceiversPicker = receiversPicker;
        MessageSender = messageSender;
    }
}