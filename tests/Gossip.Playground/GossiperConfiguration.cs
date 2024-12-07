namespace Gossip.Playground;

public sealed class GossiperConfiguration
{
    public string LocalPeer { get; set; }

    public string[] RemoteStartingPeerAddresses { get; set; } = Array.Empty<string>();
}