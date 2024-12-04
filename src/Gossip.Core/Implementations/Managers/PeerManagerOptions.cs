namespace Gossip.Core.Implementations.Managers;

public sealed class PeerManagerOptions
{
    public int ActiveRemotePeersCapacity { get; }

    public PeerManagerOptions(int activeRemotePeersCapacity)
    {
        ActiveRemotePeersCapacity = activeRemotePeersCapacity;
    }
}