namespace Gossip.IntegrationsTests.Framework.Configurations;

public sealed class GossiperConfiguration
{
    public string LocalPeer { get; set; }

    public string[] RemoteStartingPeerAddresses { get; set; } = Array.Empty<string>();

    public int SyncDigestInMs { get; set; } = 1;
}