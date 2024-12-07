namespace Gossip.Core.Implementations.Managers;

public sealed class PeerManagerOptions
{
    public int ActiveRemotePeersCapacity { get; }

    public int UnreachableRemotePeersCapacity { get; }

    public PeerManagerOptions(int activeRemotePeersCapacity, int unreachableRemotePeersCapacity)
    {
        ActiveRemotePeersCapacity = activeRemotePeersCapacity;
        UnreachableRemotePeersCapacity = unreachableRemotePeersCapacity;
    }
}