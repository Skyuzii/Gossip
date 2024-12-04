namespace Gossip.Core.Implementations.Gossipers;

public sealed class GossiperOptions
{
    public int SyncDigestInMs { get; set; }

    public GossiperOptions(int syncDigestInMs)
    {
        SyncDigestInMs = syncDigestInMs;
    }
}