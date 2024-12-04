using System.Net;

using Gossip.Core.Abstractions.Messages;
using Gossip.Core.Abstractions.Peers;

namespace Gossip.Core.Registrars;

public sealed class GossiperConfigBuilder
{
    private PeerAddress? _localPeerAddress;
    private int? _activeRemotePeersCapacity;
    private int? _messageDispatcherCapacity;
    private int? _syncDigestInMs;
    private PeerAddress[]? _remoteStartingPeerAddresses;
    private IReceiversPicker? _receiversPicker;
    private Func<IServiceProvider, IMessageSender>? _messageSender;

    public GossiperConfigBuilder SetLocalPeerAddress(PeerAddress localPeerAddress)
    {
        _localPeerAddress = localPeerAddress;

        return this;
    }

    public GossiperConfigBuilder SetActiveRemotePeersCapacity(int activeRemotePeersCapacity)
    {
        _activeRemotePeersCapacity = activeRemotePeersCapacity;

        return this;
    }

    public GossiperConfigBuilder SetMessageDispatcherCapacity(int messageDispatcherCapacity)
    {
        _messageDispatcherCapacity = messageDispatcherCapacity;

        return this;
    }

    public GossiperConfigBuilder SetSyncDigestInMs(int syncDigestInMs)
    {
        _syncDigestInMs = syncDigestInMs;

        return this;
    }

    public GossiperConfigBuilder SetRemoteStartingPeerAddresses(PeerAddress[] remoteStartingPeerAddresses)
    {
        _remoteStartingPeerAddresses = remoteStartingPeerAddresses;

        return this;
    }

    public GossiperConfigBuilder SetReceiversPicker(IReceiversPicker receiversPicker)
    {
        _receiversPicker = receiversPicker;

        return this;
    }

    public GossiperConfigBuilder SetMessageSender(Func<IServiceProvider, IMessageSender> messageSender)
    {
        _messageSender = messageSender;

        return this;
    }

    internal GossiperConfig Build()
    {
        if (_messageSender is null)
        {
            throw new ArgumentNullException("MessageSender", "must be filled");
        }

        return new GossiperConfig(
            _localPeerAddress ?? new PeerAddress(new Uri(IPAddress.Loopback.ToString())),
            _activeRemotePeersCapacity ?? 100,
            _messageDispatcherCapacity ?? 1000,
            _syncDigestInMs ?? 1000,
            _remoteStartingPeerAddresses ?? Array.Empty<PeerAddress>(),
            _receiversPicker,
            _messageSender);
    }
}