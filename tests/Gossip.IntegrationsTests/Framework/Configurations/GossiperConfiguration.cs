namespace Gossip.IntegrationsTests.Framework.Configurations;

public sealed class GossiperConfiguration
{
    public string LocalPeer { get; set; }

    public string[] RemoteStartingPeerAddresses { get; set; } = Array.Empty<string>();

    public int SyncDigestInMs { get; set; } = 100;

    public int ActiveRemotePeersCapacity { get; set; } = 1000;

    public int UnreachableRemotePeersCapacity { get; set; } = 1000;

    public int MessageDispatcherCapacity { get; set; } = 10000;
}