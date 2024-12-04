namespace Gossip.Playground;

public class GossiperConfiguration
{
    public string LocalPeer { get; set; }
 
    public string[] RemoteStartingPeerAddresses { get; set; } = Array.Empty<string>();
}