using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Registrars;

public sealed class GossiperConfig
{
    public PeerAddress LocalPeerAddress { get; }

    public int ActiveRemotePeersCapacity { get; }

    public int UnreachableRemotePeersCapacity { get; }

    public int MessageDispatcherCapacity { get; }

    public int SyncDigestInMs { get; }

    public IReadOnlyCollection<PeerAddress> RemoteStartingPeerAddresses { get; }

    internal IReceiversPicker? ReceiversPicker { get; }

    internal Func<IServiceProvider, IMessageSender> MessageSender { get; }

    public GossiperConfig(
        PeerAddress localPeerAddress,
        int activeRemotePeersCapacity,
        int unreachableRemotePeersCapacity,
        int messageDispatcherCapacity,
        int syncDigestInMs,
        IReadOnlyCollection<PeerAddress> remoteStartingPeerAddresses,
        IReceiversPicker? receiversPicker,
        Func<IServiceProvider, IMessageSender> messageSender)
    {
        LocalPeerAddress = localPeerAddress;
        ActiveRemotePeersCapacity = activeRemotePeersCapacity;
        UnreachableRemotePeersCapacity = unreachableRemotePeersCapacity;
        MessageDispatcherCapacity = messageDispatcherCapacity;
        SyncDigestInMs = syncDigestInMs;
        RemoteStartingPeerAddresses = remoteStartingPeerAddresses;
        ReceiversPicker = receiversPicker;
        MessageSender = messageSender;
    }
}