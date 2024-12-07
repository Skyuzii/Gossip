using System.Collections.Concurrent;

using Gossip.Core.Abstractions.Peers.Rumors;

namespace Gossip.Core.Abstractions.Peers;

public sealed class RemotePeer : Peer
{
    public bool IsStarting { get; }

    private RemotePeer(
        PeerAddress address,
        PeerGeneration generation,
        bool isStarting,
        IReadOnlyCollection<Rumor> rumors) : base(address, generation, rumors)
    {
        IsStarting = isStarting;
    }

    public static RemotePeer New(PeerAddress address, PeerGeneration generation, IReadOnlyCollection<Rumor> rumors)
    {
        return new RemotePeer(
            address,
            generation,
            isStarting: false,
            rumors);
    }

    public static RemotePeer NewStarting(PeerAddress address)
    {
        return new RemotePeer(
            address,
            PeerGeneration.Empty(),
            isStarting: true,
            Array.Empty<Rumor>());
    }

    /// <summary>
    /// Apply rumors
    /// Add if none exists, or update if version is greater
    /// </summary>
    /// <param name="generation"></param>
    /// <param name="rumors"></param>
    public void Apply(PeerGeneration generation, IReadOnlyCollection<Rumor> rumors)
    {
        if (generation < Generation)
        {
            return;
        }

        if (!generation.Equals(Generation))
        {
            Generation = generation;
            RumorsProtected = new ConcurrentDictionary<RumorName, Rumor>();
        }

        foreach (Rumor? rumor in rumors)
        {
            Apply(rumor);
        }
    }
}